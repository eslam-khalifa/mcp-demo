# Quickstart: Python Sandbox & `run_python_code`

**Feature**: `4-python-sandbox`  
**Date**: 2026-03-10

## Prerequisites

- .NET 8 SDK installed
- Docker installed and running
- Phase 3 complete (MCP server operational with 16 tools)
- Solution builds with zero errors

## Setup Steps

### 1. Build the Python Sandbox Docker Image

```bash
cd docker/python-sandbox
docker build -t mcp-python-sandbox .
```

Verify:
```bash
docker images | grep mcp-python-sandbox
```

### 2. Test the Docker Image Manually

```bash
echo '{"code": "print(42)", "data": "null"}' | docker run --rm -i --memory=256m --cpus=0.5 --network=none --read-only --security-opt=no-new-privileges mcp-python-sandbox
```

Expected output: `42`

### 3. Create the Source Files

Create the following new files:

| File | Purpose |
|------|---------|
| `docker/python-sandbox/Dockerfile` | Python 3.11 sandbox image |
| `docker/python-sandbox/requirements.txt` | pandas, numpy |
| `docker/python-sandbox/main.py` | General-purpose code executor |
| `src/MCPDemo.Application/Interfaces/IPythonSandboxService.cs` | Sandbox interface |
| `src/MCPDemo.Infrastructure/PythonSandbox/DockerProcessRunner.cs` | Process abstraction |
| `src/MCPDemo.Infrastructure/PythonSandbox/PythonSandboxService.cs` | Docker CLI invocation |
| `src/MCPDemo.Api/McpTools/PythonTools.cs` | `run_python_code` MCP tool |

### 4. Register Services in Program.cs

Add the following DI registrations to `src/MCPDemo.Api/Program.cs`:

```csharp
builder.Services.AddScoped<IPythonSandboxService, PythonSandboxService>();
builder.Services.AddScoped<IDockerProcessRunner, DockerProcessRunner>();
```

### 5. Build and Verify

```bash
dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true
```

Expected: **Build succeeded, 0 errors, 0 warnings**

### 6. Run the MCP Server

```bash
dotnet run --project src/MCPDemo.Api
```

## Key Verification Points

| Check | How to Verify |
|-------|---------------|
| Docker image exists | `docker images \| grep mcp-python-sandbox` returns a row |
| Image works standalone | Manual `echo ... \| docker run` test returns `42` |
| Tool is registered | MCP tool listing returns 17 tools (16 existing + 1 new) |
| Simple code runs | `run_python_code(code="print(42)", data=null)` returns `"42"` |
| Data access works | `run_python_code(code="print(len(data))", data="[1,2,3]")` returns `"3"` |
| Pandas works | `run_python_code(code="import json; print(json.dumps(pd.DataFrame(data).describe().to_dict()))", data="[{\"x\":1},{\"x\":2}]")` returns stats |
| Error handling works | `run_python_code(code="1/0")` returns error with `ZeroDivisionError` |
| Empty code rejected | `run_python_code(code="")` returns validation error |
| Timeout enforced | Long-running code is killed after 30 seconds |
| Build clean | `dotnet build` returns 0 errors, 0 warnings |

## Docker Image Contents

```
python:3.11-slim
├── pandas 2.2.0
├── numpy 1.26.3
└── main.py (general-purpose code executor)
```

## Typical AI Workflow

```
1. AI calls search_products(title="laptop") → gets product JSON
2. AI writes Python code to analyze the data
3. AI calls run_python_code(code=<script>, data=<product JSON>)
4. main.py parses payload, exec(code) with data variable
5. Python script prints result → returned to AI
```
