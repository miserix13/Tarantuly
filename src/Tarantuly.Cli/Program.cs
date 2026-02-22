using Tarantuly.Generation;
using Tarantuly.Metadata;
using Tarantuly.Templates.MudBlazor;

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase) || args.Contains("-h", StringComparer.OrdinalIgnoreCase))
{
    WriteHelp();
    return 0;
}

if (!args[0].Equals("scaffold", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Unknown command. Use 'scaffold'.");
    return 1;
}

var outputDir = ResolveArg(args, "--output") ?? "Generated";
var rootNamespace = ResolveArg(args, "--namespace") ?? "Generated";

Directory.CreateDirectory(outputDir);

var demoModel = new CrudModel(
    DbContextName: "AppDbContext",
    Entities:
    [
        new CrudEntityModel(
            Name: "SampleEntity",
            Namespace: "Domain",
            DisplayName: "Sample Entity",
            RouteSegment: "sample-entity",
            SupportsSoftDelete: true,
            SupportsConcurrency: true,
            RequiresAuthorization: true,
            Properties:
            [
                new CrudPropertyModel("Id", "System.Int32", CrudPropertyKind.Key, IsNullable: false, IsRequired: true, IsEditable: false, MaxLength: null),
                new CrudPropertyModel("Name", "System.String", CrudPropertyKind.Scalar, IsNullable: false, IsRequired: true, IsEditable: true, MaxLength: 200),
                new CrudPropertyModel("RowVersion", "System.Byte[]", CrudPropertyKind.ConcurrencyToken, IsNullable: false, IsRequired: true, IsEditable: false, MaxLength: null),
            ])
    ]);

var engine = new CrudGenerationEngine([new MudBlazorListTemplate()]);
var files = engine.Generate(demoModel, new CrudGenerationOptions { RootNamespace = rootNamespace, UiFlavor = "MudBlazor" });

foreach (var file in files)
{
    var fullPath = Path.Combine(outputDir, file.RelativePath);
    var directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(fullPath, file.Content);
    Console.WriteLine($"Generated: {fullPath}");
}

return 0;

static string? ResolveArg(IReadOnlyList<string> args, string key)
{
    for (var i = 0; i < args.Count - 1; i++)
    {
        if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }

    return null;
}

static void WriteHelp()
{
    Console.WriteLine("Tarantuly CLI");
    Console.WriteLine("Usage:");
    Console.WriteLine("  tarantuly scaffold [--output <folder>] [--namespace <rootNamespace>]");
}
