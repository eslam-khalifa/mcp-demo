namespace MCPDemo.Shared.Exceptions;

/// <summary>
/// Thrown when an error occurs during MCP tool execution.
/// </summary>
public class McpToolException : Exception
{
    public string ToolName { get; }
    public string ErrorType { get; }

    public McpToolException(string toolName, string errorType, string message) : base(message)
    {
        ToolName = toolName;
        ErrorType = errorType;
    }
}
