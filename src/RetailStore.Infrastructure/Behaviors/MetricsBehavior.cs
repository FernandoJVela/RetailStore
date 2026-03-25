using System.Diagnostics;
using System.Diagnostics.Metrics;
using MediatR;

namespace RetailStore.Infrastructure.Behaviors;

public sealed class MetricsBehavior<TRequest, TResponse> : 
    IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    private static readonly Meter Meter = new("RetailStore.Requests");
    private static readonly Counter<long> RequestCount = Meter.CreateCounter<long>("requests.total");
    private static readonly Counter<long> ErrorCount = Meter.CreateCounter<long>("requests.errors");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("requests.duration_ms");

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            RequestCount.Add(1, new KeyValuePair<string, object?>("type", requestName));
            return response;
        }
        catch
        {
            ErrorCount.Add(1, new KeyValuePair<string, object?>("type", requestName));
            throw;
        }
        finally { 
            Duration.Record(
                sw.Elapsed.TotalMilliseconds, 
                new KeyValuePair<string, object?>("type", requestName)
            ); 
        }
    }
}
