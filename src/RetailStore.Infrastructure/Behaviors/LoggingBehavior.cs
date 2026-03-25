using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Infrastructure.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "[START] {RequestName} {@Request}",
            requestName, request);

        try
        {
            var response = await next();
            sw.Stop();

            _logger.LogInformation(
                "[END]   {RequestName} completed in {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (DomainException ex)
        {
            sw.Stop();
            _logger.LogWarning(
                "[FAIL]  {RequestName} failed in {ElapsedMs}ms: {ErrorCode} - {ErrorMessage}",
                requestName, sw.ElapsedMilliseconds,
                ex.Error.Code, ex.Error.Message);
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "[ERROR] {RequestName} threw unexpected exception in {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}