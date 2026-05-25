using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

public sealed record UserDto(
    Guid Id, string Username, string Email, DateTime? LastLoginAt, bool IsActive,
    IReadOnlyList<Guid> RoleIds);

public sealed class GetUserByIdHandler
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly RetailStoreDbContext _db;

    public GetUserByIdHandler(RetailStoreDbContext db) => _db = db;

    public async Task<UserDto> Handle(
        GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await _db.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, ct);

        if (user is null)
            throw new DomainException(UserErrors.NotFound(query.Id));

        return new UserDto(user.Id, user.Username, user.Email, user.LastLoginAt, user.IsActive,
            user.RoleIds.ToList());
    }
}