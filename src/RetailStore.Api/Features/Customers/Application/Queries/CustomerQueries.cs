using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Customers.Application.Queries;
 
// ─── DTOs ───────────────────────────────────────────────────
public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt);
 
public sealed record CustomerDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    bool IsActive,
    ShippingAddressResponseDto? ShippingAddress,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
 
public sealed record ShippingAddressResponseDto(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);
 
// ═══════════════════════════════════════════════════════════
// GET ALL CUSTOMERS (with filtering)
// ═══════════════════════════════════════════════════════════
public sealed record GetCustomersQuery(
    string? Search = null,
    bool? IsActive = null
) : IQuery<List<CustomerDto>>;
 
public sealed class GetCustomersHandler : IRequestHandler<GetCustomersQuery, List<CustomerDto>>
{
    private readonly RetailStoreDbContext _db;
    public GetCustomersHandler(RetailStoreDbContext db) => _db = db;
 
    public async Task<List<CustomerDto>> Handle(GetCustomersQuery query, CancellationToken ct)
    {
        var q = _db.Set<Customer>().AsNoTracking().AsQueryable();
 
        if (query.IsActive.HasValue)
            q = q.Where(c => c.IsActive == query.IsActive.Value);
 
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant().Trim();
            q = q.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.Email.Contains(search));
        }
 
        return await q
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Select(c => new CustomerDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.FirstName + " " + c.LastName,
                c.Email,
                c.Phone,
                c.IsActive,
                c.CreatedAt))
            .ToListAsync(ct);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET CUSTOMER BY ID (with address)
// ═══════════════════════════════════════════════════════════
public sealed record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDetailDto>;
 
public sealed class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto>
{
    private readonly RetailStoreDbContext _db;
    public GetCustomerByIdHandler(RetailStoreDbContext db) => _db = db;
 
    public async Task<CustomerDetailDto> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await _db.Set<Customer>()
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new CustomerDetailDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.FirstName + " " + c.LastName,
                c.Email,
                c.Phone,
                c.IsActive,
                c.ShippingStreet != null
                    ? new ShippingAddressResponseDto(
                        c.ShippingStreet,
                        c.ShippingCity!,
                        c.ShippingState!,
                        c.ShippingZipCode!,
                        c.ShippingCountry!)
                    : null,
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(ct);
 
        if (customer is null)
            throw new DomainException(CustomerErrors.NotFound(query.Id));
 
        return customer;
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET CUSTOMER BY EMAIL
// ═══════════════════════════════════════════════════════════
public sealed record GetCustomerByEmailQuery(string Email) : IQuery<CustomerDetailDto>;
 
public sealed class GetCustomerByEmailHandler : IRequestHandler<GetCustomerByEmailQuery, CustomerDetailDto>
{
    private readonly RetailStoreDbContext _db;
    public GetCustomerByEmailHandler(RetailStoreDbContext db) => _db = db;
 
    public async Task<CustomerDetailDto> Handle(GetCustomerByEmailQuery query, CancellationToken ct)
    {
        var email = query.Email.ToLowerInvariant().Trim();
 
        var customer = await _db.Set<Customer>()
            .AsNoTracking()
            .Where(c => c.Email == email)
            .Select(c => new CustomerDetailDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.FirstName + " " + c.LastName,
                c.Email,
                c.Phone,
                c.IsActive,
                c.ShippingStreet != null
                    ? new ShippingAddressResponseDto(
                        c.ShippingStreet,
                        c.ShippingCity!,
                        c.ShippingState!,
                        c.ShippingZipCode!,
                        c.ShippingCountry!)
                    : null,
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(ct);
 
        if (customer is null)
            throw new DomainException(CustomerErrors.NotFoundByEmail(query.Email));
 
        return customer;
    }
}