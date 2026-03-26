using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Products.Application.Queries;
 
// ─── DTOs (primitives only, no value objects) ───────────────
public sealed record ProductDto(
    Guid Id, string Name, string Sku,
    decimal Price, string Currency,
    string Category, bool IsActive);
 
public sealed record ProductDetailDto(
    Guid Id, string Name, string Sku,
    string? Description, decimal Price, string Currency,
    string Category, bool IsActive,
    DateTime CreatedAt, DateTime? UpdatedAt);
 
// ═══════════════════════════════════════════════════════════
// GET ALL PRODUCTS (with filtering and search)
// ═══════════════════════════════════════════════════════════
public sealed record GetProductsQuery(
    string? Category = null,
    bool? IsActive = null,
    string? Search = null
) : IQuery<List<ProductDto>>;
 
public sealed class GetProductsHandler(RetailStoreDbContext db)
    : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetProductsQuery query, CancellationToken ct)
    {
        var q = db.Set<Product>().AsNoTracking().AsQueryable();
 
        if (query.Category is not null)
            q = q.Where(p => p.Category == query.Category);
        if (query.IsActive.HasValue)
            q = q.Where(p => p.IsActive == query.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant().Trim();
            q = q.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Sku.ToLower().Contains(search));
        }
 
        // Load entities then project in memory (ComplexProperty can't project in LINQ Select)
        var products = await q.OrderBy(p => p.Name).ToListAsync(ct);
 
        return products.Select(p => new ProductDto(
            p.Id, p.Name, p.Sku,
            p.Price.Amount, p.Price.Currency,
            p.Category, p.IsActive)).ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET PRODUCT BY ID
// ═══════════════════════════════════════════════════════════
public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDetailDto>;
 
public sealed class GetProductByIdHandler(RetailStoreDbContext db)
    : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await db.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id, ct)
            ?? throw new DomainException(ProductErrors.NotFound(query.Id));
 
        return new ProductDetailDto(
            product.Id, product.Name, product.Sku,
            product.Description, product.Price.Amount, product.Price.Currency,
            product.Category, product.IsActive,
            product.CreatedAt, product.UpdatedAt);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET PRODUCTS BY CATEGORY
// ═══════════════════════════════════════════════════════════
public sealed record GetProductsByCategoryQuery(string Category) : IQuery<List<ProductDto>>;
 
public sealed class GetProductsByCategoryHandler(RetailStoreDbContext db)
    : IRequestHandler<GetProductsByCategoryQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetProductsByCategoryQuery query, CancellationToken ct)
    {
        var products = await db.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Category == query.Category && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
 
        return products.Select(p => new ProductDto(
            p.Id, p.Name, p.Sku,
            p.Price.Amount, p.Price.Currency,
            p.Category, p.IsActive)).ToList();
    }
}