namespace RetailStore.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        context.Response.Headers["X-Request-Id"] =
            context.TraceIdentifier;

        using (Serilog.Context.LogContext
            .PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
