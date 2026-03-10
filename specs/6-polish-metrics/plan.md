# Implementation Plan: Polish & Metrics Observability

**Feature**: `6-polish-metrics` | **Date**: 2026-03-10 | **Spec**: [spec.md](./spec.md)

## Summary

Add the `get_metrics` MCP tool to expose in-memory runtime metrics to the AI assistant. Polish all tool descriptions and parameters for production quality. Update README with Quick Start and full tool catalog. Verify zero-warning build.

## Technical Context

**Language/Version**: C# 12, .NET 8  
**Primary Dependencies**: `ModelContextProtocol` NuGet SDK, `System.Text.Json`, `Microsoft.Extensions.DependencyInjection`  
**Storage**: N/A — in-memory singleton (`InMemoryMetricsCollector`), no persistence  
**Testing**: xUnit, NSubstitute, FluentAssertions (existing suite must remain green)  
**Target Platform**: .NET 8 console (stdio MCP server)  
**Project Type**: MCP tool extension + documentation polish  
**Performance Goals**: `get_metrics` must respond in < 100ms (reads in-memory dictionary only)  
**Constraints**: No new projects, no new NuGet packages, no new infrastructure services  
**Scale/Scope**: 1 new tool, ~30 description string updates, 1 README rewrite

## Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. MCP Tool Standards — definition contract | ✅ | `get_metrics` has name, description, inputs (none), outputs (JSON), category (Reporting), impl type (C#) |
| I. MCP Tool Standards — category classification | ✅ | Category: Reporting |
| II. API Calling & Efficiency — no unnecessary API calls | ✅ | `get_metrics` reads in-memory only; no external calls |
| III. Logging & Metrics — runtime metrics exposed | ✅ | This is the primary deliverable of Phase 6 |
| III. Logging & Metrics — log entry per execution | ✅ | No logging needed for read-only metrics retrieval; existing tools already log |
| IV. Clean Architecture — tool in API layer | ✅ | `MetricsTools.cs` in `src/MCPDemo.Api/McpTools/` |
| V. Security & Sandboxing — input validation | ✅ | No inputs; no validation needed |
| V. Security & Sandboxing — no secrets in logs | ✅ | Metrics contain only tool names and counters; no sensitive data |

## Project Structure

### New Files (this feature)

```text
src/MCPDemo.Api/McpTools/
└── MetricsTools.cs          ← NEW: get_metrics tool

specs/6-polish-metrics/
├── spec.md
├── plan.md                  ← This file
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── get_metrics.md
```

### Modified Files (this feature)

```text
src/MCPDemo.Api/McpTools/
├── ProductTools.cs          ← Description audit & update
├── CategoryTools.cs         ← Description audit & update
├── SearchTools.cs           ← Description audit & update
└── PythonTools.cs           ← Description audit (minor, already good quality)

README.md                    ← Full rewrite with Quick Start + tool catalog
docs/implementation-plan.md ← Mark Phase 6 complete in roadmap table
```

## Implementation Phases

### User Story 1 — `get_metrics` MCP Tool

**Tasks**:
1. Create `src/MCPDemo.Api/McpTools/MetricsTools.cs` with `get_metrics` static method
2. Add `[McpServerToolType]`, `[McpServerTool]`, `[Description]` attributes
3. Inject `IMetricsCollector` via method parameter (auto-resolved by MCP SDK)
4. Serialize `GetAllMetrics()` via `ToolJsonOptions.Default`; catch all exceptions

**Acceptance Gate**: Build succeeds; calling `get_metrics` with no arguments returns valid JSON (or `{}`)

---

### User Story 2 — Description Audit & Polish

**Tasks**:
1. Audit `ProductTools.cs` — update all 8 tool `[Description]` + ~20 parameter `[Description]` strings
2. Audit `CategoryTools.cs` — update all 7 tool `[Description]` + ~14 parameter `[Description]` strings
3. Audit `SearchTools.cs` — update 1 tool `[Description]` + 8 parameter `[Description]` strings
4. Audit `PythonTools.cs` — minor review only (already high quality)

**Standard format**:
- Tool: `"<Purpose>. Returns <format> (<entity type>)."`
- Parameter: `"<Purpose>. <Type>. Required/Optional. <Constraints>."`

**Acceptance Gate**: No `[Description]` string contains placeholder text, lacks return format info, or omits optionality

---

### User Story 3 — README & Final Polish

**Tasks**:
1. Rewrite `README.md` with sections: Overview, Prerequisites, Quick Start, MCP Tool Catalog (all 18), Project Structure, Development Commands
2. Scan all non-test `.cs` files for TODO/FIXME/placeholder comments and remove
3. Run `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` → must exit code 0
4. Run `dotnet test MCPDemo.sln --filter "Category!=Integration&Category!=Docker"` → all pass
5. Update `docs/implementation-plan.md` roadmap table to mark Phase 6 ✅

**Acceptance Gate**: Build clean, all tests green, README reviewed against SC-002 through SC-006

## Complexity Tracking

No constitution violations. No additional complexity required.
