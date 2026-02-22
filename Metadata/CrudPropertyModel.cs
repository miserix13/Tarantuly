namespace Tarantuly.Metadata;

public sealed record CrudPropertyModel(
    string Name,
    string TypeName,
    CrudPropertyKind Kind,
    bool IsNullable,
    bool IsRequired,
    bool IsEditable,
    int? MaxLength
);
