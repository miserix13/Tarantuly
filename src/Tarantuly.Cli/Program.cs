using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.EntityFrameworkCore;
using Tarantuly.Discovery;
using Tarantuly.Generation;
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
var projectPath = ResolveArg(args, "--project");
var assemblyPath = ResolveArg(args, "--assembly");
var contextName = ResolveArg(args, "--context");

if (string.IsNullOrWhiteSpace(contextName))
{
    Console.Error.WriteLine("Missing required argument '--context <DbContextTypeName>'.");
    return 1;
}

if (string.IsNullOrWhiteSpace(projectPath) && string.IsNullOrWhiteSpace(assemblyPath))
{
    Console.Error.WriteLine("Provide either '--project <path-to-csproj>' or '--assembly <path-to-dll>'.");
    return 1;
}

if (!string.IsNullOrWhiteSpace(projectPath) && string.IsNullOrWhiteSpace(assemblyPath))
{
    var built = BuildProject(projectPath);
    if (!built)
    {
        return 1;
    }

    assemblyPath = ResolveBuiltAssemblyPath(projectPath);
}

if (string.IsNullOrWhiteSpace(assemblyPath) || !File.Exists(assemblyPath))
{
    Console.Error.WriteLine($"Unable to resolve assembly file. Value: '{assemblyPath ?? "<null>"}'");
    return 1;
}

Directory.CreateDirectory(outputDir);

var loadContext = new ProjectAssemblyLoadContext(assemblyPath);
try
{
    var appAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);
    var dbContextType = ResolveDbContextType(appAssembly, contextName);

    if (dbContextType is null)
    {
        Console.Error.WriteLine($"Could not find DbContext '{contextName}' in assembly '{assemblyPath}'.");
        return 1;
    }

    using var dbContext = CreateDbContextInstance(appAssembly, dbContextType);
    if (dbContext is null)
    {
        Console.Error.WriteLine($"Could not create instance of DbContext '{dbContextType.FullName}'.");
        Console.Error.WriteLine("Support currently includes parameterless DbContext or design-time factory with CreateDbContext(string[]). ");
        return 1;
    }

    var discovery = new EfCrudModelDiscovery();
    var model = discovery.Discover(dbContext);

    var engine = new CrudGenerationEngine([
        new MudBlazorListTemplate(),
        new MudBlazorDetailsTemplate(),
        new MudBlazorUpsertTemplate(),
    ]);

    var files = engine.Generate(model, new CrudGenerationOptions { RootNamespace = rootNamespace, UiFlavor = "MudBlazor" });

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
}
finally
{
    loadContext.Unload();
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
    Console.WriteLine("  tarantuly scaffold --context <DbContextType> [--project <path-to-csproj> | --assembly <path-to-dll>] [--output <folder>] [--namespace <rootNamespace>]");
}

static bool BuildProject(string projectPath)
{
    if (!File.Exists(projectPath))
    {
        Console.Error.WriteLine($"Project file not found: {projectPath}");
        return false;
    }

    var startInfo = new ProcessStartInfo("dotnet", $"build \"{projectPath}\"")
    {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };

    using var process = Process.Start(startInfo);
    if (process is null)
    {
        Console.Error.WriteLine("Failed to start dotnet build process.");
        return false;
    }

    process.OutputDataReceived += (_, eventArgs) =>
    {
        if (!string.IsNullOrWhiteSpace(eventArgs.Data))
        {
            Console.WriteLine(eventArgs.Data);
        }
    };

    process.ErrorDataReceived += (_, eventArgs) =>
    {
        if (!string.IsNullOrWhiteSpace(eventArgs.Data))
        {
            Console.Error.WriteLine(eventArgs.Data);
        }
    };

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();

    if (process.ExitCode == 0)
    {
        return true;
    }

    Console.Error.WriteLine($"dotnet build failed with exit code {process.ExitCode}");
    return false;
}

static string? ResolveBuiltAssemblyPath(string projectPath)
{
    var projectFullPath = Path.GetFullPath(projectPath);
    var projectDir = Path.GetDirectoryName(projectFullPath);
    var projectName = Path.GetFileNameWithoutExtension(projectFullPath);

    if (string.IsNullOrWhiteSpace(projectDir))
    {
        return null;
    }

    var dllCandidates = Directory
        .EnumerateFiles(Path.Combine(projectDir, "bin"), $"{projectName}.dll", SearchOption.AllDirectories)
        .Where(path => !path.Contains("\\ref\\", StringComparison.OrdinalIgnoreCase)
                    && !path.Contains("\\refint\\", StringComparison.OrdinalIgnoreCase))
        .Select(path => new FileInfo(path))
        .OrderByDescending(file => file.LastWriteTimeUtc)
        .ToArray();

    return dllCandidates.FirstOrDefault()?.FullName;
}

static Type? ResolveDbContextType(Assembly assembly, string contextName)
{
    return GetLoadableTypes(assembly)
        .Where(type => !type.IsAbstract && typeof(DbContext).IsAssignableFrom(type))
        .FirstOrDefault(type => type.FullName?.Equals(contextName, StringComparison.Ordinal) == true
                                || type.Name.Equals(contextName, StringComparison.Ordinal));
}

static DbContext? CreateDbContextInstance(Assembly assembly, Type dbContextType)
{
    var parameterlessConstructor = dbContextType.GetConstructor(Type.EmptyTypes);
    if (parameterlessConstructor is not null)
    {
        return parameterlessConstructor.Invoke([]) as DbContext;
    }

    var factoryInstance = GetLoadableTypes(assembly)
        .Where(type => !type.IsAbstract && !type.IsInterface)
        .Select(type => new
        {
            Type = type,
            FactoryInterface = type.GetInterfaces().FirstOrDefault(@interface =>
                @interface.IsGenericType
                && @interface.Name.Equals("IDesignTimeDbContextFactory`1", StringComparison.Ordinal)
                && @interface.GetGenericArguments()[0] == dbContextType)
        })
        .FirstOrDefault(item => item.FactoryInterface is not null);

    if (factoryInstance is null)
    {
        return null;
    }

    var factory = Activator.CreateInstance(factoryInstance.Type);
    if (factory is null)
    {
        return null;
    }

    var createMethod = factoryInstance.Type.GetMethod("CreateDbContext", new[] { typeof(string[]) });
    if (createMethod is null)
    {
        return null;
    }

    return createMethod.Invoke(factory, [Array.Empty<string>()]) as DbContext;
}

static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
{
    try
    {
        return assembly.GetTypes();
    }
    catch (ReflectionTypeLoadException exception)
    {
        return exception.Types.Where(type => type is not null).Cast<Type>().ToArray();
    }
}

file sealed class ProjectAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _baseDirectory;

    public ProjectAssemblyLoadContext(string mainAssemblyPath)
        : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
        _baseDirectory = Path.GetDirectoryName(mainAssemblyPath) ?? AppContext.BaseDirectory;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is not null
            && (assemblyName.Name.Equals("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                || assemblyName.Name.Equals("Microsoft.EntityFrameworkCore.Abstractions", StringComparison.Ordinal)))
        {
            return null;
        }

        var resolvedPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (!string.IsNullOrWhiteSpace(resolvedPath) && File.Exists(resolvedPath))
        {
            return LoadFromAssemblyPath(resolvedPath);
        }

        var candidatePath = Path.Combine(_baseDirectory, $"{assemblyName.Name}.dll");
        if (File.Exists(candidatePath))
        {
            return LoadFromAssemblyPath(candidatePath);
        }

        return null;
    }
}
