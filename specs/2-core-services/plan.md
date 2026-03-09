# Implementation Plan: Core Services & Infrastructure

**Branch**: `2-core-services` | **Date**: 2026-03-09 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/2-core-services/spec.md`

## Summary

Implement the core service layer that bridges the Application layer to the
external Platzi Fake Store API. This phase delivers 4 key components:

1. **PlatziStoreApiClient** — HTTP client with retry logic, 15s timeout, and
   response mapping (Infrastructure layer).
2. **ProductService, CategoryService, SearchService** — Application-layer
   orchestrators that validate inputs, delegate to the API client, and wrap
   responses in `Result<T>` (Application layer).
3. **Serilog configuration** — Structured JSON file logging with daily rolling
   (Infrastructure layer).
4. **InMemoryMetricsCollector** — Thread-safe runtime metrics collection
   (Infrastructure layer).

## Technical Context

**Language/Version**: C# 12, .NET 8.0  
**Primary Dependencies**: `Microsoft.Extensions.Http`, `Serilog.AspNetCore`,
`Serilog.Sinks.File`, `Serilog.Formatting.Compact`, `System.Text.Json`  
**Storage**: N/A (all data from external API, no local persistence)  
**Testing**: xUnit, NSubstitute, FluentAssertions  
**Target Platform**: Cross-platform console app (stdio MCP server)  
**Project Type**: MCP server (stdio transport)  
**Performance Goals**: 15s per-request timeout, max ~47s worst-case with retries  
**Constraints**: No caching, no EF Core, warnings-as-errors, nullable enabled  
**Scale/Scope**: Single-user MCP server connected to Cursor IDE

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Notes |
|-----------|------|--------|-------|
| I. MCP Tool Standards | Tool definition contract | ⏳ Deferred | Tools defined in Phase 3; services here implement the backing logic |
| II. API Calling & Efficiency | Retry mechanism, error handling | ✅ Pass | FR-003: 2 retries with backoff; FR-004: no retry on 4xx; FR-023: 15s timeout; FR-005: domain entity mapping |
| II. API Calling & Efficiency | Caching | ⚠️ Deviation | Constitution mentions cache; project decision is "no caching" per implementation plan. Justified: Platzi API is demo/ephemeral data, caching adds complexity without value |
| III. Logging & Metrics | File logging | ✅ Pass | FR-017: JSON rolling daily; FR-018: all required fields; FR-019: no sensitive data |
| III. Logging & Metrics | Runtime metrics | ✅ Pass | FR-020: per-tool stats; FR-021: thread-safe; FR-022: single + all retrieval |
| IV. Clean Architecture | Layer separation | ✅ Pass | API client in Infrastructure; services in Application; domain entities in Domain |
| IV. Clean Architecture | Dependency direction | ✅ Pass | Infrastructure → Domain + Shared; Application → Domain + Shared; no reverse deps |
| V. Security & Sandboxing | Input validation | ✅ Pass | FR-009, FR-012: validation before API calls; empty DTO rejection |
| V. Security & Sandboxing | No secrets in logs | ✅ Pass | FR-019: explicit prohibition |

**Gate result**: ✅ PASS (1 justified deviation on caching)

## Project Structure

### Documentation (this feature)

```text
specs/2-core-services/
├── plan.md              # This file
├── research.md          # Phase 0: technology decisions
├── data-model.md        # Phase 1: entity and model definitions
├── quickstart.md        # Phase 1: build and verify instructions
├── contracts/
│   └── api-client-interface.md  # Phase 1: IPlatziStoreApiClient contract
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (new/modified files)

```text
src/
├── MCPDemo.Infrastructure/
│   ├── ExternalApi/
│   │   ├── IPlatziStoreApiClient.cs    # API client interface
│   │   ├── PlatziStoreApiClient.cs     # API client implementation
│   │   └── ApiModels/                  # Raw API response models (if needed)
│   ├── Logging/
│   │   └── SerilogConfiguration.cs     # Serilog setup helper
│   └── Metrics/
│       ├── IMetricsCollector.cs         # Metrics interface
│       ├── InMemoryMetricsCollector.cs  # Thread-safe implementation
│       └── ToolMetrics.cs              # Metrics data model
│
├── MCPDemo.Application/
│   └── Services/
│       ├── ProductService.cs           # IProductService implementation
│       ├── CategoryService.cs          # ICategoryService implementation
│       └── SearchService.cs            # ISearchService implementation
│
└── MCPDemo.Shared/                     # (no changes expected)

tests/
├── MCPDemo.Application.Tests/          # Service unit tests
└── MCPDemo.Infrastructure.Tests/       # API client + metrics unit tests
```

### Project Reference Graph (unchanged from Phase 1)

```
Api → Application → Domain
 │        │
 │        └→ Shared
 └→ Infrastructure → Domain
          │
          └→ Shared
```

**Structure Decision**: Follows the Clean Architecture established in Phase 1.
New files are added within existing projects — no new `.csproj` files needed.

## Complexity Tracking

| Deviation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| No caching (Constitution II) | Platzi API serves ephemeral demo data; caching stale data adds complexity without value | Direct API calls are simpler and always fresh |
| Application refs Shared (not just Domain) | Services need `Result<T>` from Shared | Moving Result<T> to Domain would pollute the domain with infrastructure concerns |
