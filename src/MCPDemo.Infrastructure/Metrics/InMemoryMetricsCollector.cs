using System.Collections.Concurrent;
using System.Collections.Immutable;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Models;

namespace MCPDemo.Infrastructure.Metrics;

/// <summary>
/// Thread-safe in-memory implementation of per-tool metrics collector.
/// Uses ConcurrentDictionary and Interlocked for atomic updates.
/// </summary>
public class InMemoryMetricsCollector : IMetricsCollector
{
    private readonly ConcurrentDictionary<string, ToolMetrics> _metrics = new();

    public void RecordExecution(string toolName, long elapsedMs, bool success, string? errorType = null)
    {
        _metrics.AddOrUpdate(
            toolName,
            _ => InitializeMetrics(elapsedMs, success, errorType),
            (_, existing) => UpdateMetrics(existing, elapsedMs, success, errorType)
        );
    }

    public ToolMetrics GetMetrics(string toolName)
    {
        return _metrics.TryGetValue(toolName, out var metrics) ? metrics : new ToolMetrics();
    }

    public IReadOnlyDictionary<string, ToolMetrics> GetAllMetrics()
    {
        return _metrics.ToImmutableDictionary();
    }

    private static ToolMetrics InitializeMetrics(long elapsedMs, bool success, string? errorType)
    {
        var metrics = new ToolMetrics
        {
            TotalCalls = 1,
            SuccessCount = success ? 1 : 0,
            FailureCount = success ? 0 : 1,
            TotalExecutionTimeMs = elapsedMs,
            AverageExecutionTimeMs = elapsedMs
        };

        if (!success && !string.IsNullOrWhiteSpace(errorType))
        {
            metrics.ErrorsByType[errorType] = 1;
        }

        return metrics;
    }

    private static ToolMetrics UpdateMetrics(ToolMetrics existing, long elapsedMs, bool success, string? errorType)
    {
        lock (existing)
        {
            existing.TotalCalls++;
            if (success) existing.SuccessCount++;
            else existing.FailureCount++;

            existing.TotalExecutionTimeMs += elapsedMs;
            existing.AverageExecutionTimeMs = (double)existing.TotalExecutionTimeMs / existing.TotalCalls;

            if (!success && !string.IsNullOrWhiteSpace(errorType))
            {
                if (existing.ErrorsByType.ContainsKey(errorType))
                {
                    existing.ErrorsByType[errorType]++;
                }
                else
                {
                    existing.ErrorsByType[errorType] = 1;
                }
            }
        }

        return existing;
    }
}
