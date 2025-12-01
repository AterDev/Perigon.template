using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Reflection;

namespace MigrationService.DesignTime
{
    internal class MigrationsModelDifferProxy : DispatchProxy
    {
        private object _inner = null!;

        public static T Create<T>(object inner) where T : class
        {
            var proxy = DispatchProxy.Create<T, MigrationsModelDifferProxy>() as MigrationsModelDifferProxy;
            if (proxy == null) throw new InvalidOperationException("Unable to create proxy");
            proxy._inner = inner ?? throw new ArgumentNullException(nameof(inner));
            return proxy as T ?? throw new InvalidOperationException("Could not cast proxy to target interface");
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null) throw new ArgumentNullException(nameof(targetMethod));

            try
            {
                var result = targetMethod.Invoke(_inner, args);

                if (result == null) return null;

                if (result is IEnumerable<MigrationOperation> enumOps && !(result is string))
                {
                    var list = enumOps.ToList();
                    var filtered = FilterOperations(list);

                    // match return type
                    var returnType = targetMethod.ReturnType;
                    if (returnType.IsAssignableFrom(filtered.GetType()))
                        return filtered;
                    if (returnType.IsArray) return filtered.ToArray();
                    return filtered;
                }

                return result;
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException ?? tie;
            }
        }

        private IReadOnlyList<MigrationOperation> FilterOperations(IReadOnlyList<MigrationOperation> ops)
        {
            try
            {
                var list = ops.ToList();
                var tenantSignatures = new HashSet<string>();
                foreach (var ci in list.OfType<CreateIndexOperation>())
                {
                    if (ci.Columns != null && ci.Columns.Length > 0 && string.Equals(ci.Columns[0], "TenantId", StringComparison.Ordinal))
                    {
                        var trailing = string.Join("|", ci.Columns.Skip(1));
                        var key = ci.Table + "::" + trailing;
                        tenantSignatures.Add(key);
                    }
                }

                var dbEnv = Environment.GetEnvironmentVariable("Components__Database") ?? Environment.GetEnvironmentVariable("Components:Database") ?? string.Empty;
                dbEnv = dbEnv?.ToLowerInvariant() ?? string.Empty;
                string uniqueFilter = dbEnv.Contains("postgres") || dbEnv.Contains("npgsql") ? "\"IsDeleted\" = false"
                    : dbEnv.Contains("sqlserver") || dbEnv.Contains("mssql") ? "[IsDeleted] = 0"
                    : "`IsDeleted` = 0";

                var toRemove = new List<CreateIndexOperation>();
                var toAdd = new List<(int Index, CreateIndexOperation NewOp)>();

                for (int idx = 0; idx < list.Count; idx++)
                {
                    var op = list[idx];
                    if (op is CreateIndexOperation ci)
                    {
                        if (ci.Columns == null || ci.Columns.Length == 0) continue;

                        if (string.Equals(ci.Columns[0], "TenantId", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var trailing = string.Join("|", ci.Columns);
                        var signature = ci.Table + "::" + trailing;

                        if (tenantSignatures.Contains(signature))
                        {
                            toRemove.Add(ci);
                            TryLog($"Removing duplicate non-tenant index on {ci.Table} ({string.Join(',', ci.Columns)}) because tenant-aware exists");
                            continue;
                        }

                        var newCols = new string[ci.Columns.Length + 1];
                        newCols[0] = "TenantId";
                        Array.Copy(ci.Columns, 0, newCols, 1, ci.Columns.Length);

                        var newCi = new CreateIndexOperation
                        {
                            Name = ci.Name,
                            Table = ci.Table,
                            Schema = ci.Schema,
                            Columns = newCols,
                            IsUnique = ci.IsUnique,
                            Filter = ci.IsUnique ? uniqueFilter : ci.Filter,
                        };

                        try
                        {
                            foreach (var ann in ci.GetAnnotations())
                            {
                                newCi.SetAnnotation(ann.Name, ann.Value);
                            }
                        }
                        catch { }

                        toAdd.Add((idx, newCi));
                        toRemove.Add(ci);
                        TryLog($"Replaced index on {ci.Table} ({string.Join(',', ci.Columns)}) with tenant-aware index ({string.Join(',', newCols)})");
                    }
                }

                foreach (var r in toRemove)
                {
                    list.Remove(r);
                }

                foreach (var item in toAdd.OrderBy(i => i.Index))
                {
                    var insertPos = Math.Min(item.Index, list.Count);
                    list.Insert(insertPos, item.NewOp);
                }

                return list;
            }
            catch (Exception ex)
            {
                TryLog("FilterOperations exception: " + ex);
                return ops;
            }
        }

        private void TryLog(string message)
        {
            try { File.AppendAllText(Path.Combine(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory(), "ef_migdiffer_proxy.log"), $"[{DateTime.UtcNow:O}] {message}\n"); } catch { }
        }
    }
}
