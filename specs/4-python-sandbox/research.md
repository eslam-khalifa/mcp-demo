# Research: Python Sandbox & Custom Code Execution

**Feature**: `4-python-sandbox`  
**Date**: 2026-03-10  
**Purpose**: Resolve all technical unknowns and document research decisions for Phase 4 implementation.

## Research Topics

### 1. Docker CLI Invocation from C# — Best Practices

**Decision**: Use `System.Diagnostics.Process` to invoke `docker run` directly from C#, wrapped in a testable abstraction (`IDockerProcessRunner`).

**Rationale**:
- `System.Diagnostics.Process` is the standard .NET approach for spawning external processes.
- No external NuGet packages are needed (e.g., `Docker.DotNet` uses the Docker API socket, which adds complexity and would require the Docker daemon socket to be accessible).
- CLI invocation is simple, transparent, and mirrors manual Docker usage.
- Wrapping in `IDockerProcessRunner` enables unit testing with mocked process output.

**Alternatives considered**:
- **Docker.DotNet SDK**: Provides full Docker API access but is overkill for a single `docker run` command. Adds a package dependency and requires Docker socket configuration.
- **Shell script wrapper**: Would work but adds an unnecessary layer of indirection and complicates error handling from C#.

---

### 2. Python Code Execution in a Restricted Namespace

**Decision**: Use Python's `exec()` with a custom `exec_globals` dictionary that provides only safe builtins, `json`, `pandas` (as `pd`), and `numpy` (as `np`). The `data` variable is pre-parsed from the JSON payload.

**Rationale**:
- `exec()` with a restricted `__builtins__` dictionary is the standard Python approach for controlled code execution.
- Removing dangerous builtins like `open`, `__import__`, `eval`, `compile`, `globals`, `locals` prevents filesystem and module access at the Python level.
- Docker-level restrictions (`--network=none`, `--read-only`) provide a second layer of defense even if the Python-level restrictions are bypassed.
- Pre-parsing `data` from JSON means the AI-generated code can use it directly without boilerplate.

**Alternatives considered**:
- **RestrictedPython**: A third-party library that compiles Python code with restrictions. Adds complexity and a dependency. Docker isolation already provides strong security.
- **subprocess inside Docker**: Running a separate Python process inside Docker adds latency and complexity without meaningful security benefit (Docker is already the sandbox).

---

### 3. Docker Image Build Strategy

**Decision**: Pre-build the Docker image (`mcp-python-sandbox`) as a one-time setup step. The image is NOT built at runtime.

**Rationale**:
- Building during MCP server startup would add 30-60 seconds of latency on first run and require network access to pull the base image.
- A pre-built image ensures deterministic behavior and fast container startup (~200ms).
- The Dockerfile installs only `pandas` and `numpy` on top of `python:3.11-slim`, keeping the image small (~250 MB).

**Alternatives considered**:
- **Runtime image build**: Auto-build on first `run_python_code` call. Rejected because it couples tool execution with image build lifecycle and introduces unpredictable startup latency.
- **Using a pre-configured Python virtualenv on host**: Eliminates Docker but loses all sandboxing guarantees. Not acceptable per constitution Section V.

---

### 4. Timeout and Resource Enforcement

**Decision**: Use Docker flags for memory/CPU limits and a C# `CancellationTokenSource` for the 30-second timeout.

**Rationale**:
- Docker's `--memory=256m` and `--cpus=0.5` flags are enforced by the kernel (cgroups) and cannot be bypassed by the Python code.
- The 30-second timeout is enforced by C# reading stdout asynchronously with a cancellation token. If the token fires, the process is killed.
- Docker's `--rm` flag ensures containers are cleaned up after execution.

**Alternatives considered**:
- **Python-level timeout (`signal.alarm`)**: Only works on Unix and can be defeated by blocking C extensions. Not reliable.
- **Docker `--stop-timeout`**: Only applies to `docker stop`, not to running containers. Not suitable for enforcing execution time limits.

---

### 5. Error Propagation Strategy

**Decision**: Errors are categorized and returned as descriptive strings. The MCP tool wraps errors in a structured format.

**Rationale**:
- **Validation errors** (empty code, invalid JSON data): Caught in C# before Docker is invoked. Returned immediately.
- **Python runtime errors** (syntax errors, exceptions): Caught by `main.py`'s `try/except`, serialized as `{"error": "<message>"}`, and printed to stdout with exit code 1.
- **Docker errors** (image not found, OOM kill, timeout): Caught by `PythonSandboxService` via process exit code and stderr. Wrapped in `PythonSandboxException`.
- All errors flow through the `Result<T>` pattern used by existing tools, ensuring consistent error handling across the MCP server.

**Alternatives considered**:
- **Structured error codes**: Adding numeric error codes to Python errors. Unnecessary complexity for AI consumption — the AI reads natural language error messages.

---

### 6. `data` Parameter — Nullable or Required?

**Decision**: The `data` parameter is optional (nullable). When null or empty, the `data` variable in Python is set to `None`.

**Rationale**:
- Some analytical scripts may not need external data (e.g., generating a sequence, computing a constant).
- Making `data` optional keeps the tool flexible without breaking the common case.
- The AI assistant is responsible for providing data when needed.

**Alternatives considered**:
- **Required parameter**: Would force every call to include data, even when not needed. Unnecessarily restrictive.

## Summary of Decisions

| Topic | Decision |
|-------|----------|
| Docker invocation | `System.Diagnostics.Process` with `IDockerProcessRunner` abstraction |
| Python execution | `exec()` with restricted `__builtins__` + `json`, `pd`, `np`, `data` |
| Image build | Pre-built `mcp-python-sandbox` image (one-time setup) |
| Timeout enforcement | C# `CancellationTokenSource` (30s) + Docker `--memory` / `--cpus` flags |
| Error propagation | Validation → C#, Python errors → `main.py` try/except, Docker errors → `PythonSandboxException` |
| `data` parameter | Optional (nullable), defaults to `None` in Python |
