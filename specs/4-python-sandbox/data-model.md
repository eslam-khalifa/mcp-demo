# Data Model: Python Sandbox & Custom Code Execution

**Feature**: `4-python-sandbox`  
**Date**: 2026-03-10

## Entities

### SandboxExecutionRequest

Represents the input to the `run_python_code` MCP tool and the `IPythonSandboxService`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Code` | string | Yes | The Python source code to execute. Must not be empty. |
| `Data` | string? | No | A JSON string containing the data payload. Defaults to `null`, which sets the Python `data` variable to `None`. |

**Validation Rules**:
- `Code` must not be null, empty, or whitespace.
- `Data`, if provided, must be valid JSON (validated before Docker invocation to fail fast).

---

### SandboxExecutionResult

Represents the output from the Python sandbox execution.

| Field | Type | Description |
|-------|------|-------------|
| `Output` | string | The captured standard output from the Python script. May be empty if the code produces no `print()` calls. |
| `IsSuccess` | bool | `true` if the code executed without errors; `false` otherwise. |
| `Error` | string? | Error message if execution failed (validation, runtime, timeout, or Docker error). `null` on success. |
| `ExecutionTimeMs` | long | Wall-clock execution time in milliseconds. |

---

## Interfaces

### IPythonSandboxService

Abstraction for the sandbox execution infrastructure. Defined in `MCPDemo.Application.Interfaces`.

| Method | Signature | Description |
|--------|-----------|-------------|
| `ExecuteAsync` | `Task<string> ExecuteAsync(string code, string jsonData)` | Executes Python code in a Docker sandbox with the provided JSON data. Returns stdout. Throws `PythonSandboxException` on failure. |

---

### IDockerProcessRunner

Abstraction for the Docker CLI process. Defined in `MCPDemo.Infrastructure.PythonSandbox` for testability.

| Method | Signature | Description |
|--------|-----------|-------------|
| `RunAsync` | `Task<ProcessResult> RunAsync(string arguments, string stdinInput, CancellationToken cancellationToken)` | Starts a Docker process with the given arguments, pipes stdin, and returns the result. |

### ProcessResult

| Field | Type | Description |
|-------|------|-------------|
| `ExitCode` | int | Process exit code (0 = success). |
| `StandardOutput` | string | Captured stdout. |
| `StandardError` | string | Captured stderr. |

---

## DI Registration

| Interface | Implementation | Lifetime | Project |
|-----------|---------------|----------|---------|
| `IPythonSandboxService` | `PythonSandboxService` | Scoped | MCPDemo.Infrastructure |
| `IDockerProcessRunner` | `DockerProcessRunner` | Scoped | MCPDemo.Infrastructure |

Both are registered in `Program.cs` alongside existing services.

---

## Docker Runtime Model

### Container Configuration

| Setting | Value | Enforced By |
|---------|-------|-------------|
| Image | `mcp-python-sandbox` | Docker CLI argument |
| Memory | 256 MB | `--memory=256m` |
| CPU | 0.5 cores | `--cpus=0.5` |
| Network | None | `--network=none` |
| Filesystem | Read-only | `--read-only` |
| Privileges | No escalation | `--security-opt=no-new-privileges` |
| Cleanup | Auto-remove | `--rm` |
| Stdin | Interactive | `-i` |
| Timeout | 30 seconds | C# `CancellationTokenSource` |

### Stdin Payload (JSON)

```json
{
  "code": "products = sorted(data, key=lambda p: p['price'], reverse=True)[:5]\nprint(json.dumps(products))",
  "data": "[{\"id\":1,\"title\":\"Laptop\",\"price\":999},{\"id\":2,\"title\":\"Phone\",\"price\":599}]"
}
```

### Python Execution Namespace

| Variable | Type | Source |
|----------|------|--------|
| `data` | any (parsed JSON) | Pre-parsed from `data` field in stdin payload |
| `json` | module | Python stdlib |
| `pd` | module | pandas |
| `np` | module | numpy |
| `print` | builtin | Whitelisted |
| `len`, `range`, `enumerate`, `zip`, `map`, `filter`, `sorted` | builtins | Whitelisted |
| `min`, `max`, `sum`, `abs`, `round` | builtins | Whitelisted |
| `int`, `float`, `str`, `bool`, `list`, `dict`, `tuple`, `set` | type constructors | Whitelisted |
| `isinstance`, `type` | builtins | Whitelisted |
| `True`, `False`, `None` | constants | Whitelisted |

**Blocked builtins**: `open`, `exec`, `eval`, `__import__`, `compile`, `globals`, `locals`, `getattr`, `setattr`, `delattr`, `dir`, `vars`, `breakpoint`, `exit`, `quit`, `input`.
