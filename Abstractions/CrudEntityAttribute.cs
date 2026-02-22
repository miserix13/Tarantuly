using System.Diagnostics.CodeAnalysis;

namespace Tarantuly.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CrudEntityAttribute : Attribute
{
    public CrudEntityAttribute([StringSyntax("Route")] string? route = null)
    {
        Route = route;
    }

    public string? Route { get; }
}
