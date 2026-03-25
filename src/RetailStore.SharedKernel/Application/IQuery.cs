using MediatR;

namespace RetailStore.SharedKernel.Application;

/// <summary>
/// Query that returns TResponse directly.
/// Queries never modify state. Handlers throw DomainException on error.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse> { }