using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> log) 
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, 
        Exception ex, 
        CancellationToken ct)
    {
        var (status, problem) = ex switch
        {
            DomainException d => ((int)d.Error.Type, BuildProblem(d)),
            OperationCanceledException => 
                (499, new ProblemDetails { 
                    Title = "Cancelled", 
                    Status = 499 }
                ),
            _ => (500, new ProblemDetails { 
                    Title = "Internal Server Error", 
                    Status = 500, 
                    Detail = "An unexpected error occurred." }
                )
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? ctx.TraceIdentifier;
        problem.Extensions["correlationId"] = ctx.Items["CorrelationId"]?.ToString();
        problem.Extensions["requestId"] = ctx.Items["RequestId"]?.ToString();
        problem.Extensions["timestamp"] = DateTime.UtcNow.ToString("O");

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }

    private static ProblemDetails BuildProblem(DomainException ex)
    {
        var pd = new ProblemDetails
        {
            Type = $"https://retailstore.io/errors/{ex.Error.Code.ToLowerInvariant().Replace('_', '-')}",
            Title = string.Join(' ', ex.Error.Code.Split('_')).ToLowerInvariant(),
            Status = (int)ex.Error.Type,
            Detail = ex.Error.Message
        };
        pd.Extensions["errorCode"] = ex.Error.Code;
        if (ex.ValidationErrors is { Count: > 0 })
            pd.Extensions["errors"] = ex.ValidationErrors.Select(e => new { e.Code, e.Message });
        return pd;
    }
}