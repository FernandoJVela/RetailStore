using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Products.Application.Queries;

// Query
public record GetProductByIdQuery(
    Guid Id
) : IQuery<ProductDto>;

// Handler - queries directly, bypassing repository
public class GetProductHandler
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly RetailStoreDbContext _db;

    public GetProductHandler(RetailStoreDbContext db)
        => _db = db;

    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery query, CancellationToken ct)
    {
        var q = _db.Set<Product>()
            .AsNoTracking()
            .AsQueryable();

        if (query.Id != Guid.Empty)
            q = q.Where(p => p.Id == query.Id);

        var product = await q.Select(p => new ProductDto(
            p.Id, p.Name, p.Sku,
            p.Price, p.Category, p.IsActive
        )).FirstOrDefaultAsync(ct);

        return product is not null 
            ? Result.Success(product)
            : Result.Failure<ProductDto>("Product not found");
    }
}