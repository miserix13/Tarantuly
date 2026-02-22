namespace Tarantuly.Generation;

public sealed class CrudGenerationOptions
{
    public string RootNamespace { get; init; } = "Generated";

    public string UiFlavor { get; init; } = "MudBlazor";
}
