using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Queries;

public sealed record GetUsersQuery() : IQuery<IReadOnlyList<UserDto>>;

public sealed class GetUsersHandler
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly RetailStoreDbContext _db;

    public GetUsersHandler(RetailStoreDbContext db) => _db = db;

    public async Task<IReadOnlyList<UserDto>> Handle(
        GetUsersQuery query, CancellationToken ct)
    {
        var users = await _db.Set<User>()
            .AsNoTracking()
            .Select(u => new UserDto(
                u.Id, u.Username, u.Email, u.LastLoginAt, u.IsActive))
            .ToListAsync(ct);

        // Throws DomainException with 404 mapping automatically
        if (users.Count == 0)
            throw new DomainException(UserErrors.NotFound());

        return users;
    }
}