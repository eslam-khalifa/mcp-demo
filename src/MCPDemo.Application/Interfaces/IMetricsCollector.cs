using MCPDemo.Application.Models;

namespace MCPDemo.Application.Interfaces;

/// <summary>
/// Contract for recording and retrieving per-tool execution metrics.
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Record a single tool execution.
    /// </summary>
    void RecordExecution(string toolName, long elapsedMs, bool success, string? errorType = null);

    /// <summary>
    /// Retrieve metrics for a single tool.
    /// </summary>
    ToolMetrics GetMetrics(string toolName);

    /// <summary>
    /// Retrieve metrics for all tools.
    /// </summary>
    IReadOnlyDictionary<string, ToolMetrics> GetAllMetrics();
}
