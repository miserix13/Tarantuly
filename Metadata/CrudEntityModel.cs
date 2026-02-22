namespace Tarantuly.Metadata;

public sealed record CrudEntityModel(
    string Name,
    string Namespace,
    string DisplayName,
    string RouteSegment,
    bool SupportsSoftDelete,
    bool SupportsConcurrency,
    bool RequiresAuthorization,
    IReadOnlyList<CrudPropertyModel> Properties
)
{
    public string FullName => $"{Namespace}.{Name}";
}
