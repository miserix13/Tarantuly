namespace Tarantuly.Metadata;

public sealed record CrudModel(
    string DbContextName,
    IReadOnlyList<CrudEntityModel> Entities
);
