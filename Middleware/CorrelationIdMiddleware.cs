namespace RetailStore.Api.Middleware;

public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var correlationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();

        var requestId = Guid.NewGuid().ToString();

        ctx.Items["CorrelationId"] = correlationId;
        ctx.Items["RequestId"] = requestId;

        ctx.Response.Headers["X-Correlation-Id"] = correlationId;
        ctx.Response.Headers["X-Request-Id"] = requestId;
        
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        using (Serilog.Context.LogContext.PushProperty("RequestId", requestId))
            await next(ctx);
    }
}
