using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Queries;

public sealed record GetRolesQuery() : IQuery<IReadOnlyList<RoleDto>>;

public sealed record RoleDto(
    Guid Id, string Name, string? Description, bool IsSystem, IReadOnlyCollection<Permission> Permissions, DateTime CreatedAt); 

public sealed class GetRolesHandler
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly RetailStoreDbContext _db;

    public GetRolesHandler(RetailStoreDbContext db) => _db = db;

    public async Task<IReadOnlyList<RoleDto>> Handle(
        GetRolesQuery query, CancellationToken ct)
    {
        var roles = await _db.Set<Role>()
            .AsNoTracking()
            .Select(u => new RoleDto(
                u.Id, u.Name, u.Description, u.IsSystem, u.Permissions, u.CreatedAt))
            .ToListAsync(ct);

        // Throws DomainException with 404 mapping automatically
        if (roles.Count == 0)
            throw new DomainException(UserErrors.NotFound());

        return roles;
    }
}