# Quickstart: Polish & Metrics Observability (Phase 6)

**Feature**: `6-polish-metrics`  
**Date**: 2026-03-10

## Prerequisites

- Phases 1–5 complete and all tests passing
- `dotnet build MCPDemo.sln` exits with code 0

## Implementation Steps

### Step 1: Add `get_metrics` Tool

Create `src/MCPDemo.Api/McpTools/MetricsTools.cs`:

```csharp
[McpServerToolType]
public static class MetricsTools
{
    [McpServerTool]
    [Description("Returns a JSON object of runtime metrics for all MCP tools called since server start. " +
                 "Each key is a tool name; each value contains: totalCalls, successCount, failureCount, " +
                 "averageExecutionTimeMs, and errorsByType (breakdown by error type string). " +
                 "Returns an empty object {} if no tools have been called yet.")]
    public static string get_metrics(IMetricsCollector metrics)
    {
        try
        {
            var all = metrics.GetAllMetrics();
            return JsonSerializer.Serialize(all, ToolJsonOptions.Default);
        }
        catch (Exception ex)
        {
            return $"Error: Failed to retrieve metrics. {ex.Message}";
        }
    }
}
```

### Step 2: Audit & Update All Tool Descriptions

Review each tool file and ensure every `[Description]` follows:
- **Tool description**: `"<Purpose>. Returns <format> (<entity type>)."`
- **Parameter description**: `"<Purpose>. <Type>. Required/Optional. <Constraints>."`

Files to audit: `ProductTools.cs`, `CategoryTools.cs`, `SearchTools.cs`, `PythonTools.cs`

### Step 3: Update README.md

Restructure to include:
1. **Overview** section
2. **Prerequisites** (Docker, .NET 8 SDK)
3. **Quick Start** (5 steps: clone → build sandbox → run server → connect Cursor → call first tool)
4. **MCP Tool Catalog** — table of all 18 tools
5. **Project Structure** section
6. **Development Commands** section

### Step 4: Clean Up Source Files

Scan and remove all `// TODO`, `// FIXME`, placeholder strings, and dead code from non-test `.cs` files.

### Step 5: Final Build Verification

```bash
dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true
dotnet test MCPDemo.sln --filter "Category!=Integration&Category!=Docker"
```

Both must exit with code 0.

## Verify

```bash
# Check build is clean
dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true

# Run unit + infrastructure tests
dotnet test MCPDemo.sln --filter "Category!=Integration&Category!=Docker"

# Optionally run integration tests (requires internet + Docker)
dotnet test MCPDemo.sln --filter "Category=Integration"
dotnet test MCPDemo.sln --filter "Category=Docker"
```
