# Research: Polish & Metrics Observability (Phase 6)

**Feature**: `6-polish-metrics`  
**Date**: 2026-03-10  
**Spec**: [spec.md](./spec.md)

---

## Decision 1: `get_metrics` Tool Placement

**Decision**: Place `get_metrics` in a new `MetricsTools.cs` file inside `src/MCPDemo.Api/McpTools/`.

**Rationale**: Consistent with the existing pattern — each domain/concern has its own tool file (`ProductTools.cs`, `CategoryTools.cs`, `SearchTools.cs`, `PythonTools.cs`). Metrics is a cross-cutting observability concern and deserves its own file rather than being appended to an existing one.

**Alternatives considered**:
- Appending to `PythonTools.cs` — rejected: different domain concern; reduces cohesion.
- Placing in `Infrastructure` layer — rejected: violates Clean Architecture; tools belong to the API layer.

---

## Decision 2: `get_metrics` Return Shape

**Decision**: Return `IMetricsCollector.GetAllMetrics()` serialized directly via `ToolJsonOptions.Default` (the existing `JsonSerializerOptions` with camelCase) as a JSON string.

**Rationale**: `GetAllMetrics()` returns `IReadOnlyDictionary<string, ToolMetrics>` which is directly serializable. Reusing the existing `ToolJsonOptions.Default` ensures consistency with all other tool return values.

**Alternatives considered**:
- Wrapping in a container `{ "metrics": {...}, "timestamp": "..." }` — may add value but adds complexity beyond spec requirements; deferred to future enhancement.

---

## Decision 3: Metrics Reset Policy

**Decision**: Metrics are **not reset** on each `get_metrics` call. They persist for the lifetime of the process (server session). Restarting the MCP server resets all counters to zero (in-memory only, no persistence).

**Rationale**: `InMemoryMetricsCollector` is registered as a singleton. The spec explicitly states `get_metrics` is read-only. No requirement for persistence or reset exists.

---

## Decision 4: README Structure

**Decision**: Restructure `README.md` with these top-level sections:
1. **Overview** — what the project does
2. **Quick Start** — 5-step setup (clone → Docker build → server run → Cursor connect → first tool call)
3. **MCP Tool Catalog** — table of all 18 tools with one-line descriptions
4. **Project Structure** — layer map
5. **Development** — build, test, coverage commands
6. **Testing** — reference to `specs/5-testing/quickstart.md`

**Rationale**: The current README is minimal (29 lines). The spec requires a Quick Start section and full tool catalog. The proposed structure is a common pattern for developer-facing MCP servers.

---

## Decision 5: Description Audit Scope

**Decision**: Audit and update `[Description]` attributes in all 4 existing tool files:
- `ProductTools.cs` — 8 tool methods + ~20 parameters
- `CategoryTools.cs` — 7 tool methods + ~14 parameters  
- `SearchTools.cs` — 1 tool method + 8 parameters
- `PythonTools.cs` — 1 tool method + 2 parameters (already high quality)

**Standard format for tool descriptions**:
> `"<Purpose>. Returns <format> (<entity type>)."`

**Standard format for parameter descriptions**:
> `"<Purpose>. <Type>. <Required/Optional>. <Constraints if any>."`
