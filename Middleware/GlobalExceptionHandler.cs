using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, problemDetails) = exception switch
        {
            DomainException domainEx => MapDomainException(domainEx),
            OperationCanceledException => (499, new ProblemDetails
            {
                Title = "Request Cancelled",
                Detail = "The request was cancelled by the client.",
                Status = 499
            }),
            _ => (500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Status = 500
            })
        };

        // Enrich with correlation and trace info
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id
            ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] =
            httpContext.Items["CorrelationId"]?.ToString();
        problemDetails.Extensions["timestamp"] =
            DateTime.UtcNow.ToString("O");

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails, ct);

        return true;  // Exception is handled
    }

    private static (int, ProblemDetails) MapDomainException(
        DomainException ex)
    {
        var statusCode = ex.Error.Type switch
        {
            DomainErrorType.Validation     => 400,
            DomainErrorType.Unauthorized   => 401,
            DomainErrorType.Forbidden      => 403,
            DomainErrorType.NotFound       => 404,
            DomainErrorType.Conflict       => 409,
            DomainErrorType.BusinessRule   => 422,
            DomainErrorType.Internal       => 500,
            _ => 500
        };

        var problem = new ProblemDetails
        {
            Type = $"https://retailstore.io/errors/{ex.Error.Code.ToLowerInvariant().Replace('_', '-')}",
            Title = FormatTitle(ex.Error.Code),
            Status = statusCode,
            Detail = ex.Error.Message
        };

        problem.Extensions["errorCode"] = ex.Error.Code;

        // Include validation errors array if present
        if (ex.ValidationErrors is { Count: > 0 })
        {
            problem.Extensions["errors"] = ex.ValidationErrors
                .Select(e => new { code = e.Code, message = e.Message });
        }

        return (statusCode, problem);
    }

    private static string FormatTitle(string code)
        => string.Join(' ', code.Split('_'))
            .ToLowerInvariant();
}