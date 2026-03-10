using System.ComponentModel;
using System.Text.Json;
using MCPDemo.Application.Interfaces;
using ModelContextProtocol.Server;

namespace MCPDemo.Api.McpTools;

/// <summary>
/// Exposes runtime metrics for MCP tools.
/// </summary>
[McpServerToolType]
public static class MetricsTools
{
    /// <summary>
    /// Returns a JSON object of runtime metrics for all MCP tools called since server start.
    /// Each key is a tool name; each value contains: totalCalls, successCount, failureCount,
    /// averageExecutionTimeMs, and errorsByType (breakdown by error type string).
    /// Returns an empty object {} if no tools have been called yet.
    /// </summary>
    /// <param name="metrics">The metrics collector (injected).</param>
    /// <returns>A JSON string representing the metrics, or an error message.</returns>
    [McpServerTool]
    [Description("Returns a JSON object of runtime metrics for all MCP tools called since server start. " +
                 "Each key is a tool name; each value contains: totalCalls, successCount, failureCount, " +
                 "averageExecutionTimeMs, and errorsByType (breakdown by error type string). " +
                 "Returns an empty object {} if no tools have been called yet.")]
    public static string get_metrics(IMetricsCollector metrics)
    {
        try
        {
            var allMetrics = metrics.GetAllMetrics();
            return JsonSerializer.Serialize(allMetrics, ToolJsonOptions.Default);
        }
        catch (Exception ex)
        {
            // Return plain text error string as per FR-005
            return $"Error: Failed to retrieve metrics. {ex.Message}";
        }
    }
}
