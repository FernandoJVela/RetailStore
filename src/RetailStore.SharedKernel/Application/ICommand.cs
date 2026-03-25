using MediatR;

namespace RetailStore.SharedKernel.Application;

/// <summary>
/// Command with no return value. Handler returns Unit.
/// </summary>
public interface ICommand : IRequest<Unit> { }

/// <summary>
/// Command that returns a value (e.g., the created entity's ID).
/// Note: returns TResponse directly, NOT Result of TResponse.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse> { }
