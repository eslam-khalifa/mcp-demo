namespace MCPDemo.Shared.Models;

/// <summary>
/// Standardized error model for MCP tool output.
/// </summary>
public class ErrorResponse
{
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ToolName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
