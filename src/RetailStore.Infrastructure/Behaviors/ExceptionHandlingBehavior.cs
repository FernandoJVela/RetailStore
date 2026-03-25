using MediatR;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Infrastructure.Behaviors;

public sealed class ExceptionHandlingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (DomainException)
        {
            throw;  // Already a domain error, pass through
        }
        catch (OperationCanceledException)
        {
            throw;  // Let cancellation propagate naturally
        }
        catch (Exception ex)
        {
            // Wrap unexpected exceptions as Internal errors
            throw new DomainException(new DomainError(
                "INTERNAL_ERROR",
                $"An unexpected error occurred: {ex.Message}",
                DomainErrorType.Internal));
        }
    }
}
