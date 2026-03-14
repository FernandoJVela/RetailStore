using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Products.Application.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;

public sealed record ProductDto(
    Guid Id, string Name, string Sku,
    Money Price, string Category, bool IsActive);

public sealed class GetProductByIdHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly RetailStoreDbContext _db;

    public GetProductByIdHandler(RetailStoreDbContext db) => _db = db;

    public async Task<ProductDto> Handle(
        GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _db.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Id == query.Id)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.Sku,
                p.Price, p.Category, p.IsActive))
            .FirstOrDefaultAsync(ct);

        // Throws DomainException with 404 mapping automatically
        if (product is null)
            throw new DomainException(ProductErrors.NotFound(query.Id));

        return product;
    }
}