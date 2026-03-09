# Tasks: Core Services & Infrastructure

**Input**: Design documents from `specs/2-core-services/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-client-interface.md

**Tests**: Not explicitly requested in spec. Test tasks omitted per workflow rules.

**Organization**: Tasks grouped by user story (US1–US5) to enable independent implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Exact file paths included in all descriptions

---

## Phase 1: Setup (NuGet Dependencies)

**Purpose**: Install required NuGet packages before any implementation.

- [x] T001 Add `Microsoft.Extensions.Http` package to `src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj`
- [x] T002 [P] Add `Serilog.AspNetCore` package to `src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj`
- [x] T003 [P] Add `Serilog.Sinks.File` package to `src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj`
- [x] T004 [P] Add `Serilog.Formatting.Compact` package to `src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj`
- [x] T005 [P] Add `NSubstitute` and `FluentAssertions` packages to `tests/MCPDemo.Application.Tests/MCPDemo.Application.Tests.csproj`
- [x] T006 [P] Add `NSubstitute` and `FluentAssertions` packages to `tests/MCPDemo.Infrastructure.Tests/MCPDemo.Infrastructure.Tests.csproj`
- [x] T007 Verify full solution build: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: All NuGet packages installed. Solution builds cleanly.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T008 Create `IPlatziStoreApiClient` interface in `src/MCPDemo.Application/Interfaces/IPlatziStoreApiClient.cs` with all 16 methods (8 product + 7 category + 1 search) returning domain entities, each method async (depends on existing DTOs from Phase 1)
- [x] T009 Create `IMetricsCollector` interface in `src/MCPDemo.Infrastructure/Metrics/IMetricsCollector.cs` with `RecordExecution(string toolName, long elapsedMs, bool success, string? errorType)`, `GetMetrics(string toolName)`, and `GetAllMetrics()` methods
- [x] T010 [P] Create `ToolMetrics` data model in `src/MCPDemo.Infrastructure/Metrics/ToolMetrics.cs` with: TotalCalls (int), SuccessCount (int), FailureCount (int), AverageExecutionTimeMs (double), TotalExecutionTimeMs (long), ErrorsByType (Dictionary<string, int>)
- [x] T011 Delete auto-generated `Class1.cs` placeholder files from `src/MCPDemo.Application/` and `src/MCPDemo.Infrastructure/` if they still exist
- [x] T012 Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: Foundation ready — all interfaces defined, metrics model created. User story implementation can now begin.

---

## Phase 3: User Story 1 — API Client with Retry (Priority: P1) 🎯 MVP

**Goal**: Implement the HTTP client that communicates with all Platzi Fake Store API endpoints, including retry logic (2 retries on 5xx/timeout), 15-second per-request timeout, and response mapping to domain entities.

**Independent Test**: Mock `HttpMessageHandler` and verify URL construction, request bodies, retry behavior on 5xx, no-retry on 4xx, timeout handling, and correct response deserialization.

### Implementation for User Story 1

- [x] T013 [US1] Implement `PlatziStoreApiClient` constructor and `JsonSerializerOptions` setup in `src/MCPDemo.Infrastructure/ExternalApi/PlatziStoreApiClient.cs` — accept `HttpClient` via DI, configure camelCase + case-insensitive deserialization
- [x] T014 [US1] Implement private `SendWithRetryAsync` method in `src/MCPDemo.Infrastructure/ExternalApi/PlatziStoreApiClient.cs` — retry up to 2 times on 5xx and `TaskCanceledException` (timeout), exponential backoff (500ms × attempt), no retry on 4xx
- [x] T015 [US1] Implement product GET methods in `src/MCPDemo.Infrastructure/ExternalApi/PlatziStoreApiClient.cs`: `GetAllProductsAsync` (with offset/limit query params), `GetProductByIdAsync`, `GetProductBySlugAsync`, `GetRelatedProductsByIdAsync`, `GetRelatedProductsBySlugAsync`
- [x] T016 [US1] Implement product mutation methods in `src/MCPDemo.Infrastructure/ExternalApi/PlatziStoreApiClient.cs`: `CreateProductAsync` (POST with DTO body), `UpdateProductAsync` (PUT with DTO body), `DeleteProductAsync` (DELETE, return bool)
- [x] T017 [US1] Implement category methods in `src/MCPDemo.Infrastructure/ExternalApi/PlatziStoreApiClient.cs`: `GetAllCategoriesAsync`, `GetCategoryByIdAsync`, `GetCategoryBySlugAsync`, `CreateCategoryAsync`, `UpdateCategoryAsync`, `DeleteCategoryAsync`, `GetProductsByCategoryAsync`
- [x] T018 [US1] Implement `SearchProductsAsync` in `src/MCPDemo.Infrastructure/ExternalApi/PlatziStoreApiClient.cs` — build query string from `SearchProductsDto` by including only non-null filter parameters (title, price, price_min, price_max, categoryId, categorySlug, offset, limit)
- [x] T019 [US1] Implement error handling in `PlatziStoreApiClient`: throw `EntityNotFoundException` on 404, throw `ExternalApiException` on other 4xx/5xx (after retries exhausted), include status code and response body in exception
- [x] T020 [US1] Verify Infrastructure project builds: `dotnet build src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj` — zero errors, zero warnings

**Checkpoint**: API client compiles and implements all 16 endpoints with retry logic. Can be tested independently with mocked HTTP handler.

---

## Phase 4: User Story 2 — Product Service (Priority: P2)

**Goal**: Implement `ProductService` that validates inputs, delegates to the API client, and wraps all responses in `Result<T>`.

**Independent Test**: Mock `IPlatziStoreApiClient` and verify validation logic, delegation, and Result wrapping.

### Implementation for User Story 2

- [x] T021 [US2] Implement `ProductService` class in `src/MCPDemo.Application/Services/ProductService.cs` — constructor accepts `IPlatziStoreApiClient` and `ILogger<ProductService>` via DI, implements `IProductService`
- [x] T022 [US2] Implement read operations in `ProductService`: `GetAllAsync`, `GetByIdAsync`, `GetBySlugAsync`, `GetRelatedByIdAsync`, `GetRelatedBySlugAsync` — delegate to API client, wrap in `Result<T>.Success()`, catch exceptions and return `Result<T>.Failure()`
- [x] T023 [US2] Implement `CreateAsync` in `ProductService` with input validation: reject empty Title (return failure "Product title is required"), reject Price < 0 (return failure "Product price must be non-negative"), reject CategoryId <= 0 (return failure "Valid category ID is required"), reject null/empty Images (return failure "At least one image URL is required")
- [x] T024 [US2] Implement `UpdateAsync` in `ProductService` with validation: reject empty update DTO where all fields are null (return failure "No fields to update"), validate Price >= 0 if provided, validate CategoryId > 0 if provided
- [x] T025 [US2] Implement `DeleteAsync` in `ProductService` — delegate to API client, wrap bool result in `Result<bool>`
- [x] T026 [US2] Verify Application project builds: `dotnet build src/MCPDemo.Application/MCPDemo.Application.csproj` — zero errors, zero warnings

**Checkpoint**: ProductService validates all inputs, delegates to API client, and wraps responses. Independently testable by mocking API client.

---

## Phase 5: User Story 3 — Category Service (Priority: P3)

**Goal**: Implement `CategoryService` following the same patterns as `ProductService`.

**Independent Test**: Mock `IPlatziStoreApiClient` and verify validation and Result wrapping.

### Implementation for User Story 3

- [x] T027 [US3] Implement `CategoryService` class in `src/MCPDemo.Application/Services/CategoryService.cs` — constructor accepts `IPlatziStoreApiClient` and `ILogger<CategoryService>` via DI, implements `ICategoryService`
- [x] T028 [US3] Implement read operations in `CategoryService`: `GetAllAsync`, `GetByIdAsync`, `GetBySlugAsync`, `GetProductsAsync` — delegate to API client, wrap in `Result<T>`
- [x] T029 [US3] Implement `CreateAsync` in `CategoryService` with input validation: reject empty Name (return failure "Category name is required"), reject empty Image (return failure "Category image URL is required")
- [x] T030 [US3] Implement `UpdateAsync` in `CategoryService` with validation: reject empty update DTO where all fields are null (return failure "No fields to update")
- [x] T031 [US3] Implement `DeleteAsync` in `CategoryService` — delegate to API client, wrap bool result in `Result<bool>`
- [x] T032 [US3] Verify Application project builds: `dotnet build src/MCPDemo.Application/MCPDemo.Application.csproj` — zero errors, zero warnings

**Checkpoint**: CategoryService validates all inputs and wraps responses. Independently testable.

---

## Phase 6: User Story 4 — Search Service (Priority: P4)

**Goal**: Implement `SearchService` that passes combinable filter criteria to the API client.

**Independent Test**: Mock `IPlatziStoreApiClient` and verify correct delegation with various filter combinations.

### Implementation for User Story 4

- [x] T033 [US4] Implement `SearchService` class in `src/MCPDemo.Application/Services/SearchService.cs` — constructor accepts `IPlatziStoreApiClient` and `ILogger<SearchService>` via DI, implements `ISearchService`
- [x] T034 [US4] Implement `SearchProductsAsync` in `SearchService` — delegate `SearchProductsDto` directly to `IPlatziStoreApiClient.SearchProductsAsync()`, wrap response in `Result<T>`, handle exceptions as failure Results
- [x] T035 [US4] Verify full solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: SearchService compiles and delegates filter criteria. All 3 services now operational.

---

## Phase 7: User Story 5 — Logging & Metrics (Priority: P5)

**Goal**: Configure structured logging and implement thread-safe in-memory metrics collection.

**Independent Test**: Invoke a service method, verify structured log output and metrics counter accuracy under concurrent access.

### Implementation for User Story 5

- [x] T036 [US5] Implement `SerilogConfiguration` in `src/MCPDemo.Infrastructure/Logging/SerilogConfiguration.cs` — configure daily rolling JSON file sink at `logs/mcp-tool-.log`
- [x] T037 [US5] Implement `InMemoryMetricsCollector` in `src/MCPDemo.Infrastructure/Metrics/InMemoryMetricsCollector.cs` — use `ConcurrentDictionary` and `Interlocked` (or `lock`) for thread-safe per-tool metrics aggregation
- [x] T038 [US5] Instrument `ProductService` with logging: log tool name and input parameters (e.g., product ID or title) at Information level, log exceptions at Error level with structured properties
- [x] T039 [US5] Instrument `CategoryService` with logging: log tool name and category ID/name, log errors
- [x] T040 [US5] Instrument `SearchService` with logging: log tool name and filter summary, log errors
- [x] T041 [US5] Instrument `ProductService` with metrics: use `Stopwatch` to measure execution time, record success/failure and elapsed time in `IMetricsCollector`
- [x] T042 [US5] Instrument `CategoryService` with metrics: measure execution time and record to collector
- [x] T043 [US5] Instrument `SearchService` with metrics: measure execution time and record to collector
- [x] T044 [US5] Verify full solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: All service methods produce structured log entries and update metrics. MetricsCollector is thread-safe.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup.

- [x] T045 Verify full solution build with warnings-as-errors: `dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true` — zero errors, zero warnings
- [x] T046 [P] Verify project dependency graph: Infrastructure does NOT reference Application, Application does NOT reference Infrastructure, Domain has zero external refs
- [x] T047 [P] Add XML documentation comments to all public methods in `PlatziStoreApiClient`, `ProductService`, `CategoryService`, `SearchService`, `InMemoryMetricsCollector`, and `SerilogConfiguration`
- [x] T048 [P] Run quickstart.md smoke test validation: all key files exist, solution builds, no architecture violations
- [x] T049 Remove any unused `using` statements or auto-generated placeholder files across all modified projects

**Checkpoint**: Phase 2 feature complete. All 23 functional requirements satisfied. Solution builds with zero warnings.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1: API Client (Phase 3)**: Depends on Foundational — BLOCKS US2, US3, US4
- **US2: ProductService (Phase 4)**: Depends on US1 (needs API client implementation)
- **US3: CategoryService (Phase 5)**: Depends on US1 (needs API client implementation)
- **US4: SearchService (Phase 6)**: Depends on US1 (needs API client implementation)
- **US5: Logging & Metrics (Phase 7)**: Depends on US2, US3, US4 (instruments existing services)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 → Phase 2 → US1 (P1) → US2 (P2) ─┐
                         │                  │
                         ├─→ US3 (P3) ──────┼─→ US5 (P5) → Polish
                         │                  │
                         └─→ US4 (P4) ──────┘
```

- **US1 (API Client)**: Foundation for all services — MUST complete first
- **US2, US3, US4**: Can run in PARALLEL after US1 completes
- **US5 (Logging/Metrics)**: Runs after US2–US4 (instruments existing services)

### Parallel Opportunities

**Within Phase 1**: T002–T006 can all run in parallel (independent NuGet additions)

**Within Phase 2**: T009 and T010 can run in parallel (different files)

**After US1 completes**: US2, US3, US4 can run simultaneously:
```
US2 tasks (T021–T026) in parallel with
US3 tasks (T027–T032) in parallel with
US4 tasks (T033–T035)
```

**Within US5**: T036 and T037 can run in parallel (different files)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (NuGet packages)
2. Complete Phase 2: Foundational (interfaces, metrics model)
3. Complete Phase 3: US1 — API Client with retry
4. **STOP and VALIDATE**: Verify API client works with mocked HTTP handler
5. The API client is the MVP — it enables all downstream services

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 (API Client) → Core data access working (MVP!)
3. US2 + US3 + US4 (Services) → Business logic orchestration
4. US5 (Logging/Metrics) → Observability layer
5. Polish → Production readiness

---

## Notes

- All services depend on `IPlatziStoreApiClient` — the API client is the critical path
- US2, US3, US4 follow identical patterns (validate → delegate → wrap) — implement US2 first as the template, then US3 and US4 follow quickly
- Logging and metrics (US5) are additive — they modify existing service methods WITHOUT changing behavior
- The `IMetricsCollector` interface lives in Application; implementation is in Infrastructure
- Total tasks: 49
