using System.Text.Json;

namespace MCPDemo.Api.McpTools;

/// <summary>
/// Shared JSON serialization options for all MCP tools.
/// </summary>
public static class ToolJsonOptions
{
    /// <summary>
    /// Default JSON options: camelCase naming policy, case-insensitive property matching.
    /// </summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
}
