using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Tarantuly.Metadata;

namespace Tarantuly.Discovery;

public sealed class EfCrudModelDiscovery : IEfCrudModelDiscovery
{
    public CrudModel Discover(DbContext dbContext)
    {
        var entities = dbContext.Model
            .GetEntityTypes()
            .Where(type => !type.IsOwned())
            .Select(MapEntity)
            .OrderBy(entity => entity.Name)
            .ToArray();

        return new CrudModel(dbContext.GetType().Name, entities);
    }

    private static CrudEntityModel MapEntity(IEntityType entityType)
    {
        var properties = entityType
            .GetProperties()
            .Select(MapProperty)
            .OrderBy(property => property.Name)
            .ToArray();

        var routeSegment = entityType.GetTableName() ?? entityType.ClrType.Name;
        var supportsSoftDelete = properties.Any(property => property.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase));
        var supportsConcurrency = properties.Any(property => property.Kind == CrudPropertyKind.ConcurrencyToken);

        return new CrudEntityModel(
            entityType.ClrType.Name,
            entityType.ClrType.Namespace ?? string.Empty,
            entityType.ClrType.Name,
            routeSegment.ToLowerInvariant(),
            supportsSoftDelete,
            supportsConcurrency,
            RequiresAuthorization: true,
            properties);
    }

    private static CrudPropertyModel MapProperty(IProperty property)
    {
        var isKey = property.IsPrimaryKey();
        var isConcurrencyToken = property.IsConcurrencyToken;
        var kind = isKey
            ? CrudPropertyKind.Key
            : isConcurrencyToken
                ? CrudPropertyKind.ConcurrencyToken
                : CrudPropertyKind.Scalar;

        return new CrudPropertyModel(
            property.Name,
            property.ClrType.FullName ?? property.ClrType.Name,
            kind,
            property.IsNullable,
            !property.IsNullable,
            IsEditable: !isKey,
            property.GetMaxLength());
    }
}
