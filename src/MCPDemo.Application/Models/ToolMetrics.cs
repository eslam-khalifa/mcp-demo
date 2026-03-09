namespace MCPDemo.Application.Models;

/// <summary>
/// Aggregated runtime metrics for a single MCP tool.
/// </summary>
public class ToolMetrics
{
    /// <summary>Total number of invocations.</summary>
    public int TotalCalls { get; set; }

    /// <summary>Number of successful completions.</summary>
    public int SuccessCount { get; set; }

    /// <summary>Number of failed completions.</summary>
    public int FailureCount { get; set; }

    /// <summary>Running average of execution time in milliseconds.</summary>
    public double AverageExecutionTimeMs { get; set; }

    /// <summary>Sum of all execution times (used to compute the running average).</summary>
    public long TotalExecutionTimeMs { get; set; }

    /// <summary>Error count grouped by error type classification.</summary>
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
}
