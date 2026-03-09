# Quickstart: Core Services & Infrastructure

**Feature**: `2-core-services`  
**Date**: 2026-03-09

## Prerequisites

- .NET 8 SDK installed
- Solution builds from Phase 1 (`dotnet build MCPDemo.sln` — zero errors)
- NuGet packages added for this phase (see below)

## NuGet Packages Required

```bash
# Infrastructure project
dotnet add src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj package Microsoft.Extensions.Http
dotnet add src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj package Serilog.AspNetCore
dotnet add src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj package Serilog.Sinks.File
dotnet add src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj package Serilog.Formatting.Compact

# Test projects (if not already present)
dotnet add tests/MCPDemo.Application.Tests/MCPDemo.Application.Tests.csproj package NSubstitute
dotnet add tests/MCPDemo.Application.Tests/MCPDemo.Application.Tests.csproj package FluentAssertions
dotnet add tests/MCPDemo.Infrastructure.Tests/MCPDemo.Infrastructure.Tests.csproj package NSubstitute
dotnet add tests/MCPDemo.Infrastructure.Tests/MCPDemo.Infrastructure.Tests.csproj package FluentAssertions
```

## Build Verification

```bash
dotnet build MCPDemo.sln
# Expected: Build succeeded, 0 warnings, 0 errors
```

## Key Files to Verify

After implementation, these files should exist and compile:

### Infrastructure Layer
- `src/MCPDemo.Infrastructure/ExternalApi/PlatziStoreApiClient.cs`
- `src/MCPDemo.Infrastructure/Logging/SerilogConfiguration.cs`
- `src/MCPDemo.Infrastructure/Metrics/IMetricsCollector.cs`
- `src/MCPDemo.Infrastructure/Metrics/InMemoryMetricsCollector.cs`
- `src/MCPDemo.Infrastructure/Metrics/ToolMetrics.cs`

### Application Layer
- `src/MCPDemo.Application/Interfaces/IPlatziStoreApiClient.cs`
- `src/MCPDemo.Application/Services/ProductService.cs`
- `src/MCPDemo.Application/Services/CategoryService.cs`
- `src/MCPDemo.Application/Services/SearchService.cs`

## Smoke Tests

1. **API Client compiles**: `dotnet build src/MCPDemo.Infrastructure/`
2. **Services compile**: `dotnet build src/MCPDemo.Application/`
3. **Full solution**: `dotnet build MCPDemo.sln` — zero errors, zero warnings
4. **Unit tests pass**: `dotnet test MCPDemo.sln` — all green
5. **Metrics thread safety**: Run concurrent metrics test — zero data loss

## Architecture Validation

Verify that no new project references violate Clean Architecture:
- Infrastructure MUST NOT reference Application
- Application MUST NOT reference Infrastructure
- Domain MUST NOT reference any other project
- Application references: Domain, Shared (unchanged from Phase 1)
- Infrastructure references: Domain, Shared (unchanged from Phase 1)
