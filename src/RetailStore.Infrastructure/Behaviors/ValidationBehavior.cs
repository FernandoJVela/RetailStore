using FluentValidation;
using MediatR;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Infrastructure.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => new DomainError(
                $"VALIDATION_{f.PropertyName.ToUpperInvariant()}",
                f.ErrorMessage,
                DomainErrorType.Validation))
            .ToList();

        if (failures.Count > 0)
            throw new DomainException(failures);

        return await next();
    }
}