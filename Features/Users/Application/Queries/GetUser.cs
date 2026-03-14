using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;

public sealed record UserDto(
    Guid Id, string Username, string Email, bool IsActive);

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
            .Where(u => u.Id == query.Id)
            .Select(u => new UserDto(
                u.Id, u.Username, u.Email, u.IsActive))
            .FirstOrDefaultAsync(ct);

        // Throws DomainException with 404 mapping automatically
        if (user is null)
            throw new DomainException(UserErrors.NotFound(query.Id));

        return user;
    }
}