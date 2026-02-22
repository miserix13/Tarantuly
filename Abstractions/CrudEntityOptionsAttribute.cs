namespace Tarantuly.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CrudEntityOptionsAttribute : Attribute
{
    public bool EnableSoftDelete { get; init; }

    public bool EnableDetailsPage { get; init; } = true;

    public bool EnableAuthorizationChecks { get; init; }

    public int DefaultPageSize { get; init; } = 25;
}
