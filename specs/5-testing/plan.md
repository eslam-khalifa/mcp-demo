# Implementation Plan: Unit & Integration Testing

**Branch**: `main` | **Date**: 2026-03-10 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/5-testing/spec.md`

## Summary

Implement comprehensive test coverage for the MCP Demo project across three test projects: `MCPDemo.Application.Tests` (unit tests for Application services), `MCPDemo.Infrastructure.Tests` (unit tests for Infrastructure components), and `MCPDemo.Integration.Tests` (end-to-end tests against real Platzi API and Docker sandbox). All test projects already exist with the correct NuGet packages but contain no test source files. The goal is ≥50 tests with ≥80% code coverage for Application and Infrastructure layers.

## Technical Context

**Language/Version**: C# 12, .NET 8  
**Primary Dependencies**: xUnit 2.9.2, NSubstitute 5.3.0, FluentAssertions 8.8.0, coverlet.collector 6.0.2  
**Storage**: N/A (no database; tests mock external API)  
**Testing**: xUnit with `dotnet test`  
**Target Platform**: Windows (developer machine)  
**Project Type**: Test suite for an MCP server  
**Performance Goals**: All unit tests complete in <10 seconds  
**Constraints**: Integration tests require network; Docker tests require Docker + built sandbox image  
**Scale/Scope**: ~50+ test cases across 3 test projects  

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status |
|-----------|------|--------|
| I. MCP Tool Standards | Tests verify tool behavior contracts | ✅ Pass — acceptance scenarios cover all tool operations |
| II. API Calling & Efficiency | Tests verify retry logic and error handling | ✅ Pass — FR-006 covers API client HTTP tests |
| III. Logging & Metrics | Tests verify metric recording | ✅ Pass — FR-010 mandates metrics verification in every service test |
| IV. Clean Architecture | Tests follow layer separation | ✅ Pass — separate test projects per layer |
| V. Security & Sandboxing | Tests verify sandbox constraints | ✅ Pass — FR-004 covers Docker argument verification |
| Governance: Phased review | Implementation phases require user approval | ✅ Pass — will follow phased approach |

## Project Structure

### Documentation (this feature)

```text
specs/5-testing/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (test fixtures & mocking patterns)
├── quickstart.md        # Phase 1 output
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
tests/
├── MCPDemo.Application.Tests/          # Unit tests for Application services
│   ├── Services/
│   │   ├── ProductServiceTests.cs      # ProductService unit tests (~15 tests)
│   │   ├── CategoryServiceTests.cs     # CategoryService unit tests (~12 tests)
│   │   └── SearchServiceTests.cs       # SearchService unit tests (~4 tests)
│   └── MCPDemo.Application.Tests.csproj
│
├── MCPDemo.Infrastructure.Tests/       # Unit tests for Infrastructure
│   ├── PythonSandbox/
│   │   └── PythonSandboxServiceTests.cs  # Sandbox service tests (~6 tests)
│   ├── Metrics/
│   │   └── InMemoryMetricsCollectorTests.cs  # Metrics collector tests (~5 tests)
│   ├── ExternalApi/
│   │   └── PlatziStoreApiClientTests.cs  # API client tests (~5 tests)
│   └── MCPDemo.Infrastructure.Tests.csproj
│
└── MCPDemo.Integration.Tests/          # Integration tests (real API + Docker)
    ├── Services/
    │   └── ServiceIntegrationTests.cs  # Platzi API integration tests (~3 tests)
    ├── PythonSandbox/
    │   └── SandboxIntegrationTests.cs  # Docker sandbox integration tests (~3 tests)
    └── MCPDemo.Integration.Tests.csproj
```

**Structure Decision**: Tests are organized by layer (matching the `src/` structure) and further by component within each test project. This mirrors the production code organization and makes it easy to find the tests for any given component.

## Complexity Tracking

No constitution violations. No complexity justifications needed.
