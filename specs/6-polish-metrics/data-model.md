# Data Model: Polish & Metrics Observability (Phase 6)

**Feature**: `6-polish-metrics`  
**Date**: 2026-03-10

---

## Entities

### ToolMetrics *(already implemented — read-only in this phase)*

Represents the aggregated runtime statistics for a single MCP tool for the current server session.

| Field | Type | Description |
|-------|------|-------------|
| `TotalCalls` | `int` | Number of times this tool has been invoked since server start |
| `SuccessCount` | `int` | Number of invocations that completed without error |
| `FailureCount` | `int` | Number of invocations that returned an error or threw |
| `AverageExecutionTimeMs` | `double` | Mean wall-clock execution time in milliseconds |
| `ErrorsByType` | `Dictionary<string, int>` | Breakdown of failure count by error type string (e.g., `"ValidationError": 3`) |

**Constraints**:
- `TotalCalls == SuccessCount + FailureCount` (invariant maintained by `InMemoryMetricsCollector`)
- `AverageExecutionTimeMs >= 0`
- `ErrorsByType` is empty when `FailureCount == 0`

### MetricsSummary *(output shape of `get_metrics`)*

The top-level return value of the `get_metrics` MCP tool.

| Shape | Type | Description |
|-------|------|-------------|
| Root | `IReadOnlyDictionary<string, ToolMetrics>` | Map of tool name (e.g., `"get_all_products"`) to its `ToolMetrics` |

**Serialization**: camelCase via `ToolJsonOptions.Default` (consistent with all other tools).

**Example output**:
```json
{
  "get_all_products": {
    "totalCalls": 5,
    "successCount": 4,
    "failureCount": 1,
    "averageExecutionTimeMs": 213.4,
    "errorsByType": { "ExternalApiException": 1 }
  },
  "run_python_code": {
    "totalCalls": 2,
    "successCount": 2,
    "failureCount": 0,
    "averageExecutionTimeMs": 1850.0,
    "errorsByType": {}
  }
}
```

---

## No New Entities

Phase 6 introduces no new domain entities, no new domain exceptions, and no new infrastructure services. All changes are:
1. A new MCP tool class (`MetricsTools.cs`) in the API layer
2. Description string updates in existing tool files
3. README and docs updates

---

## State & Lifecycle

- `ToolMetrics` entries are created lazily by `InMemoryMetricsCollector` on first `RecordExecution()` call for a given tool name.
- All metrics are **session-scoped**: they reset to zero when the MCP server process restarts.
- No persistence layer is used or needed for this phase.
