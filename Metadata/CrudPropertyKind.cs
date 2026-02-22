namespace Tarantuly.Metadata;

public enum CrudPropertyKind
{
    Scalar,
    Key,
    ConcurrencyToken,
    NavigationReference,
    NavigationCollection,
}
