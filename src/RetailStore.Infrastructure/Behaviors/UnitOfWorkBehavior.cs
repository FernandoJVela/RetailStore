using MediatR;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Infrastructure.Behaviors;

/// <summary>
/// Automatically calls SaveChangesAsync after commands succeed.
/// Only runs for ICommand types (not queries).
/// This means handlers NEVER call SaveChanges themselves.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly RetailStoreDbContext _db;

    public UnitOfWorkBehavior(RetailStoreDbContext db) => _db = db;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Skip for queries - they don't modify state
        if (request is not ICommand && !typeof(TRequest)
            .GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(ICommand<>)))
        {
            return await next();
        }

        var response = await next();

        // SaveChangesAsync triggers the Outbox (domain events → outbox messages)
        await _db.SaveChangesAsync(ct);

        return response;
    }
}