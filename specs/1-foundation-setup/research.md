# Research: Foundation & Project Setup

**Feature**: 1-foundation-setup  
**Date**: 2026-03-09  
**Status**: Complete (no NEEDS CLARIFICATION items)

## Overview

This research document consolidates decisions for the foundation phase.
Since the technology stack was already defined in the project-level
implementation plan and all ambiguities were resolved in the clarification
session, there are no unknowns to research. This document records the
rationale behind key decisions for traceability.

---

## R1: Project Template Strategy

**Decision**: Use `dotnet new classlib` for library projects and
`dotnet new console` for the Api project (MCP server host). Do not use
`dotnet new webapi` since the MCP server uses stdio transport, not HTTP.

**Rationale**: The MCP server is a console application that communicates
via stdin/stdout (JSON-RPC over stdio). ASP.NET Core's HTTP pipeline is
unnecessary overhead. A console project with `Microsoft.Extensions.Hosting`
(added in a later phase) is the correct host type.

**Alternatives considered**:
- `dotnet new webapi` — rejected: adds HTTP pipeline, Kestrel, middleware
  that are not needed for stdio transport.
- `dotnet new worker` — considered but deferred: worker service template
  adds `BackgroundService` boilerplate. Could be used later if needed.

---

## R2: Nullable Reference Types Strategy

**Decision**: Enable `<Nullable>enable</Nullable>` in all project files
and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.

**Rationale**: Clarification Q1 confirmed this choice. For a greenfield
.NET 8 project, nullable analysis catches null reference bugs at compile
time. Combined with warnings-as-errors, this ensures all nullable
annotations are intentional and validated.

**Implementation note**: Add both settings to a `Directory.Build.props`
file at the solution root to enforce them globally without repeating in
each `.csproj` file.

**Alternatives considered**:
- Per-project settings — rejected: risk of inconsistency across projects.
- Nullable annotations only (no warnings-as-errors) — rejected: warnings
  would be advisory only, reducing effectiveness.

---

## R3: Result<T> Design Pattern

**Decision**: Implement a generic `Result<T>` type that enforces non-null
success payloads (clarification Q2). Use a static factory pattern:
`Result<T>.Success(T value)` and `Result<T>.Failure(string error)`.

**Rationale**: A Result wrapper eliminates exceptions for expected failure
paths (e.g., "product not found" is not exceptional). The non-null
guarantee aligns with nullable reference types and prevents ambiguous
states where a Result is "successful" but carries no data.

**Design constraints**:
- `Success(T value)` throws `ArgumentNullException` if value is null.
- `Failure(string error)` creates a failed Result with an error message.
- `IsSuccess` and `IsFailure` properties for status checking.
- `Value` property throws `InvalidOperationException` if accessed on a
  failed Result.

**Alternatives considered**:
- Throwing exceptions for all failures — rejected: exceptions are expensive
  for expected control flow and make service composition harder.
- `OneOf<T, Error>` discriminated union — considered: more functional style
  but adds a NuGet dependency (`OneOf`), which this phase avoids.
- Allowing null success values — rejected per clarification Q2.

---

## R4: Domain Exception Hierarchy

**Decision**: Two base exception types in the Domain project:
- `DomainException` — base for all domain-level errors.
- `EntityNotFoundException` — specific case for "entity not found" scenarios.

Shared project adds two cross-cutting exceptions:
- `McpToolException` — errors during MCP tool execution.
- `ExternalApiException` — errors from the Platzi Fake Store API.

**Rationale**: Domain exceptions reside in the Domain layer (no external
dependencies). Cross-cutting exceptions reside in Shared because they
involve infrastructure concepts (MCP tools, external APIs) that span
multiple layers.

**Alternatives considered**:
- All exceptions in Domain — rejected: McpToolException and
  ExternalApiException reference concepts outside the domain boundary.
- All exceptions in Shared — rejected: DomainException and
  EntityNotFoundException are pure domain concepts.

---

## R5: Service Interface Design

**Decision**: Define 5 service interfaces in the Application layer:
- `IProductService` — 8 methods (CRUD + related products)
- `ICategoryService` — 7 methods (CRUD + products by category)
- `ISearchService` — 1 method (combinable filter search)
- `IAnalyticsService` — 4 methods (Python-backed analytics)
- `IPythonSandboxService` — 1 method (execute Python tool in Docker)

**Rationale**: Each interface maps to a distinct MCP tool category from
the constitution. `IPythonSandboxService` is separated because it's an
infrastructure concern accessed through an application-layer abstraction
(constitution Principle IV).

**Design constraints**:
- All methods return `Task<Result<T>>` for async operations with
  structured error handling.
- Input parameters use primitive types or DTOs (no domain entities as
  direct inputs to avoid coupling).
- `IPythonSandboxService` accepts JSON string input and returns JSON
  string output (constitution Principle I: inputs/outputs serialized
  to JSON).

**Alternatives considered**:
- Single `IMcpToolService` with a dispatch method — rejected: violates
  Interface Segregation Principle and makes mocking harder.
- Separate interface per MCP tool — rejected: too granular (21 interfaces
  for 21 tools is excessive).

---

## R6: Directory.Build.props for Global Settings

**Decision**: Use a single `Directory.Build.props` at the repository root
to set shared MSBuild properties for all projects.

**Rationale**: Centralizes framework version, nullable analysis, and
warnings-as-errors settings. Any new project added to the solution
automatically inherits these settings.

**Properties to set**:
- `<TargetFramework>net8.0</TargetFramework>`
- `<Nullable>enable</Nullable>`
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- `<ImplicitUsings>enable</ImplicitUsings>`

**Alternatives considered**:
- Repeating settings in each `.csproj` — rejected: error-prone and
  inconsistent.
