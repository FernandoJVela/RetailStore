using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Products.Application.Queries;

// Query
public record GetProductsQuery(
    string? Category = null,
    bool? IsActive = null
) : IQuery<IReadOnlyList<ProductDto>>;

// Handler - queries directly, bypassing repository
public class GetProductsHandler
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly RetailStoreDbContext _db;

    public GetProductsHandler(RetailStoreDbContext db)
        => _db = db;

    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetProductsQuery query, CancellationToken ct)
    {
        var q = _db.Set<Product>()
            .AsNoTracking()
            .AsQueryable();

        if (query.Category is not null)
            q = q.Where(p => p.Category == query.Category);
        if (query.IsActive is not null)
            q = q.Where(p => p.IsActive == query.IsActive);

        var products = await q.Select(p => new ProductDto(
            p.Id, p.Name, p.Sku,
            p.Price, p.Category, p.IsActive
        )).ToListAsync(ct);

        // Throws DomainException with 404 mapping automatically
        if (products is null)
            throw new DomainException(ProductErrors.Empty());

        return products;
    }
}