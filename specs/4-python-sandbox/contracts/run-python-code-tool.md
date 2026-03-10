# MCP Tool Contract: `run_python_code`

**Feature**: `4-python-sandbox`  
**Date**: 2026-03-10  
**Version**: 1.0.0  
**Category**: Data Analysis  
**Implementation Type**: Python (via C# Docker invocation)

## Tool Definition

| Property | Value |
|----------|-------|
| **Name** | `run_python_code` |
| **Description** | Execute arbitrary Python code in a secure Docker sandbox to perform custom data analysis on store data. The code receives data via a pre-parsed `data` variable and outputs results via `print()`. Libraries available: `json`, `pandas` (as `pd`), `numpy` (as `np`). |
| **Category** | Data Analysis |
| **Implementation** | C# MCP tool → `IPythonSandboxService` → Docker CLI → `main.py` |

## Inputs

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `code` | `string` | Yes | The Python source code to execute. Must not be empty. The code can reference the `data` variable (pre-parsed JSON), `json`, `pd` (pandas), and `np` (numpy). Use `print()` to output results. |
| `data` | `string?` | No | A JSON string containing the data payload for the script. If omitted or null, the `data` variable in Python is set to `None`. |

## Outputs

| Scenario | Return Type | Description |
|----------|-------------|-------------|
| **Success** | `string` | The captured standard output (`print()` calls) from the Python code. |
| **Validation Error** | `string` | Error message if `code` is empty. Docker is not invoked. |
| **Python Error** | `string` | Error message from the Python runtime (syntax error, exception, etc.). Includes error type and message. |
| **Timeout Error** | `string` | Error message indicating execution exceeded the 30-second timeout. |
| **Docker Error** | `string` | Error message if Docker/sandbox is unavailable or the process failed. |

## MCP Tool Attributes

```
[McpServerTool]
[Description("Execute Python code in a secure sandbox for custom data analysis. The code can use json, pandas (pd), numpy (np), and a pre-parsed 'data' variable.")]
```

### Parameter Attributes

```
[Description("The Python source code to execute. Use print() to output results.")]
string code

[Description("JSON string data payload for the script. Available as the 'data' variable in Python. Optional.")]
string? data = null
```

## Example Invocations

### Example 1: Top 5 Most Expensive Products

**Input**:
- `code`: 
  ```python
  products = sorted(data, key=lambda p: p["price"], reverse=True)[:5]
  avg = sum(p["price"] for p in products) / len(products)
  print(json.dumps({"top_5": [p["title"] for p in products], "average_price": avg}))
  ```
- `data`: `[{"title":"Laptop","price":999},{"title":"Phone","price":599},...]`

**Output**: `{"top_5": ["Laptop", ...], "average_price": 750.0}`

### Example 2: Products Under $50

**Input**:
- `code`: `print(len([p for p in data if p["price"] < 50]))`
- `data`: `[{"title":"T-Shirt","price":25},{"title":"Laptop","price":999},...]`

**Output**: `1`

### Example 3: Price Statistics with Pandas

**Input**:
- `code`:
  ```python
  import json
  df = pd.DataFrame(data)
  stats = df["price"].describe().to_dict()
  print(json.dumps(stats))
  ```
- `data`: `[{"title":"A","price":10},{"title":"B","price":20},{"title":"C","price":30}]`

**Output**: `{"count": 3.0, "mean": 20.0, "std": 10.0, "min": 10.0, "25%": 15.0, "50%": 20.0, "75%": 25.0, "max": 30.0}`

## Security Constraints

| Constraint | Enforcement | Level |
|------------|-------------|-------|
| No network access | Docker `--network=none` | Container |
| Read-only filesystem | Docker `--read-only` | Container |
| Memory cap (256 MB) | Docker `--memory=256m` | Container |
| CPU cap (0.5 cores) | Docker `--cpus=0.5` | Container |
| No privilege escalation | Docker `--security-opt=no-new-privileges` | Container |
| Execution timeout (30s) | C# `CancellationTokenSource` | Host |
| Restricted builtins | Python `exec()` with custom `__builtins__` | Application |
| Auto-cleanup | Docker `--rm` | Container |
