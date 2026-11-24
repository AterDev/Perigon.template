using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Perigon.AspNetCore.SourceGeneration.Build;

public sealed class GenerateEnumMetadata : Task
{
    [Required] public string OutputPath { get; set; } = string.Empty;
    [Required] public string AssemblyName { get; set; } = string.Empty;
    public string DefineConstants { get; set; } = string.Empty;
    public string LangVersion { get; set; } = string.Empty;
    [Required] public ITaskItem[] Compile { get; set; } = Array.Empty<ITaskItem>();
    public ITaskItem[] References { get; set; } = Array.Empty<ITaskItem>();

    public override bool Execute()
    {
        try
        {
            var parseOptions = new CSharpParseOptions(
                ParseLanguageVersion(LangVersion),
                preprocessorSymbols: DefineConstants
                    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(symbol => symbol.Trim())
                    .Where(symbol => symbol.Length > 0));
            var trees = Compile
                .Select(item => item.ItemSpec)
                .Where(File.Exists)
                .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parseOptions, path))
                .ToArray();
            var references = References
                .Select(item => item.ItemSpec)
                .Where(File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(path => path, path => (MetadataReference)MetadataReference.CreateFromFile(path), StringComparer.OrdinalIgnoreCase);
            var compilation = CSharpCompilation.Create(
                AssemblyName,
                trees,
                references.Values,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var enums = new List<EnumInfo>();
            CollectNamespace(compilation.Assembly.GlobalNamespace, AssemblyName, enums);
            foreach (var reference in References.Where(IsProjectReference))
            {
                if (!references.TryGetValue(reference.ItemSpec, out var metadataReference)) continue;
                if (compilation.GetAssemblyOrModuleSymbol(metadataReference) is IAssemblySymbol assembly)
                    CollectNamespace(assembly.GlobalNamespace, assembly.Name, enums);
            }

            WriteXml(enums, OutputPath);
            return true;
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(exception, true);
            return false;
        }
    }

    private static LanguageVersion ParseLanguageVersion(string value)
        => LanguageVersionFacts.TryParse(value, out var version) ? version : LanguageVersion.Latest;

    private static bool IsProjectReference(ITaskItem item)
        => string.Equals(item.GetMetadata("ReferenceSourceTarget"), "ProjectReference", StringComparison.OrdinalIgnoreCase);

    private static void CollectNamespace(INamespaceSymbol space, string assemblyName, List<EnumInfo> result)
    {
        foreach (var type in space.GetTypeMembers()) CollectType(type, assemblyName, result);
        foreach (var child in space.GetNamespaceMembers()) CollectNamespace(child, assemblyName, result);
    }

    private static void CollectType(INamedTypeSymbol type, string assemblyName, List<EnumInfo> result)
    {
        if (type.TypeKind == TypeKind.Enum)
        {
            var members = type.GetMembers().OfType<IFieldSymbol>()
                .Where(field => field.IsConst)
                .Select(field => field.HasConstantValue ? field : throw new InvalidOperationException(
                    $"Cannot resolve enum value {type.ToDisplayString()}.{field.Name}."))
                .Select(field => new EnumMemberInfo(
                    field.Name,
                    Convert.ToString(field.ConstantValue, CultureInfo.InvariantCulture) ?? string.Empty,
                    GetDescription(field)))
                .ToArray();
            result.Add(new EnumInfo(assemblyName, GetRuntimeFullName(type), type.EnumUnderlyingType?.ToDisplayString() ?? "int", type.GetAttributes().Any(IsFlags), members));
            return;
        }
        foreach (var child in type.GetTypeMembers()) CollectType(child, assemblyName, result);
    }

    private static bool IsFlags(AttributeData attribute)
        => attribute.AttributeClass?.ToDisplayString() == "System.FlagsAttribute";

    private static string GetDescription(IFieldSymbol field)
    {
        var attribute = field.GetAttributes().FirstOrDefault(item =>
            item.AttributeClass?.ToDisplayString() == "System.ComponentModel.DescriptionAttribute");
        return attribute?.ConstructorArguments.FirstOrDefault().Value as string ?? field.Name;
    }

    private static string GetRuntimeFullName(INamedTypeSymbol type)
    {
        var names = new Stack<string>();
        for (var current = type; current is not null; current = current.ContainingType)
            names.Push(current.MetadataName);
        var name = string.Join("+", names);
        var namespaces = new Stack<string>();
        for (var ns = type.ContainingNamespace; ns is not null && !ns.IsGlobalNamespace; ns = ns.ContainingNamespace)
            namespaces.Push(ns.MetadataName);
        return namespaces.Count == 0 ? name : string.Join(".", namespaces) + "." + name;
    }

    private static void WriteXml(IEnumerable<EnumInfo> values, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
        using var buffer = new MemoryStream();
        using var writer = XmlWriter.Create(buffer, settings);
        writer.WriteStartElement("enums");
        foreach (var value in values.OrderBy(item => item.AssemblyName, StringComparer.Ordinal).ThenBy(item => item.Type, StringComparer.Ordinal))
        {
            writer.WriteStartElement("enum");
            writer.WriteAttributeString("assembly", value.AssemblyName);
            writer.WriteAttributeString("type", value.Type);
            writer.WriteAttributeString("underlyingType", value.UnderlyingType);
            writer.WriteAttributeString("flags", value.IsFlags ? "true" : "false");
            foreach (var member in value.Members)
            {
                writer.WriteStartElement("member");
                writer.WriteAttributeString("name", member.Name);
                writer.WriteAttributeString("value", member.Value);
                writer.WriteAttributeString("description", member.Description);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.Flush();
        var content = buffer.ToArray();
        if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(content)) File.WriteAllBytes(path, content);
    }

    private sealed class EnumInfo
    {
        public EnumInfo(string assemblyName, string type, string underlyingType, bool isFlags, IReadOnlyList<EnumMemberInfo> members)
        {
            AssemblyName = assemblyName;
            Type = type;
            UnderlyingType = underlyingType;
            IsFlags = isFlags;
            Members = members;
        }

        public string AssemblyName { get; }
        public string Type { get; }
        public string UnderlyingType { get; }
        public bool IsFlags { get; }
        public IReadOnlyList<EnumMemberInfo> Members { get; }
    }

    private sealed class EnumMemberInfo
    {
        public EnumMemberInfo(string name, string value, string description)
        {
            Name = name;
            Value = value;
            Description = description;
        }

        public string Name { get; }
        public string Value { get; }
        public string Description { get; }
    }
}
