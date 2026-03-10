---
description: "Task list for compiling Phase 6 - Polish & Metrics Observability feature."
---

# Tasks: 6-polish-metrics

**Input**: Design documents from `/specs/6-polish-metrics/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: No specific tests requested as per specification. Existing tests will be executed to make sure the app remains stable. 

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: Project initialization and basic structure.
Since this is an increment on top of existing project, no initialization tasks are required.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented.
Since the required dependencies (`IMetricsCollector`) are already populated, no new blocking foundation needs to be created.

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 — Get Runtime Metrics via MCP Tool (Priority: P1) 🎯 MVP

**Goal**: An AI assistant needs to inspect the current health and performance of the MCP server at runtime — how many times each tool has been called, the success/failure rates, average execution times, and any Python sandbox usage — without leaving the MCP tool interface.

**Independent Test**: Call the `get_metrics` MCP tool after making several other tool calls. Verify that the returned JSON reflects the correct call counts, success counts, and average times.

### Implementation for User Story 1

- [x] T001 [US1] Create the `get_metrics` static method in a new `MetricsTools` static class decorated with `[McpServerToolType]` in `src/MCPDemo.Api/McpTools/MetricsTools.cs`.
- [x] T002 [US1] Inject `IMetricsCollector` to `get_metrics` and implement returning `IMetricsCollector.GetAllMetrics()` serialized via `ToolJsonOptions.Default`.
- [x] T003 [US1] Add `[McpServerTool]` and `[Description]` attributes to the `get_metrics` method.
- [x] T004 [US1] Implement try/catch block inside `get_metrics` to return plain text error strings if `System.Text.Json` serialization fails.

**Checkpoint**: At this point, the `get_metrics` tool should be implemented, discoverable, and successfully report accurate runtime metrics as JSON strings.

---

## Phase 4: User Story 2 — Clean, Production-Ready Tool Descriptions (Priority: P1)

**Goal**: An AI assistant discovers and reads all MCP tool descriptions to understand when and how to use each tool. All tool descriptions must be precise, complete, and professional — matching the quality expected in a production MCP deployment.

**Independent Test**: Review all `[Description]` attributes on tools and parameters. Verify each clearly states purpose, input constraints, and expected output format.

### Implementation for User Story 2

- [x] T005 [P] [US2] Audit and update `[Description]` attributes for all 8 tools and ~20 parameters in `src/MCPDemo.Api/McpTools/ProductTools.cs`.
- [x] T006 [P] [US2] Audit and update `[Description]` attributes for all 7 tools and ~14 parameters in `src/MCPDemo.Api/McpTools/CategoryTools.cs`.
- [x] T007 [P] [US2] Audit and update `[Description]` attributes for the 1 tool and 8 parameters in `src/MCPDemo.Api/McpTools/SearchTools.cs`.
- [x] T008 [P] [US2] Audit and update `[Description]` attributes for the 1 tool and 2 parameters in `src/MCPDemo.Api/McpTools/PythonTools.cs` (minor review only).

**Checkpoint**: At this point, User Stories 1 AND 2 should both be completed. Tools should feature clear description matching the defined standardization format.

---

## Phase 5: User Story 3 — Clean Solution Build & Documentation (Priority: P2)

**Goal**: A new developer joining the project should be able to clone the repository, read the README, and have the MCP server running in Cursor IDE within 5 minutes. The solution must build with zero errors and zero warnings.

**Independent Test**: Run `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` and verify it exits with code 0. Then follow README instructions independently.

### Implementation for User Story 3

- [x] T009 [P] [US3] Rewrite `README.md` containing sections: Overview, Prerequisites, Quick Start, MCP Tool Catalog (all 18 tools), Project Structure, and Development Commands.
- [x] T010 [P] [US3] Scan and remove all `TODO`, `FIXME`, placeholder strings, and dead code from all non-test `.cs` files across the solution.
- [x] T011 [P] [US3] Update Roadmap table in `docs/implementation-plan.md` to flag Phase 6 as completed.
- [x] T012 [US3] Final verification: Run `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` and confirm a clean 0 warnings build, followed by `dotnet test MCPDemo.sln --filter "Category!=Integration&Category!=Docker"` to ensure no regressions occur.

**Checkpoint**: Codebase is clean, 0 warnings thrown during build, tests are fully passing, and comprehensive README documentation matches application status.

---

## Dependencies & Execution Order

### Phase Dependencies

- **User Story 1 (Phase 3)**: Has no blockers. Starts directly.
- **User Story 2 (Phase 4)**: Has no blockers. Starts directly in parallel if needed.
- **User Story 3 (Phase 5)**: Depends on US1 being completed because `get_metrics` needs to be accounted for in the README list.

### Within Each User Story

- Tasks related to the code functionality (`get_metrics` additions) should be handled prior to documentation to ensure correct representations in docs.

### Parallel Opportunities

- Due to different file targets, tasks under **US2 (T005 - T008)** are independently executable and can be parallelized.
- Some tasks in **US3 (T009 - T011)** can be executed simultaneously.

---

## Implementation Strategy

### Incremental Delivery

1. Implement `get_metrics` (US1) as the baseline for this phase. Test manually using IDE or automated test suites (implicitly via app usage/demo).
2. Clean up all the parameter and endpoint tool descriptions (US2).
3. Draft documentation and remove all code placeholders/TODOs (US3).
4. Do a final verification run using `dotnet build` and `dotnet test`.

---

## Notes

- Keep track of `[Description]` pattern: `"<Purpose>. Returns <format> (<entity type>)."` (Tools) and `"<Purpose>. <Type>. Required/Optional. <Constraints>."` (Params).
- The returned Object for the `get_metrics` method should be correctly serialized via `ToolJsonOptions.Default`.
