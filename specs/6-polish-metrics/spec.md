# Feature Specification: Polish & Metrics Observability

**Feature**: `6-polish-metrics`  
**Created**: 2026-03-10  
**Status**: Draft  
**Input**: Phase 6 from implementation-plan.md — Add `get_metrics` MCP tool, final code review, and solution cleanup.

## Clarifications

### Session 2026-03-10

- Q: What is the correct total MCP tool count? → A: 18 tools (16 CRUD/Search + `run_python_code` + `get_metrics`). The "17" in FR-011 was a typo.

## Assumptions

- Phases 1–5 are fully complete: The MCP server runs on stdio, all 16 product/category/search tools are implemented, the Python sandbox tool (`run_python_code`) is operational, and the full test suite is passing.
- `IMetricsCollector` (`InMemoryMetricsCollector`) is already implemented and registered as a singleton in DI. All existing tools already call `RecordExecution()`.
- The `get_metrics` tool will be a **read-only** observability tool. It does NOT modify metrics state.
- "Final code review and cleanup" means removing TODO/debug comments, ensuring all tool descriptions are production-ready, and verifying the solution builds cleanly with zero warnings.
- The MCP server documentation in `README.md` and `docs/` must be sufficient for a new developer to run the server in Cursor IDE without additional guidance.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Get Runtime Metrics via MCP Tool (Priority: P1)

An AI assistant needs to inspect the current health and performance of the MCP server at runtime — how many times each tool has been called, the success/failure rates, average execution times, and any Python sandbox usage — without leaving the MCP tool interface.

**Why this priority**: Observability is the core deliverable of this phase. Without the `get_metrics` tool, the AI cannot self-diagnose issues (e.g., a high failure rate on a specific tool), and the project's constitution requirement for runtime metrics exposure remains unfulfilled.

**Independent Test**: Call the `get_metrics` MCP tool after making several other tool calls. Verify that the returned JSON reflects the correct call counts, success counts, and average times.

**Acceptance Scenarios**:

1. **Given** the MCP server has handled at least one tool call, **When** `get_metrics` is called with no arguments, **Then** a JSON object containing per-tool metrics (total calls, success count, failure count, average execution time, error breakdown) is returned.
2. **Given** a tool has never been called, **When** `get_metrics` is called, **Then** that tool either does not appear in results or appears with zero counts.
3. **Given** a tool has failed at least once, **When** `get_metrics` is called, **Then** the failure count and error type breakdown are correctly reflected.
4. **Given** `run_python_code` has been called multiple times, **When** `get_metrics` is called, **Then** the Python sandbox execution count is recorded separately and accurately.

---

### User Story 2 — Clean, Production-Ready Tool Descriptions (Priority: P1)

An AI assistant discovers and reads all MCP tool descriptions to understand when and how to use each tool. All tool descriptions must be precise, complete, and professional — matching the quality expected in a production MCP deployment.

**Why this priority**: Tool descriptions are the primary interface between the AI and the server. Poor or incomplete descriptions cause the AI to misuse or skip tools.

**Independent Test**: Review all `[Description]` attributes on tools and parameters. Verify each clearly states purpose, input constraints, and expected output format.

**Acceptance Scenarios**:

1. **Given** any MCP tool, **When** an AI assistant reads its description, **Then** the description unambiguously explains the tool's purpose, what its inputs mean, and what the output looks like.
2. **Given** any tool parameter, **When** an AI assistant reads its `[Description]` attribute, **Then** the parameter's type, valid range/format, and whether it is optional are all clear.
3. **Given** a tool that returns JSON, **When** an AI assistant reads its description, **Then** the description explicitly mentions the return format (JSON object or JSON array) and the entity type it contains.

---

### User Story 3 — Clean Solution Build & Documentation (Priority: P2)

A new developer joining the project should be able to clone the repository, read the README, and have the MCP server running in Cursor IDE within 5 minutes. The solution must build with zero errors and zero warnings.

**Why this priority**: Build cleanliness and documentation are gates to maintainability. Without them, Phase 6 cannot be considered "done."

**Independent Test**: Run `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` and verify it exits with code 0. Then follow README instructions independently.

**Acceptance Scenarios**:

1. **Given** a clean checkout of the repository, **When** `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` is run, **Then** it exits with zero errors and zero warnings.
2. **Given** the README, **When** a developer follows the setup steps, **Then** they can connect the MCP server to Cursor IDE and invoke at least one tool successfully.
3. **Given** the `quickstart.md` in `docs/`, **When** a developer follows the test commands, **Then** all test categories (unit, integration, Docker) execute and report results without additional configuration.
4. **Given** the project, **When** all source files are reviewed, **Then** no TODO, FIXME, placeholder text, or debug-only code remains in any non-test file.

---

### Edge Cases

- What if `get_metrics` is called before any other tool has been used? → Return an empty metrics object (empty dictionary), not an error.
- What if `InMemoryMetricsCollector` has grown very large (thousands of entries)? → The tool must still return all entries without truncation; no pagination is required for this in-memory store.
- What if a `[Description]` attribute is missing on a newly added parameter? → This must be caught during the final code review and corrected before the phase is closed.
- What if the README setup instructions reference a file or command that no longer exists? → All README references must be validated against the actual repository state.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose a `get_metrics` MCP tool in `src/MCPDemo.Api/McpTools/` that returns a JSON object representing all current runtime metrics from `IMetricsCollector.GetAllMetrics()`.
- **FR-002**: The `get_metrics` tool MUST be decorated with `[McpServerTool]` and a clear `[Description]` attribute, and it MUST be auto-discovered by `WithToolsFromAssembly()`.
- **FR-003**: The `get_metrics` tool MUST receive `IMetricsCollector` via DI method injection (consistent with all other tools).
- **FR-004**: The `get_metrics` tool MUST serialize the metrics dictionary using `System.Text.Json` with camelCase naming and return it as a JSON string.
- **FR-005**: The `get_metrics` tool MUST NOT throw exceptions; any error must be returned as a plain text error string.
- **FR-006**: All existing tool `[Description]` attributes MUST be reviewed and updated to include: purpose, return format (JSON object/array), and entity type returned.
- **FR-007**: All existing tool parameter `[Description]` attributes MUST explicitly state whether the parameter is optional or required, its type, and any constraints (e.g., "must be a positive integer").
- **FR-008**: The solution MUST build with `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` and exit with code 0 (zero errors, zero warnings).
- **FR-009**: All TODO, FIXME, commented-out code blocks, and placeholder strings MUST be removed from non-test source files.
- **FR-010**: `README.md` MUST include a "Quick Start" section with step-by-step instructions for: cloning the repo, building the sandbox Docker image, running the MCP server, and connecting it to Cursor IDE.
- **FR-011**: `README.md` MUST include a complete list of all 18 MCP tools (16 CRUD/Search + `run_python_code` + `get_metrics`) with a one-line description of each.
- **FR-012**: `docs/implementation-plan.md` MUST be updated to mark Phase 6 as complete in the Implementation Roadmap table.

### Key Entities

- **MetricsSummary**: The output of `get_metrics` — a map of tool name to `ToolMetrics` (total calls, success count, failure count, average execution time ms, errors by type).
- **MCP Tool Description**: The human-readable text in `[Description]` attributes — the primary discovery interface for AI assistants.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The `get_metrics` tool is discoverable and returns valid JSON within 100ms — verifiable by calling it immediately after server startup.
- **SC-002**: All 18 MCP tools (16 CRUD/Search + `run_python_code` + `get_metrics`) are listed in the README with accurate one-line descriptions.
- **SC-003**: `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` exits with code 0 — zero errors, zero compiler warnings.
- **SC-004**: All 44+ tests (unit + integration) continue to pass after Phase 6 changes — verified by `dotnet test MCPDemo.sln --filter "Category!=Integration&Category!=Docker"`.
- **SC-005**: A developer with no prior knowledge of the project can follow only the README to start the MCP server and invoke a tool in Cursor IDE in under 5 minutes.
- **SC-006**: No `[Description]` attribute on any tool or parameter is empty, uses placeholder text, or lacks return format information.
