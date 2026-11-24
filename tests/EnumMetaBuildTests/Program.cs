using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

if (args.Length == 0)
    throw new ArgumentException("Pass the local Perigon.AspNetCore.SourceGeneration nupkg path.");

var package = Path.GetFullPath(args[0]);
if (!File.Exists(package))
    throw new FileNotFoundException("The source-generation package was not found.", package);

var version = ReadPackageVersion(package);
var feed = Path.GetDirectoryName(package)!;
var root = Path.Combine(Path.GetTempPath(), $"Perigon-EnumMetaBuildTests-{Guid.NewGuid():N}");
Directory.CreateDirectory(root);
try
{
    WriteProjectFiles(root, version);
    RunDotnet(root, $"restore Web/Web.csproj --source \"{feed}\" --ignore-failed-sources");
    RunDotnet(root, "build Web/Web.csproj -c Release --no-restore");

    var metadataFiles = Directory.GetFiles(root, "enum-meta.generated.xml", SearchOption.AllDirectories);
    if (metadataFiles.Length == 0)
        throw new InvalidOperationException("The Web build did not produce enum-meta.generated.xml.");

    var documents = metadataFiles.Select(path => XDocument.Load(path)).ToArray();
    foreach (var document in documents)
        AssertMetadata(document);

    foreach (var targetFramework in new[] { "net10.0", "net11.0" })
    {
        var assemblyPath = Path.Combine(root, "Web", "bin", "Release", targetFramework, "Web.dll");
        // Load from bytes so Windows can remove the temporary fixture after validation.
        var assembly = Assembly.Load(File.ReadAllBytes(assemblyPath));
        var resources = assembly.GetManifestResourceNames();
        var resourceName = resources.SingleOrDefault(name => name.EndsWith("enum-meta.generated.xml", StringComparison.Ordinal));
        if (resourceName is null)
            throw new InvalidOperationException($"The {targetFramework} Web assembly does not embed enum-meta.generated.xml.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The {targetFramework} embedded enum metadata resource could not be opened.");
        var embedded = XDocument.Load(stream);
        AssertMetadata(embedded);
        var external = XDocument.Load(Path.Combine(root, "Web", "bin", "Release", targetFramework, "enum-meta.generated.xml"));
        if (!XNode.DeepEquals(external, embedded))
            throw new InvalidOperationException($"The {targetFramework} external and embedded enum metadata differ.");
    }

    foreach (var targetFramework in new[] { "net10.0", "net11.0" })
    {
        var publishDirectory = Path.Combine(root, "Publish", targetFramework);
        RunDotnet(root, $"publish Web/Web.csproj -c Release -f {targetFramework} --no-restore -o \"{publishDirectory}\"");
        var publishedMetadata = Path.Combine(publishDirectory, "enum-meta.generated.xml");
        if (!File.Exists(publishedMetadata))
            throw new InvalidOperationException($"The {targetFramework} publish root did not contain enum-meta.generated.xml.");
        AssertMetadata(XDocument.Load(publishedMetadata));
    }

    RunSdk10Fixture(root, version, feed);

    // A second build must refresh the XML when a new enum is added; this guards against stale incremental outputs.
    File.AppendAllText(Path.Combine(root, "Web", "Program.cs"), "\npublic enum IncrementalEnum { IncrementalValue = 303 }\n");
    RunDotnet(root, "build Web/Web.csproj -c Release --no-restore");
    foreach (var targetFramework in new[] { "net10.0", "net11.0" })
    {
        var updated = XDocument.Load(Path.Combine(root, "Web", "bin", "Release", targetFramework, "enum-meta.generated.xml"));
        var members = updated.Root!.Elements("enum").SelectMany(element => element.Elements("member"));
        if (!members.Any(member => (string)member.Attribute("name")! == "IncrementalValue" && (string)member.Attribute("value")! == "303"))
            throw new InvalidOperationException($"The {targetFramework} incremental enum metadata was stale.");
    }

    WritePlainProject(root, version);
    RunDotnet(root, "restore Plain/Plain.csproj --source \"" + feed + "\" --ignore-failed-sources");
    RunDotnet(root, "build Plain/Plain.csproj -c Release --no-restore");
    if (Directory.GetFiles(Path.Combine(root, "Plain"), "enum-meta.generated.xml", SearchOption.AllDirectories).Length != 0)
        throw new InvalidOperationException("A non-Web SDK project produced enum-meta.generated.xml.");

    Console.WriteLine("Validated Web enum metadata XML, embedded resource, transitive references, and non-Web exclusion.");
}
finally
{
    Directory.Delete(root, recursive: true);
}

static string ReadPackageVersion(string package)
{
    using var archive = System.IO.Compression.ZipFile.OpenRead(package);
    var nuspec = archive.Entries.Single(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
    using var reader = new StreamReader(nuspec.Open());
    var document = XDocument.Parse(reader.ReadToEnd());
    return document.Descendants().First(element => element.Name.LocalName == "version").Value;
}

static void WriteProjectFiles(string root, string version)
{
    Directory.CreateDirectory(Path.Combine(root, "Web"));
    Directory.CreateDirectory(Path.Combine(root, "DirectEnums"));
    Directory.CreateDirectory(Path.Combine(root, "TransitiveEnums"));
    File.WriteAllText(Path.Combine(root, "Web", "Web.csproj"), $"""
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup><TargetFrameworks>net10.0;net11.0</TargetFrameworks><GenerateDocumentationFile>true</GenerateDocumentationFile></PropertyGroup>
          <ItemGroup><PackageReference Include="Perigon.AspNetCore.SourceGeneration" Version="{version}" /><ProjectReference Include="../DirectEnums/DirectEnums.csproj" /></ItemGroup>
        </Project>
        """);
    File.WriteAllText(Path.Combine(root, "Web", "Program.cs"), "using Microsoft.AspNetCore.Builder; var app = WebApplication.CreateBuilder(args).Build(); app.MapGet(\"/\", () => 1); app.Run();\npublic enum WebEnum { [System.ComponentModel.Description(\"Web \\\"quote\\\" \\\\ slash\")] WebValue = 11 }\n#if DEBUG\npublic enum ConfigurationEnum { DebugOnly = 100 }\n#else\npublic enum ConfigurationEnum { ReleaseOnly = 200 }\n#endif\n");
    File.WriteAllText(Path.Combine(root, "DirectEnums", "DirectEnums.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFrameworks>net10.0;net11.0</TargetFrameworks></PropertyGroup><ItemGroup><ProjectReference Include=\"../TransitiveEnums/TransitiveEnums.csproj\" /></ItemGroup></Project>");
    File.WriteAllText(Path.Combine(root, "DirectEnums", "Class.cs"), "using System.ComponentModel; namespace DirectEnums; public enum DirectEnum { [Description(\"直接\")] Direct = 2 }\n");
    File.WriteAllText(Path.Combine(root, "TransitiveEnums", "TransitiveEnums.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFrameworks>net10.0;net11.0</TargetFrameworks></PropertyGroup></Project>");
    File.WriteAllText(Path.Combine(root, "TransitiveEnums", "Class.cs"), "using System; using System.ComponentModel; namespace TransitiveEnums; public enum TransitiveEnum : long { [Description(\"最大值\")] Max = 9223372036854775807, Alias = 7, Alias2 = 7 } [Flags] public enum FlagEnum { None = 0, Read = 1 } public class Container { public enum NestedEnum { Nested = 9 } }\n");
}

static void WritePlainProject(string root, string version)
{
    var path = Path.Combine(root, "Plain");
    Directory.CreateDirectory(path);
    File.WriteAllText(Path.Combine(path, "Plain.csproj"), $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net11.0</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include=\"Perigon.AspNetCore.SourceGeneration\" Version=\"{version}\" /></ItemGroup></Project>");
    File.WriteAllText(Path.Combine(path, "Class.cs"), "public enum PlainEnum { Value }\n");
}

static void RunSdk10Fixture(string root, string version, string feed)
{
    var fixture = Path.Combine(root, "Sdk10");
    Directory.CreateDirectory(Path.Combine(fixture, "Web"));
    Directory.CreateDirectory(Path.Combine(fixture, "DirectEnums"));
    Directory.CreateDirectory(Path.Combine(fixture, "TransitiveEnums"));
    File.WriteAllText(Path.Combine(fixture, "Web", "Web.csproj"), $"<Project Sdk=\"Microsoft.NET.Sdk.Web\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include=\"Perigon.AspNetCore.SourceGeneration\" Version=\"{version}\" /><ProjectReference Include=\"../DirectEnums/DirectEnums.csproj\" /></ItemGroup></Project>");
    File.WriteAllText(Path.Combine(fixture, "Web", "Program.cs"), "using Microsoft.AspNetCore.Builder; var app = WebApplication.CreateBuilder(args).Build(); app.MapGet(\"/\", () => 1); app.Run();\npublic enum WebEnum { [System.ComponentModel.Description(\"Web \\\"quote\\\" \\\\ slash\")] WebValue = 11 }\n#if DEBUG\npublic enum ConfigurationEnum { DebugOnly = 100 }\n#else\npublic enum ConfigurationEnum { ReleaseOnly = 200 }\n#endif\n");
    File.WriteAllText(Path.Combine(fixture, "DirectEnums", "DirectEnums.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup><ProjectReference Include=\"../TransitiveEnums/TransitiveEnums.csproj\" /></ItemGroup></Project>");
    File.WriteAllText(Path.Combine(fixture, "DirectEnums", "Class.cs"), "using System.ComponentModel; namespace DirectEnums; public enum DirectEnum { [Description(\"直接\")] Direct = 2 }\n");
    File.WriteAllText(Path.Combine(fixture, "TransitiveEnums", "TransitiveEnums.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
    File.WriteAllText(Path.Combine(fixture, "TransitiveEnums", "Class.cs"), "using System; using System.ComponentModel; namespace TransitiveEnums; public enum TransitiveEnum : long { [Description(\"最大值\")] Max = 9223372036854775807, Alias = 7, Alias2 = 7 } [Flags] public enum FlagEnum { None = 0, Read = 1 } public class Container { public enum NestedEnum { Nested = 9 } }\n");

    var globalJson = Path.Combine(fixture, "global.json");
    File.WriteAllText(globalJson, "{\"sdk\":{\"version\":\"10.0.401\",\"rollForward\":\"disable\"}}\n");
    try
    {
        if (RunDotnet(fixture, "--version") != "10.0.401")
            throw new InvalidOperationException("The SDK 10 fixture was not built with SDK 10.0.401.");
        RunDotnet(fixture, $"restore Web/Web.csproj --source \"{feed}\" --ignore-failed-sources");
        RunDotnet(fixture, "build Web/Web.csproj -c Release --no-restore");
        var outputDirectory = Path.Combine(fixture, "Web", "bin", "Release", "net10.0");
        var metadata = Path.Combine(outputDirectory, "enum-meta.generated.xml");
        if (!File.Exists(metadata))
            throw new InvalidOperationException("The SDK 10 Web build did not produce enum-meta.generated.xml.");
        var document = XDocument.Load(metadata);
        AssertMetadata(document);
        var assembly = Assembly.Load(File.ReadAllBytes(Path.Combine(outputDirectory, "Web.dll")));
        var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(name => name.EndsWith("enum-meta.generated.xml", StringComparison.Ordinal));
        if (resourceName is null)
            throw new InvalidOperationException("The SDK 10 Web assembly did not embed enum-meta.generated.xml.");
    }
    finally
    {
        File.Delete(globalJson);
    }
}

static void AssertMetadata(XDocument document)
{
    var root = document.Root ?? throw new InvalidOperationException("enum-meta.generated.xml has no root element.");
    if (root.Name != "enums") throw new InvalidOperationException("enum-meta.generated.xml root must be <enums>.");
    var enums = root.Elements("enum").ToDictionary(element => (string)element.Attribute("assembly")! + ":" + (string)element.Attribute("type")!);
    var allMembers = enums.Values.SelectMany(element => element.Elements("member")).ToArray();
    RequireMember(allMembers, "WebValue", "11", "Web \"quote\" \\ slash");
    RequireMember(allMembers, "Direct", "2", "直接");
    RequireMember(allMembers, "Max", "9223372036854775807", "最大值");
    if (!allMembers.Any(member => (string)member.Attribute("name")! == "Alias" && (string)member.Attribute("value")! == "7") || !allMembers.Any(member => (string)member.Attribute("name")! == "Alias2" && (string)member.Attribute("value")! == "7"))
        throw new InvalidOperationException("Enum aliases were not preserved.");
    if (!allMembers.Any(member => (string)member.Attribute("name")! == "ReleaseOnly" && (string)member.Attribute("value")! == "200") || allMembers.Any(member => (string)member.Attribute("name")! == "DebugOnly"))
        throw new InvalidOperationException("Release/Debug preprocessor symbols were not honored.");
    var flags = enums.Values.SingleOrDefault(element => (string)element.Attribute("type")! == "TransitiveEnums.FlagEnum");
    if (flags is null || !bool.TryParse((string?)flags.Attribute("flags"), out var isFlags) || !isFlags)
        throw new InvalidOperationException("Flags enum metadata is missing its flags marker.");
    var transitive = enums.Values.SingleOrDefault(element => (string)element.Attribute("type")! == "TransitiveEnums.TransitiveEnum");
    if (transitive is null || (string)transitive.Attribute("underlyingType")! != "long")
        throw new InvalidOperationException("The long enum underlying type was not preserved.");
    if (!enums.Values.Any(element => (string)element.Attribute("type")! == "TransitiveEnums.Container+NestedEnum"))
        throw new InvalidOperationException("Nested enum metadata name was not preserved.");
}

static void RequireMember(IEnumerable<XElement> members, string name, string value, string description)
{
    if (!members.Any(member => (string)member.Attribute("name")! == name && (string)member.Attribute("value")! == value && (string)member.Attribute("description")! == description))
        throw new InvalidOperationException($"Missing enum member {name}.");
}

static string RunDotnet(string root, string arguments)
{
    var startInfo = new ProcessStartInfo("dotnet", arguments)
    {
        WorkingDirectory = root,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    // dotnet run may inherit MSBuild's SDK 11 resolver variables. Remove them so a
    // fixture-local global.json can select SDK 10 for the independent compatibility test.
    startInfo.Environment.Remove("MSBUILD_EXE_PATH");
    startInfo.Environment.Remove("MSBuildSDKsPath");
    startInfo.Environment.Remove("MSBUILD_SDKS_PATH");
    startInfo.Environment.Remove("DOTNET_MSBUILD_SDK_RESOLVER_SDKS_DIR");
    startInfo.Environment.Remove("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR");
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start dotnet.");
    var output = process.StandardOutput.ReadToEnd();
    var error = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"dotnet {arguments} failed ({process.ExitCode}).\n{output}\n{error}");
    return output.Trim();
}
