using Microsoft.EntityFrameworkCore;
using Tarantuly.Abstractions;

namespace Tarantuly.SampleApp.Data;

public sealed class ProductCrudDataService : ICrudDataService<Product, int>
{
    private readonly AppDbContext _dbContext;

    public ProductCrudDataService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSeedAsync(cancellationToken);
        return await _dbContext.Products
            .Where(product => !product.IsDeleted)
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureSeedAsync(cancellationToken);
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<Product> CreateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.RowVersion = [];
        _dbContext.Products.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == entity.Id, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException($"Product '{entity.Id}' was not found.");
        }

        existing.Name = entity.Name;
        existing.Price = entity.Price;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.IsDeleted = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureSeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.Products.AddRange(
            new Product { Name = "Huntsman", Price = 14.99m },
            new Product { Name = "Orb Weaver", Price = 19.99m },
            new Product { Name = "Jumping Spider", Price = 24.99m });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
