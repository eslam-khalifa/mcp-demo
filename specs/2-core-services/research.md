# Research: Core Services & Infrastructure

**Feature**: `2-core-services`  
**Date**: 2026-03-09

## Decision 1: HTTP Client Pattern

**Decision**: Use `IHttpClientFactory` with typed client registration via
`AddHttpClient<IPlatziStoreApiClient, PlatziStoreApiClient>()`.

**Rationale**: `IHttpClientFactory` manages `HttpMessageHandler` lifetimes,
preventing socket exhaustion. Typed clients provide compile-time safety and
clean DI registration. The base address is configured at registration time.

**Alternatives Considered**:
- Raw `new HttpClient()` — rejected due to socket exhaustion risk.
- Named clients — rejected because typed clients offer stronger typing.
- Refit — rejected as it adds a dependency for limited benefit given our
  simple endpoint structure.

## Decision 2: Retry Strategy Implementation

**Decision**: Implement retry logic manually inside `PlatziStoreApiClient`
with a private `SendWithRetryAsync` method. Retry up to 2 times on 5xx
errors and timeouts with exponential backoff (500ms × attempt number).

**Rationale**: The retry logic is simple enough (3 conditions: success,
non-retriable error, max retries exceeded) that a manual implementation
is clearer and avoids adding Polly as a dependency. The implementation
plan explicitly shows this pattern.

**Alternatives Considered**:
- Polly NuGet — rejected to keep dependency count low for a demo project.
  Could be upgraded later if retry patterns become more complex.
- HttpClient middleware (DelegatingHandler) — viable but adds indirection
  for a simple 2-retry policy.

## Decision 3: JSON Serialization Configuration

**Decision**: Use `System.Text.Json` with `JsonSerializerOptions` configured
for `camelCase` property naming and case-insensitive deserialization. Share
a single `JsonSerializerOptions` instance across the API client.

**Rationale**: `System.Text.Json` is the default .NET serializer, requires
no additional NuGet packages, and performs well. The Platzi API uses camelCase
JSON properties.

**Alternatives Considered**:
- `Newtonsoft.Json` — rejected to avoid unnecessary dependency.
- Per-method options — rejected for performance (options should be cached).

## Decision 4: Serilog Configuration Approach

**Decision**: Configure Serilog in a static helper class
(`SerilogConfiguration.cs`) in the Infrastructure layer. Use
`CompactJsonFormatter` for structured output and `RollingInterval.Day`
for daily log rotation. Output to `logs/mcp-tool-.log`.

**Rationale**: Having a dedicated configuration class keeps logging setup
isolated from `Program.cs` and allows Infrastructure.Tests to verify the
configuration independently. The compact JSON format is machine-readable
and suitable for log aggregation.

**Alternatives Considered**:
- `appsettings.json` configuration — viable for a full ASP.NET app but
  overly complex for a stdio console MCP server.
- Console sink — rejected because MCP uses stdio for communication;
  console logging would corrupt the JSON-RPC stream.

## Decision 5: Metrics Collector Thread Safety

**Decision**: Use `ConcurrentDictionary<string, ToolMetrics>` with
`Interlocked` operations for atomic counter updates. The
`InMemoryMetricsCollector` is registered as a singleton.

**Rationale**: `ConcurrentDictionary` provides thread-safe key lookup
without explicit locking. `Interlocked.Increment` and
`Interlocked.Add` ensure atomic counter updates. This avoids lock
contention while maintaining accuracy.

**Alternatives Considered**:
- `lock` statements — simpler but creates contention under high
  concurrency.
- `Channel<T>` + background processor — overly complex for simple
  counter increments.
- `System.Diagnostics.Metrics` — good for production but adds complexity
  and requires a metrics listener for testing.

## Decision 6: Service Validation Pattern

**Decision**: Validate inputs at the top of each service method using
early-return `Result<T>.Failure()`. Validation happens BEFORE any API
client call. Empty update DTOs are rejected with "No fields to update".

**Rationale**: Early validation prevents unnecessary HTTP round-trips
and provides immediate feedback to callers. The Result pattern makes
validation failures explicit without throwing exceptions for expected
error conditions.

**Alternatives Considered**:
- FluentValidation library — overkill for the simple validation rules
  in this project.
- Data annotations — not applicable since DTOs are records, not
  entity models.
- Throwing exceptions — rejected because validation failure is an
  expected condition, not an exceptional one.

## Decision 7: API Client Interface Location

**Decision**: Define `IPlatziStoreApiClient` in the Infrastructure layer
(not Application). Services in Application receive it via constructor
injection.

**Rationale**: The API client is an infrastructure concern. However,
Application needs to reference it. We solve this by:
- Adding an Application → Infrastructure reference would violate Clean
  Architecture.
- Instead, we define the interface in Application (as `IExternalApiClient`
  or similar) and implement it in Infrastructure.

**CORRECTION**: After reviewing the dependency rules, the interface should
be defined in **Application** (as an abstraction), and the **implementation**
lives in Infrastructure. This follows the Dependency Inversion Principle.
Application depends on the abstraction; Infrastructure implements it.

**Final Decision**: Define `IPlatziStoreApiClient` in
`MCPDemo.Application/Interfaces/` and implement `PlatziStoreApiClient`
in `MCPDemo.Infrastructure/ExternalApi/`.

## Decision 8: Per-Request Timeout

**Decision**: Set `HttpClient.Timeout = 15 seconds` at the typed client
registration level. Timeouts are treated as retriable errors (same as 5xx).

**Rationale**: Clarified in spec (Q2). The Platzi API typically responds
in under 2 seconds. 15s provides generous headroom. Worst-case with retries:
~47 seconds total.
