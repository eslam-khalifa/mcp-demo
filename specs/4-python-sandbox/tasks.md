# Tasks: Python Sandbox & Custom Code Execution

**Input**: Design documents from `specs/4-python-sandbox/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/run-python-code-tool.md, quickstart.md

**Tests**: Not explicitly requested in the feature specification. Test tasks are excluded.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Docker & Python Sandbox)

**Purpose**: Create the Docker sandbox image and Python entry point — the foundation that all user stories depend on.

- [X] T001 Create `docker/python-sandbox/requirements.txt` with `pandas==2.2.0` and `numpy==1.26.3`
- [X] T002 Create `docker/python-sandbox/Dockerfile` based on `python:3.11-slim` — copies `requirements.txt`, installs deps, copies `main.py`, sets entrypoint to `python main.py`
- [X] T003 Create `docker/python-sandbox/main.py` — general-purpose code executor that reads `{"code": "...", "data": "..."}` from stdin, parses the data payload, executes code via `exec()` with restricted `__builtins__` and `json`, `pd`, `np`, `data` in the namespace, captures output via `print()`, and handles errors with `try/except` returning `{"error": "..."}` on failure
- [X] T004 Build the Docker image: `docker build -t mcp-python-sandbox docker/python-sandbox/` — verify it builds successfully
- [X] T005 Verify the Docker image works standalone: `echo '{"code": "print(42)", "data": "null"}' | docker run --rm -i --memory=256m --cpus=0.5 --network=none --read-only --security-opt=no-new-privileges mcp-python-sandbox` — expected output: `42`

**Checkpoint**: Docker sandbox image is built and verified. Python code can be executed in isolation. All subsequent phases depend on this.

---

## Phase 2: Foundational (C# Infrastructure)

**Purpose**: Create the C# interfaces and services that bridge the MCP server to the Docker sandbox. MUST be complete before any MCP tool can be implemented.

- [X] T006 [P] Create `IPythonSandboxService` interface in `src/MCPDemo.Application/Interfaces/IPythonSandboxService.cs` — single method: `Task<string> ExecuteAsync(string code, string jsonData)` that executes Python code in the Docker sandbox and returns stdout
- [X] T007 [P] Create `IDockerProcessRunner` interface and `ProcessResult` record in `src/MCPDemo.Infrastructure/PythonSandbox/DockerProcessRunner.cs` — abstracts `System.Diagnostics.Process` for testability; method: `Task<ProcessResult> RunAsync(string arguments, string stdinInput, CancellationToken cancellationToken)`; `ProcessResult` has `ExitCode`, `StandardOutput`, `StandardError`
- [X] T008 Implement `DockerProcessRunner` class in `src/MCPDemo.Infrastructure/PythonSandbox/DockerProcessRunner.cs` — implements `IDockerProcessRunner` using `System.Diagnostics.Process` to run `docker` commands with stdin/stdout/stderr redirection
- [X] T009 Implement `PythonSandboxService` in `src/MCPDemo.Infrastructure/PythonSandbox/PythonSandboxService.cs` — implements `IPythonSandboxService`; builds JSON payload `{"code": "...", "data": "..."}`, calls `IDockerProcessRunner.RunAsync` with Docker flags (`--rm -i --memory=256m --cpus=0.5 --network=none --read-only --security-opt=no-new-privileges mcp-python-sandbox`), enforces 30-second timeout via `CancellationTokenSource`, logs execution via `ILogger`, throws `PythonSandboxException` on non-zero exit code
- [X] T010 Create `PythonSandboxException` class in `src/MCPDemo.Shared/Exceptions/PythonSandboxException.cs` — inherits from `Exception`, used for sandbox execution failures
- [X] T011 Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: Infrastructure is ready. `IPythonSandboxService` can execute Python code via Docker. MCP tool implementation can now begin.

---

## Phase 3: User Story 1 — Run Custom Analytics on Store Data (Priority: P1) 🎯 MVP

**Goal**: Expose the `run_python_code` MCP tool so the AI assistant can execute custom Python code against store data fetched by existing tools.

**Independent Test**: Call `run_python_code(code="print(42)")` and verify it returns `"42"`. Then call with `data` containing product JSON and code that processes it.

### Implementation for User Story 1

- [X] T012 [US1] Implement `run_python_code` MCP tool in `src/MCPDemo.Api/McpTools/PythonTools.cs` — static class with `[McpServerToolType]`; single method `run_python_code` with `[McpServerTool]` and `[Description]`; parameters: `string code` (required), `string? data = null` (optional JSON payload); validates code is not empty; calls `IPythonSandboxService.ExecuteAsync(code, data ?? "null")`; returns stdout on success or error message on failure using `Result<T>` pattern
- [X] T013 [US1] Register `IPythonSandboxService` and `IDockerProcessRunner` in `src/MCPDemo.Api/Program.cs` — add `builder.Services.AddScoped<IPythonSandboxService, PythonSandboxService>()` and `builder.Services.AddScoped<IDockerProcessRunner, DockerProcessRunner>()`
- [X] T014 [US1] Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings
- [X] T015 [US1] Verify tool is discoverable: MCP tool listing should now return 17 tools (16 existing + `run_python_code`)

**Checkpoint**: `run_python_code` is registered and functional. AI assistant can execute custom Python data analysis code. MVP is complete.

---

## Phase 4: User Story 2 — Secure Execution Environment (Priority: P1)

**Goal**: Verify and harden the sandbox so that untrusted code cannot access the network, filesystem, or consume excessive resources.

**Independent Test**: Submit code that attempts network access, file read/write, or infinite loops — all should fail gracefully.

### Implementation for User Story 2

- [X] T016 [US2] Verify Docker security flags are enforced in `PythonSandboxService` — confirm the `docker run` arguments include `--network=none`, `--read-only`, `--memory=256m`, `--cpus=0.5`, `--security-opt=no-new-privileges`, `--rm`
- [X] T017 [US2] Verify restricted Python builtins in `docker/python-sandbox/main.py` — confirm `exec_globals["__builtins__"]` excludes `open`, `exec`, `eval`, `__import__`, `compile`, `globals`, `locals`, `getattr`, `setattr`, `delattr`, `dir`, `vars`, `breakpoint`, `exit`, `quit`, `input`
- [X] T018 [US2] Verify timeout enforcement in `PythonSandboxService` — confirm `CancellationTokenSource` is set to 30 seconds and the process is killed on timeout
- [X] T019 [US2] Manual verification: test that `run_python_code(code="import urllib.request; urllib.request.urlopen('http://example.com')")` returns an error (network blocked)
- [X] T020 [US2] Manual verification: test that `run_python_code(code="open('/etc/passwd').read()")` returns an error (builtin `open` blocked)

**Checkpoint**: Sandbox security is verified. All resource limits and access restrictions are enforced.

---

## Phase 5: User Story 3 — Error Handling and Feedback (Priority: P2)

**Goal**: Ensure that Python errors (syntax, runtime, validation) produce clear, actionable error messages for the AI assistant to self-correct.

**Independent Test**: Submit code with various error types and verify descriptive error messages are returned.

### Implementation for User Story 3

- [X] T021 [US3] Add input validation in `PythonTools.cs` — if `code` is null, empty, or whitespace, return an error message immediately without invoking the sandbox
- [X] T022 [US3] Add JSON validation for `data` parameter in `PythonTools.cs` — if `data` is provided but not valid JSON, return a descriptive parse error before invoking the sandbox
- [X] T023 [US3] Verify Python error propagation in `main.py` — confirm that syntax errors, runtime exceptions (e.g., `ZeroDivisionError`), and `NameError` are caught by `try/except` and returned as `{"error": "<type>: <message>"}`
- [X] T024 [US3] Verify Docker-level error handling in `PythonSandboxService` — confirm that non-zero exit codes from Docker (OOM kill, image not found) are wrapped in `PythonSandboxException` with descriptive messages
- [X] T025 [US3] Add logging for `run_python_code` executions in `PythonTools.cs` — log tool invocation, execution time, success/failure, and error type using Serilog; record execution in `IMetricsCollector`
- [X] T026 [US3] Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: Error handling is robust. All error scenarios produce actionable messages.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, cleanup, and documentation.

- [X] T027 Verify full solution build with warnings-as-errors: `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` — zero errors, zero warnings
- [X] T028 [P] Verify `run_python_code` tool method has `[Description]` attributes on both the method and all parameters in `src/MCPDemo.Api/McpTools/PythonTools.cs`
- [X] T029 [P] Add XML documentation comments to all public methods in `src/MCPDemo.Api/McpTools/PythonTools.cs`, `src/MCPDemo.Application/Interfaces/IPythonSandboxService.cs`, `src/MCPDemo.Infrastructure/PythonSandbox/PythonSandboxService.cs`, and `src/MCPDemo.Infrastructure/PythonSandbox/DockerProcessRunner.cs`
- [X] T030 [P] Verify `Program.cs` registers all required DI services — cross-check with `data-model.md` DI registration table
- [X] T031 Run `quickstart.md` validation: all key files exist, Docker image builds, solution builds, no architecture violations

**Checkpoint**: Phase 4 feature complete. 17 MCP tools operational (16 C# + 1 Python sandbox). Solution builds with zero warnings.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1: Custom Analytics (Phase 3)**: Depends on Foundational (needs `IPythonSandboxService` + Docker image)
- **US2: Secure Execution (Phase 4)**: Depends on US1 (verifies the running system)
- **US3: Error Handling (Phase 5)**: Depends on US1 (adds validation/logging to existing tool)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 → Phase 2 → US1 (P1, MVP) → US2 (P1, security verification) → US3 (P2, error hardening) → Polish
```

- **US1 (Custom Analytics)**: MVP — implement first. Creates the tool and registers it.
- **US2 (Secure Execution)**: Verification phase — confirms security is enforced. Must run after US1 since it tests the running system.
- **US3 (Error Handling)**: Adds validation, logging, and error propagation improvements. Depends on US1 for the base tool.

### Parallel Opportunities

**Within Phase 1**: T001 and T002 can run in parallel (different files)

**Within Phase 2**: T006 and T007 can run in parallel (different projects/files)

**Within Phase 4 (US2)**: T016, T017, T018 are independent verification tasks — parallelizable

**Within Phase 6 (Polish)**: T028, T029, T030 can run in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (Docker image + main.py)
2. Complete Phase 2: Foundational (C# interfaces + services)
3. Complete Phase 3: US1 — `run_python_code` MCP tool
4. **STOP and VALIDATE**: Call `run_python_code(code="print(42)")` — should return `"42"`
5. The AI assistant can now run custom data analysis — this is the MVP

### Incremental Delivery

1. Setup + Foundational → Docker sandbox operational (no MCP tool yet)
2. US1 (Custom Analytics) → AI can run Python code (MVP!)
3. US2 (Secure Execution) → Security verified and hardened
4. US3 (Error Handling) → Robust error messages for self-correction
5. Polish → Production readiness

---

## Notes

- All C# source files follow the existing Clean Architecture patterns from Phase 3
- `PythonTools.cs` follows the same MCP tool pattern as `ProductTools.cs`, `CategoryTools.cs`, `SearchTools.cs`
- The `run_python_code` tool uses the same `Result<T>` unwrapping and JSON serialization approach
- Docker image name `mcp-python-sandbox` is hardcoded in `PythonSandboxService`
- The `data` parameter is optional — some scripts don't need external data
- Total tasks: 31
