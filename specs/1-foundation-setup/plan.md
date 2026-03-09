# Implementation Plan: Foundation & Project Setup

**Branch**: `1-foundation-setup` | **Date**: 2026-03-09 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/1-foundation-setup/spec.md`

## Summary

Establish the complete .NET solution structure following Clean Architecture
with 5 source projects (Api, Application, Domain, Infrastructure, Shared)
and 3 test projects. Define domain entities (Product, Category), a PriceRange
value object, shared utilities (Result<T>, ErrorResponse, exceptions, API
constants), and service interfaces for all downstream features. All projects
enforce warnings-as-errors and nullable reference type analysis.

## Technical Context

**Language/Version**: C# 12, .NET 8.0  
**Primary Dependencies**: None beyond default SDK (no NuGet packages in this phase)  
**Storage**: N/A (no database in this phase)  
**Testing**: xUnit (scaffolded, no test cases in this phase)  
**Target Platform**: Cross-platform (.NET 8 — Windows, Linux, macOS)  
**Project Type**: MCP Server (stdio) — foundation scaffolding in this phase  
**Performance Goals**: Solution build under 2 minutes with warm NuGet cache  
**Constraints**: Warnings-as-errors, nullable reference types enabled, zero circular dependencies  
**Scale/Scope**: 8 projects (5 source + 3 test), ~15 source files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status |
|-----------|------|--------|
| I. MCP Tool Standards | Tool definitions not implemented in this phase (scaffolding only). Interfaces declare the contract shape. | ✅ Pass (deferred) |
| II. API Calling & Efficiency | External API client not implemented in this phase. Base URL stored as constant. | ✅ Pass (deferred) |
| III. Logging & Metrics | Not implemented in this phase. Infrastructure project scaffolded for future Serilog integration. | ✅ Pass (deferred) |
| IV. Clean Architecture | Strict 4-layer + Shared dependency graph enforced. Domain has zero refs. Dependencies flow inward. MCP tools will reside in Application layer. | ✅ Pass |
| V. Security & Sandboxing | Python sandbox not implemented in this phase. Infrastructure project scaffolded for future Docker integration. No secrets stored in code. | ✅ Pass (deferred) |

**Result: All gates pass.** Principles I, II, III, V are deferred to
later implementation phases. Principle IV (Clean Architecture) is the
primary concern of this feature and is fully addressed.

## Project Structure

### Documentation (this feature)

```text
specs/1-foundation-setup/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (service interfaces)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── MCPDemo.Api/                        # API / MCP Host Layer
│   ├── MCPDemo.Api.csproj
│   └── Program.cs                      # Minimal placeholder
│
├── MCPDemo.Application/                # Application Layer
│   ├── MCPDemo.Application.csproj
│   ├── Interfaces/
│   │   ├── IProductService.cs
│   │   ├── ICategoryService.cs
│   │   ├── ISearchService.cs
│   │   ├── IAnalyticsService.cs
│   │   └── IPythonSandboxService.cs
│   └── DTOs/
│       ├── Products/
│       └── Categories/
│
├── MCPDemo.Domain/                     # Domain Layer (zero references)
│   ├── MCPDemo.Domain.csproj
│   ├── Entities/
│   │   ├── Product.cs
│   │   └── Category.cs
│   ├── ValueObjects/
│   │   └── PriceRange.cs
│   └── Exceptions/
│       ├── DomainException.cs
│       └── EntityNotFoundException.cs
│
├── MCPDemo.Infrastructure/             # Infrastructure Layer
│   ├── MCPDemo.Infrastructure.csproj
│   └── (empty — scaffolded for future features)
│
└── MCPDemo.Shared/                     # Shared / Cross-Cutting (zero references)
    ├── MCPDemo.Shared.csproj
    ├── Constants/
    │   └── ApiConstants.cs
    ├── Exceptions/
    │   ├── McpToolException.cs
    │   └── ExternalApiException.cs
    ├── Extensions/
    │   └── StringExtensions.cs
    └── Models/
        ├── Result.cs
        └── ErrorResponse.cs

tests/
├── MCPDemo.Application.Tests/
│   └── MCPDemo.Application.Tests.csproj
├── MCPDemo.Infrastructure.Tests/
│   └── MCPDemo.Infrastructure.Tests.csproj
└── MCPDemo.Integration.Tests/
    └── MCPDemo.Integration.Tests.csproj

MCPDemo.sln
```

**Structure Decision**: Clean Architecture with 5 source projects + 1 Shared
cross-cutting project. The Shared project is justified by the constitution
(cross-cutting concerns like Result<T>, ErrorResponse, and custom exceptions
are needed by multiple layers). Domain has zero references. Dependencies
flow inward: Api → Application → Domain ← Infrastructure. All layers may
reference Shared.

### Project Reference Graph

```text
MCPDemo.Api ─────────────► MCPDemo.Application ──────► MCPDemo.Domain
     │                            │                          ▲
     │                            │                          │
     ├──► MCPDemo.Infrastructure ─┼──────────────────────────┘
     │           │                │
     │           └────────────────┼──► MCPDemo.Shared
     └────────────────────────────┘          ▲
                                             │ (all layers may reference)

MCPDemo.Application.Tests ──► MCPDemo.Application + MCPDemo.Domain
MCPDemo.Infrastructure.Tests ──► MCPDemo.Infrastructure + MCPDemo.Domain + MCPDemo.Shared
MCPDemo.Integration.Tests ──► MCPDemo.Api + MCPDemo.Application + MCPDemo.Infrastructure + MCPDemo.Shared
```

## Complexity Tracking

| Aspect | Decision | Justification |
|--------|----------|---------------|
| 5th project (Shared) | Kept | Constitution requires cross-cutting models (Result, ErrorResponse) used by multiple layers. Without Shared, these would be duplicated or create circular dependencies. |
| Warnings-as-errors | Enabled globally | Clarification Q1: Catches issues at compile time. Standard for greenfield .NET 8 projects. |
| Nullable reference types | Enabled globally | Clarification Q1: Prevents null reference bugs. Aligns with Result type's non-null payload guarantee. |
