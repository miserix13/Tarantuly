namespace Tarantuly.Abstractions;

public interface ICrudDataService<TEntity, in TKey>
    where TEntity : class
{
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task RestoreAsync(TKey id, CancellationToken cancellationToken = default);
}
