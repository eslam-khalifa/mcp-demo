# MCP Tool Contract: get_metrics

**Version**: 1.0.0  
**Feature**: `6-polish-metrics`  
**Date**: 2026-03-10  
**Category**: Reporting  
**Implementation Type**: C#

---

## Tool Definition

| Field | Value |
|-------|-------|
| **Name** | `get_metrics` |
| **Description** | Returns a JSON object of runtime metrics for all MCP tools called in this server session. Includes total calls, success count, failure count, average execution time (ms), and error breakdown by type per tool. Returns an empty object `{}` if no tools have been called yet. |
| **Category** | Reporting |
| **Implementation** | `MetricsTools.get_metrics` static method in `src/MCPDemo.Api/McpTools/MetricsTools.cs` |

---

## Inputs

This tool takes **no parameters**.

---

## Output

| Scenario | Return Type | Example |
|----------|-------------|---------|
| Tools have been called | JSON object | `{"get_all_products": {"totalCalls": 3, ...}}` |
| No tools called yet | Empty JSON object | `{}` |
| Internal error | Plain error string | `"Error: Failed to retrieve metrics."` |

**Return format**: JSON string (camelCase, via `ToolJsonOptions.Default`)  
**Entity type**: `IReadOnlyDictionary<string, ToolMetrics>`

---

## Error Handling

| Error Condition | Behaviour |
|-----------------|-----------|
| `IMetricsCollector` throws unexpectedly | Catch exception, return plain text `"Error: ..."` string — never rethrow |

---

## Acceptance Criteria

1. Called with no arguments → returns valid JSON object (or `{}`)
2. After other tools are invoked → per-tool counts are reflected accurately
3. After a tool failure → failure count and error type are included
4. Does not modify any metrics state (read-only)
5. Never throws; returns error string on failure
