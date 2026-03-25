using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Providers.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Providers.Application.Queries;
 
// ─── DTOs ───────────────────────────────────────────────────
public sealed record ProviderDto(
    Guid Id, string CompanyName, string ContactName,
    string Email, string? Phone, bool IsActive,
    int ProductCount);
 
public sealed record ProviderDetailDto(
    Guid Id, string CompanyName, string ContactName,
    string Email, string? Phone, bool IsActive,
    List<Guid> ProductIds, int ProductCount,
    DateTime CreatedAt, DateTime? UpdatedAt);
 
// ═══════════════════════════════════════════════════════════
// GET ALL PROVIDERS
// ═══════════════════════════════════════════════════════════
public sealed record GetProvidersQuery(
    string? Search = null, bool? IsActive = null
) : IQuery<List<ProviderDto>>;
 
public sealed class GetProvidersHandler(RetailStoreDbContext db)
    : IRequestHandler<GetProvidersQuery, List<ProviderDto>>
{
    public async Task<List<ProviderDto>> Handle(GetProvidersQuery query, CancellationToken ct)
    {
        var q = db.Set<Provider>().AsNoTracking().AsQueryable();
 
        if (query.IsActive.HasValue)
            q = q.Where(p => p.IsActive == query.IsActive.Value);
 
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant().Trim();
            q = q.Where(p =>
                p.CompanyName.ToLower().Contains(search) ||
                p.ContactName.ToLower().Contains(search) ||
                p.Email.Contains(search));
        }
 
        var providers = await q
            .OrderBy(p => p.CompanyName)
            .ToListAsync(ct);
 
        return providers.Select(p => new ProviderDto(
            p.Id, p.CompanyName, p.ContactName,
            p.Email, p.Phone, p.IsActive,
            p.ProductCount)).ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET PROVIDER BY ID
// ═══════════════════════════════════════════════════════════
public sealed record GetProviderByIdQuery(Guid Id) : IQuery<ProviderDetailDto>;
 
public sealed class GetProviderByIdHandler(RetailStoreDbContext db)
    : IRequestHandler<GetProviderByIdQuery, ProviderDetailDto>
{
    public async Task<ProviderDetailDto> Handle(GetProviderByIdQuery query, CancellationToken ct)
    {
        var provider = await db.Set<Provider>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id, ct)
            ?? throw new DomainException(ProviderErrors.NotFound(query.Id));
 
        return new ProviderDetailDto(
            provider.Id, provider.CompanyName, provider.ContactName,
            provider.Email, provider.Phone, provider.IsActive,
            provider.ProductIdList, provider.ProductCount,
            provider.CreatedAt, provider.UpdatedAt);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET PROVIDERS BY PRODUCT (who supplies this product?)
// ═══════════════════════════════════════════════════════════
public sealed record GetProvidersByProductQuery(Guid ProductId) : IQuery<List<ProviderDto>>;
 
public sealed class GetProvidersByProductHandler(RetailStoreDbContext db)
    : IRequestHandler<GetProvidersByProductQuery, List<ProviderDto>>
{
    public async Task<List<ProviderDto>> Handle(GetProvidersByProductQuery query, CancellationToken ct)
    {
        // Since ProductIds is a JSON column, we load all active providers
        // and filter in memory. For large datasets, consider a join table.
        var providers = await db.Set<Provider>()
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(ct);
 
        return providers
            .Where(p => p.SuppliesProduct(query.ProductId))
            .Select(p => new ProviderDto(
                p.Id, p.CompanyName, p.ContactName,
                p.Email, p.Phone, p.IsActive,
                p.ProductCount))
            .OrderBy(p => p.CompanyName)
            .ToList();
    }
}