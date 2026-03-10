# Implementation Plan: Python Sandbox & Custom Code Execution

**Branch**: `4-python-sandbox` | **Date**: 2026-03-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/4-python-sandbox/spec.md`

## Summary

Implement a general-purpose `run_python_code` MCP tool that accepts AI-generated Python code and a JSON data payload, executes the code inside a secure Docker sandbox (`python:3.11-slim`), and returns the script's stdout output. The sandbox is hardened with no network access, a read-only filesystem, 256 MB memory cap, 0.5 CPU cap, and a 30-second timeout. This enables the AI assistant to answer any ad-hoc analytical question about store data by writing and executing custom Python scripts at runtime.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (MCP tool + infrastructure service), Python 3.11 (sandbox runtime)  
**Primary Dependencies**: `ModelContextProtocol` NuGet (MCP SDK), `System.Diagnostics.Process` (Docker CLI invocation), `pandas` 2.2.0, `numpy` 1.26.3 (Python sandbox)  
**Storage**: N/A — stateless execution, no persistence  
**Testing**: xUnit, NSubstitute, FluentAssertions (C# side); manual Docker integration tests  
**Target Platform**: Windows/Linux host with Docker installed  
**Project Type**: MCP server tool (extension to existing `MCPDemo.Api` project)  
**Performance Goals**: Typical analytics (sort/filter/aggregate on ≤1,000 products) complete within 30 seconds  
**Constraints**: 256 MB memory, 0.5 CPU, 30s timeout, no network, read-only filesystem, restricted Python builtins  
**Scale/Scope**: Single tool addition to existing 16-tool MCP server; 1 Dockerfile, 1 Python entry point, 1 C# interface, 1 C# service, 1 MCP tool class

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. MCP Tool Standards** | ✅ PASS | `run_python_code` has name, description, typed inputs (`code`, `data`), typed output (string), category (Analytics), implementation type (Python via C# wrapper) |
| **II. API Calling & Efficiency** | ✅ PASS | The sandbox tool does not call the Platzi API directly. Data is fetched by existing C# tools before being passed to `run_python_code`. |
| **III. Logging & Metrics** | ✅ PASS | Each `run_python_code` invocation will be logged (timestamp, tool name, execution time, success/failure) and counted in the metrics collector (including Python sandbox execution counter). |
| **IV. Clean Architecture** | ✅ PASS | `IPythonSandboxService` interface in Application layer; `PythonSandboxService` implementation in Infrastructure layer; `PythonTools.cs` MCP tool in Api layer. Dependencies flow inward. |
| **V. Security & Sandboxing** | ✅ PASS | Docker with `--network=none`, `--read-only`, `--memory=256m`, `--cpus=0.5`, `--security-opt=no-new-privileges`. Python builtins restricted. 30s timeout enforced by C# `CancellationTokenSource`. |
| **Technology Stack** | ✅ PASS | .NET 8, C#, Python 3.11 in `python:3.11-slim`, structured file logging via Serilog. |
| **Spec-Kit Conventions** | ✅ PASS | Tool spec follows spec-kit format. |

**Gate Result**: ✅ All gates pass. Proceeding to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/4-python-sandbox/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── run-python-code-tool.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── MCPDemo.Api/
│   ├── Program.cs                          # Add IPythonSandboxService DI registration
│   └── McpTools/
│       └── PythonTools.cs                  # NEW — run_python_code MCP tool
│
├── MCPDemo.Application/
│   └── Interfaces/
│       └── IPythonSandboxService.cs        # NEW — sandbox execution interface
│
└── MCPDemo.Infrastructure/
    └── PythonSandbox/
        ├── PythonSandboxService.cs         # NEW — Docker CLI invocation
        └── DockerProcessRunner.cs          # NEW — Process abstraction for testability

docker/
└── python-sandbox/
    ├── Dockerfile                          # NEW — python:3.11-slim + pandas + numpy
    ├── requirements.txt                    # NEW — pandas, numpy
    └── main.py                             # NEW — general-purpose code executor

tests/
├── MCPDemo.Infrastructure.Tests/
│   └── PythonSandbox/
│       └── PythonSandboxServiceTests.cs    # NEW — unit tests with mocked Process
└── MCPDemo.Integration.Tests/
    └── PythonTools/
        └── RunPythonCodeTests.cs           # NEW — end-to-end Docker integration tests
```

**Structure Decision**: Extends the existing Clean Architecture layout. New files are added to existing projects — no new .csproj files needed. The `docker/python-sandbox/` directory is new at the repo root.

## Complexity Tracking

No constitution violations. No complexity justifications needed.
