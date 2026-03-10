# Implementation Plan: MCP Server & C# Tool Definitions

**Branch**: `main` | **Date**: 2026-03-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/3-mcp-server-tools/spec.md`

## Summary

Bootstrap the MCP server using stdio transport and implement 16 MCP tool methods (8 product, 7 category, 1 search) as thin wrappers over the existing Application-layer services. Each tool delegates to the service, unwraps `Result<T>`, and returns serialized JSON (success) or plain error message (failure).

## Technical Context

**Language/Version**: C# 12, .NET 8  
**Primary Dependencies**: `ModelContextProtocol` SDK, `Microsoft.Extensions.Hosting`, `Serilog.AspNetCore`, `System.Text.Json`  
**Storage**: N/A (external API only)  
**Testing**: xUnit, NSubstitute, FluentAssertions  
**Target Platform**: Console app (stdio), Windows/Linux  
**Project Type**: MCP server (console app with stdio transport)  
**Performance Goals**: Server startup < 5s, tool call < 15s  
**Constraints**: No HTTP transport, stdio only. No exceptions thrown from tools.  
**Scale/Scope**: 16 MCP tools, single-user (1 AI assistant connection)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. MCP Tool Standards (definition contract) | ✅ Pass | All 16 tools have name, description, typed inputs, typed outputs, category, implementation type (C#) |
| II. API Calling & Efficiency (retry, error handling) | ✅ Pass | Already implemented in Phase 2. Tools delegate to services. |
| III. Logging & Metrics (file logging, 4 counters) | ✅ Pass | Serilog configured, InMemoryMetricsCollector integrated into services |
| IV. Clean Architecture (4 layers + Shared) | ✅ Pass | Tools in Api layer, services in Application, client in Infrastructure |
| V. Security & Sandboxing (validation, no secrets in logs) | ✅ Pass | Input validation in Application services. No Python tools in Phase 3. |
| Caching requirement (constitution II) | ⚠️ Deferred | Constitution mentions cache layer. Not in scope for Phase 3 (tools layer). Will be addressed separately. |

### Post-Design Re-Check

After completing Phase 1 design artifacts, all gates remain **✅ Pass**. The tool definitions follow the MCP SDK's `[McpServerToolType]` + `[McpServerTool]` pattern, all 16 tools adhere to the definition contract (name, description, inputs, outputs, category, implementation type), and the clean architecture dependency direction is preserved (tools in Api layer delegate to Application services).

## Project Structure

### Documentation (this feature)

```text
specs/3-mcp-server-tools/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── mcp-tool-contracts.md
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── MCPDemo.Api/
│   ├── Program.cs                  # MCP server bootstrap (stdio + DI)
│   └── McpTools/
│       ├── ProductTools.cs         # 8 product MCP tool methods
│       ├── CategoryTools.cs        # 7 category MCP tool methods
│       └── SearchTools.cs          # 1 search MCP tool method
│
├── MCPDemo.Application/            # (existing — services, interfaces, DTOs)
├── MCPDemo.Infrastructure/         # (existing — API client, metrics, logging)
├── MCPDemo.Domain/                 # (existing — entities, exceptions)
└── MCPDemo.Shared/                 # (existing — Result<T>, exceptions)
```

**Structure Decision**: Tools reside in `src/MCPDemo.Api/McpTools/` as static classes. No new projects needed. `Program.cs` is rewritten to bootstrap the MCP server with DI wiring.

## Complexity Tracking

No constitution violations requiring justification.
