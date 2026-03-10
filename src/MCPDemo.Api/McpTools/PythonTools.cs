using System.ComponentModel;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.Interfaces;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using MCPDemo.Shared.Exceptions;
using MCPDemo.Infrastructure.PythonSandbox;

namespace MCPDemo.Api.McpTools;

/// <summary>
/// Exposes Python sandbox execution as a discoverable MCP tool.
/// This tool allows the AI to perform custom data analysis that isn't possible through standard API endpoints.
/// </summary>
[McpServerToolType]
public static class PythonTools
{
    /// <summary>
    /// Executes Python code in a secure Docker sandbox.
    /// </summary>
    /// <param name="sandboxService">The sandbox service (injected).</param>
    /// <param name="logger">The logger (injected).</param>
    /// <param name="metrics">The metrics collector (injected).</param>
    /// <param name="code">The Python code to execute.</param>
    /// <param name="data">Optional JSON data payload.</param>
    /// <returns>The captured stdout or an error message.</returns>
    [McpServerTool]
    [Description("Execute arbitrary Python code in a secure Docker sandbox for advanced data analysis. Returns the captured stdout (String). " +
                 "Libraries available: json, pandas (as pd), numpy (as np).")]
    public static async Task<string> run_python_code(
        IPythonSandboxService sandboxService,
        ILogger<PythonSandboxService> logger, // Re-use service logger for consistency
        IMetricsCollector metrics,
        [Description("The Python source code to execute. String. Required. Available: pandas (pd), numpy (np), json. Access data via 'data' variable. Use print() for output.")] 
        string code,
        [Description("Optional JSON data payload provided to the script as 'data' variable. JSON String. Optional.")] 
        string? data = null)
    {
        const string toolName = "run_python_code";
        var sw = Stopwatch.StartNew();

        // Validate code is not empty
        if (string.IsNullOrWhiteSpace(code))
        {
            metrics.RecordExecution(toolName, 0, false, "ValidationError");
            return "Error: Python code cannot be empty.";
        }

        // Validate data is valid JSON if provided
        if (data != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(data);
            }
            catch (JsonException ex)
            {
                metrics.RecordExecution(toolName, 0, false, "JsonValidationError");
                return $"Error: 'data' parameter is not valid JSON. {ex.Message}";
            }
        }

        try
        {
            logger.LogInformation("Executing MCP Tool: {ToolName}", toolName);
            
            // The service handles Docker invocation, resource limits, and timeouts.
            var result = await sandboxService.ExecuteAsync(code, data);
            
            sw.Stop();
            metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            
            return result;
        }
        catch (PythonSandboxException ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            logger.LogError(ex, "MCP Tool {ToolName} failed with sandbox error", toolName);
            
            // Return sandbox/runtime errors back to the AI assistant for potential self-correction
            return ex.Message;
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            logger.LogError(ex, "MCP Tool {ToolName} failed with unexpected error", toolName);
            
            return $"An unexpected error occurred: {ex.Message}";
        }
    }
}
