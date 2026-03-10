# Quickstart: Unit & Integration Testing

**Feature**: `5-testing`  
**Date**: 2026-03-10

## Prerequisites

- .NET 8 SDK installed
- Solution builds cleanly: `dotnet build MCPDemo.sln` (zero errors)
- Test projects exist in `tests/` with NuGet packages installed
- Docker installed and running (for Docker integration tests only)
- Python sandbox image built: `docker build -t mcp-python-sandbox docker/python-sandbox/` (for Docker tests only)

## Running Tests

### All Tests (unit + integration + Docker)

```bash
dotnet test MCPDemo.sln
```

### Unit Tests Only (fast, no external dependencies)

```bash
dotnet test MCPDemo.sln --filter "Category!=Integration&Category!=Docker"
```

### Integration Tests Only (requires network)

```bash
dotnet test MCPDemo.sln --filter "Category=Integration"
```

### Docker Tests Only (requires Docker + sandbox image)

```bash
dotnet test MCPDemo.sln --filter "Category=Docker"
```

### With Code Coverage

```bash
dotnet test MCPDemo.sln --collect:"XPlat Code Coverage"
```

Coverage reports are generated in `tests/*/TestResults/*/coverage.cobertura.xml`.

## Test File Locations

| Test Project | Directory | Files |
|-------------|-----------|-------|
| `MCPDemo.Application.Tests` | `tests/MCPDemo.Application.Tests/Services/` | `ProductServiceTests.cs`, `CategoryServiceTests.cs`, `SearchServiceTests.cs` |
| `MCPDemo.Infrastructure.Tests` | `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/` | `PythonSandboxServiceTests.cs` |
| `MCPDemo.Infrastructure.Tests` | `tests/MCPDemo.Infrastructure.Tests/Metrics/` | `InMemoryMetricsCollectorTests.cs` |
| `MCPDemo.Infrastructure.Tests` | `tests/MCPDemo.Infrastructure.Tests/ExternalApi/` | `PlatziStoreApiClientTests.cs` |
| `MCPDemo.Integration.Tests` | `tests/MCPDemo.Integration.Tests/Services/` | `ProductIntegrationTests.cs` |
| `MCPDemo.Integration.Tests` | `tests/MCPDemo.Integration.Tests/PythonSandbox/` | `PythonIntegrationTests.cs` |

## Key Verification Points

| Check | How to Verify |
|-------|---------------|
| All unit tests pass | `dotnet test --filter "Category!=Integration&Category!=Docker"` returns 0 failures |
| Unit tests are fast | Total execution time < 10 seconds |
| Integration tests pass (with network) | `dotnet test --filter "Category=Integration"` returns 0 failures |
| Docker tests pass (with Docker) | `dotnet test --filter "Category=Docker"` returns 0 failures |
| Minimum test count | `dotnet test` reports ≥ 50 total test cases |
| Code coverage ≥ 80% | Run with `--collect:"XPlat Code Coverage"` and inspect report |
| Solution still builds | `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` succeeds |

## Test Naming Convention

All tests follow: `MethodName_StateUnderTest_ExpectedBehavior`

Examples:
- `GetByIdAsync_ValidId_ReturnsSuccessWithProduct`
- `CreateAsync_EmptyTitle_ReturnsFailure`
- `ExecuteAsync_NonZeroExitCode_ThrowsPythonSandboxException`
- `RecordExecution_ConcurrentCalls_NoDataLoss`

## Dependencies Between Test Projects

```
MCPDemo.Application.Tests → MCPDemo.Application, MCPDemo.Domain
MCPDemo.Infrastructure.Tests → MCPDemo.Infrastructure, MCPDemo.Domain, MCPDemo.Shared
MCPDemo.Integration.Tests → MCPDemo.Api, MCPDemo.Application, MCPDemo.Infrastructure, MCPDemo.Shared
```
