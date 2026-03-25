using System.Diagnostics;
using MediatR;

namespace RetailStore.Infrastructure.Behaviors;

public sealed class TracingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource Source = new("RetailStore.Pipeline");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        using var activity = Source.StartActivity(requestName);

        activity?.SetTag("mediatr.request_type", requestName);
        activity?.SetTag("mediatr.response_type", typeof(TResponse).Name);

        try
        {
            var response = await next();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}
