# Research: MCP Server & C# Tool Definitions

**Date**: 2026-03-10  
**Feature**: `3-mcp-server-tools`

## Research Summary

All technical unknowns have been resolved. Phase 3 builds on top of well-established Phase 2 components (services, API client, metrics, logging) using the official ModelContextProtocol C# SDK.

---

## R1: MCP SDK Package & API

**Decision**: Use `ModelContextProtocol` NuGet package (main package, includes hosting + DI extensions).

**Rationale**: The main package includes `WithStdioServerTransport()` and `WithToolsFromAssembly()` extensions needed for stdio-based discovery. The Core-only package lacks hosting extensions. The AspNetCore package is for HTTP transport (not needed).

**Alternatives considered**:
- `ModelContextProtocol.Core` — Too low-level, requires manual wiring.
- `ModelContextProtocol.AspNetCore` — HTTP transport, not applicable (stdio required).

---

## R2: Host Builder Pattern

**Decision**: Use `Host.CreateApplicationBuilder(args)` with `builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`.

**Rationale**: This is the standard .NET 8 pattern for console apps with DI. The MCP SDK integrates seamlessly with `IHostBuilder`. Stdio transport is configured as a single extension method call.

**Alternatives considered**:
- Manual `ServiceProvider` — No hosting lifetime management, harder to integrate Serilog.
- `WebApplication.CreateBuilder` — Overkill for stdio, adds unnecessary HTTP pipeline.

---

## R3: Tool Definition Pattern

**Decision**: Static classes with `[McpServerToolType]` attribute, static methods with `[McpServerTool]` and `[Description]` attributes. Services injected via method parameters (not constructor).

**Rationale**: The MCP SDK resolves method parameters from the DI container at invocation time. Static classes avoid lifetime management. `[Description]` attributes enable AI discoverability.

**Alternatives considered**:
- Instance-based tool classes — Not supported by the SDK's `WithToolsFromAssembly()` scanning.

---

## R4: JSON Serialization Strategy

**Decision**: Use `System.Text.Json.JsonSerializer.Serialize()` with `JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }` for success payloads. Failures return plain `string`.

**Rationale**: CamelCase is the standard for JSON consumed by AI assistants. Plain string for errors avoids JSON-in-JSON confusion and aligns with clarification session decision.

**Alternatives considered**:
- JSON error objects — Rejected per clarification (Option A selected).
- Newtonsoft.Json — Not needed; System.Text.Json is sufficient and already used.

---

## R5: Error Handling in Tools

**Decision**: Each tool wraps its service call in a try/catch. On `Result<T>.IsSuccess`, serialize the value. On failure, return `result.ErrorMessage`. On unexpected exceptions, return the exception message (never throw).

**Rationale**: The MCP SDK expects tool methods to return a string. Thrown exceptions would crash the tool invocation and return unhelpful error messages to the AI.

**Alternatives considered**:
- Let exceptions propagate — SDK would return generic error, not actionable for AI.

---

## R6: Required NuGet Packages for MCPDemo.Api

**Decision**: Add the following packages to `MCPDemo.Api.csproj`:
- `ModelContextProtocol` — MCP SDK
- `Microsoft.Extensions.Hosting` — Host builder
- `Serilog.AspNetCore` — Serilog integration with hosting

**Rationale**: These are the minimum set needed for a functional MCP server with logging. `Microsoft.Extensions.Hosting` provides the DI container, lifetime management, and `Host.CreateApplicationBuilder`. Serilog.AspNetCore integrates with the hosting pipeline.

**Alternatives considered**:
- Manual Serilog wiring — Less integrated, more boilerplate.
