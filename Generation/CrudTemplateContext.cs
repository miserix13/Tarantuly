using Tarantuly.Metadata;

namespace Tarantuly.Generation;

public sealed record CrudTemplateContext(
    CrudModel Model,
    CrudEntityModel Entity,
    string RootNamespace,
    string UiFlavor
);
