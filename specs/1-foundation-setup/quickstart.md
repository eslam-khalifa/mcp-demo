# Quickstart: Foundation & Project Setup

**Feature**: 1-foundation-setup  
**Date**: 2026-03-09

## Prerequisites

- .NET 8 SDK installed (`dotnet --version` should show `8.x.x`)
- A code editor (Visual Studio 2022 17.8+, VS Code with C# Dev Kit, or Rider 2023.3+)

## Building the Solution

1. Clone the repository and navigate to the project root.

2. Build the entire solution:
   ```bash
   dotnet build MCPDemo.sln
   ```

3. Verify the build completes with **zero errors and zero warnings**.
   All 8 projects (5 source + 3 test) should compile successfully.

## Verifying the Architecture

The project dependency graph should match this structure:

- **MCPDemo.Domain** — zero project references
- **MCPDemo.Shared** — zero project references
- **MCPDemo.Application** — references Domain only
- **MCPDemo.Infrastructure** — references Domain + Shared
- **MCPDemo.Api** — references Application + Infrastructure + Shared

Test projects:
- **MCPDemo.Application.Tests** — references Application + Domain
- **MCPDemo.Infrastructure.Tests** — references Infrastructure + Domain + Shared
- **MCPDemo.Integration.Tests** — references Api + Application + Infrastructure + Shared

## Key Files to Inspect

| File | Purpose |
|------|---------|
| `Directory.Build.props` | Global settings: .NET 8, nullable enabled, warnings-as-errors |
| `src/MCPDemo.Domain/Entities/Product.cs` | Product entity with all attributes |
| `src/MCPDemo.Domain/Entities/Category.cs` | Category entity with all attributes |
| `src/MCPDemo.Domain/ValueObjects/PriceRange.cs` | Price range value object with validation |
| `src/MCPDemo.Shared/Models/Result.cs` | Generic Result<T> wrapper (non-null success payloads) |
| `src/MCPDemo.Shared/Models/ErrorResponse.cs` | Standardized error model |
| `src/MCPDemo.Application/Interfaces/*.cs` | Service contracts for all features |

## Smoke Test

After building, verify the foundation is solid:

1. **Build succeeds** with zero errors and zero warnings.
2. **Domain entities** are defined with all expected attributes.
3. **PriceRange** validation correctly rejects invalid ranges.
4. **Result<T>** enforces non-null success payloads.
5. **Service interfaces** compile and can be mock-implemented.

## What's Next

This foundation phase sets up the scaffolding. The following phases add
functionality:

- **Phase 2**: Platzi API client + Product/Category services (implementation)
- **Phase 3**: MCP server bootstrap + tool definitions
- **Phase 4**: Python sandbox + analytics tools
- **Phase 5**: Testing
- **Phase 6**: Metrics + polish
