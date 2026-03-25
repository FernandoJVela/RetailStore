using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace RetailStore.Infrastructure.Behaviors;

public sealed class CorrelationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationBehavior(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var correlationId = 
            _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? 
                Guid.NewGuid().ToString();

        var requestId = 
            _httpContextAccessor.HttpContext?.Items["RequestId"]?.ToString() ?? 
                Guid.NewGuid().ToString();
                
        Activity.Current?.SetTag("correlation.id", correlationId);
        Activity.Current?.SetTag("request.id", requestId);
        Activity.Current?.SetTag("request.type", typeof(TRequest).Name);
        
        return await next();

    }
}
