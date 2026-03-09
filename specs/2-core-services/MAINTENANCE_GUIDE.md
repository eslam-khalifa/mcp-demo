# Maintenance Guide: Phase 2 — Core Services

This document provides technical details for maintaining the Core Services and Infrastructure layer.

## 1. Metrics & Observability

### `IMetricsCollector`
Metrics are collected in-memory per-tool. To retrieve the current state:
- Use `IMetricsCollector.GetMetrics(toolName)` for specific tool data.
- Use `IMetricsCollector.GetAllMetrics()` for a full system snapshot.

**Data Structure (`ToolMetrics`):**
- `TotalCalls`, `SuccessCount`, `FailureCount`
- `AverageExecutionTimeMs`
- `ErrorsByType`: Dictionary of exception names mapping to their occurrence count.

### Logging
Logs are written to `logs/mcp-tool-.log` using Serilog's `CompactJsonFormatter`.
- **Level**: Information (Application), Warning (Microsoft/System).
- **Format**: Structured JSON for easy parsing by external tools like Seq or ELK.

## 2. Shared Exceptions

- `EntityNotFoundException`: Thrown when a resource isn't found (mapped from 404).
- `ExternalApiException`: Thrown for all other non-success API responses after retries are exhausted.
- `McpToolException`: Base for tool-specific errors.

## 3. Configuration Points

- **API Base URL**: Configured via `HttpClient` in Dependency Injection (usually in `Program.cs`).
- **Retry Strategy**: Hardcoded in `PlatziStoreApiClient` (2 retries, 500ms backoff).
- **Timeouts**: 15 seconds per request (enforced via `HttpClient.Timeout`).
