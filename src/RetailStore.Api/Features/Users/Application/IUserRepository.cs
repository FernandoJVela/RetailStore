using RetailStore.SharedKernel.Application;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Users.Application;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> ExistsWithUsernameAsync(string username, CancellationToken ct = default);
}