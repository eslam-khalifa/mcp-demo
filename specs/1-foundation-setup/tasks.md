# Tasks: Foundation & Project Setup

**Input**: Design documents from `/specs/1-foundation-setup/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Not requested for this phase. Test projects are scaffolded (empty).

**Organization**: Tasks are grouped by user story to enable independent
implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Solution root**: `MCPDemo.sln`, `Directory.Build.props`
- **Source projects**: `src/MCPDemo.{ProjectName}/`
- **Test projects**: `tests/MCPDemo.{ProjectName}.Tests/`

---

## Phase 1: Setup

**Purpose**: Create the solution file and global build configuration.

- [x] T001 Create solution file `MCPDemo.sln` at repository root
- [x] T002 Create `Directory.Build.props` at repository root with global settings: `TargetFramework=net8.0`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `ImplicitUsings=enable`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create all 8 project files with correct references and add them to the solution. NO user story work can begin until this phase is complete.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Source Projects

- [x] T003 Create `src/MCPDemo.Domain/MCPDemo.Domain.csproj` as classlib with zero project references and add to solution
- [x] T004 [P] Create `src/MCPDemo.Shared/MCPDemo.Shared.csproj` as classlib with zero project references and add to solution
- [x] T005 Create `src/MCPDemo.Application/MCPDemo.Application.csproj` as classlib with project reference to MCPDemo.Domain and add to solution (depends on T003)
- [x] T006 Create `src/MCPDemo.Infrastructure/MCPDemo.Infrastructure.csproj` as classlib with project references to MCPDemo.Domain and MCPDemo.Shared, and add to solution (depends on T003, T004)
- [x] T007 Create `src/MCPDemo.Api/MCPDemo.Api.csproj` as console app with project references to MCPDemo.Application, MCPDemo.Infrastructure, and MCPDemo.Shared, and add to solution (depends on T004, T005, T006)
- [x] T008 Create minimal `src/MCPDemo.Api/Program.cs` placeholder that compiles (empty Main or top-level statements)

### Test Projects

- [x] T009 [P] Create `tests/MCPDemo.Application.Tests/MCPDemo.Application.Tests.csproj` with references to MCPDemo.Application and MCPDemo.Domain, add xUnit SDK references, and add to solution
- [x] T010 [P] Create `tests/MCPDemo.Infrastructure.Tests/MCPDemo.Infrastructure.Tests.csproj` with references to MCPDemo.Infrastructure, MCPDemo.Domain, and MCPDemo.Shared, add xUnit SDK references, and add to solution
- [x] T011 [P] Create `tests/MCPDemo.Integration.Tests/MCPDemo.Integration.Tests.csproj` with references to MCPDemo.Api, MCPDemo.Application, MCPDemo.Infrastructure, and MCPDemo.Shared, add xUnit SDK references, and add to solution

**Checkpoint**: Solution builds with zero errors and zero warnings. All 8 projects compile. User Story 1 (P1) acceptance criteria met.

---

## Phase 3: User Story 2 — Domain Entities (Priority: P2) 🎯

**Goal**: Define all domain entities, value objects, and domain exceptions so downstream features can express business logic.

**Independent Test**: Instantiate Product, Category, and PriceRange; verify all attributes are accessible; confirm PriceRange validation rejects invalid ranges.

### Implementation for User Story 2

- [x] T012 [P] [US2] Create Product entity in `src/MCPDemo.Domain/Entities/Product.cs` with attributes: Id (int), Title (string), Slug (string), Price (decimal), Description (string), Category (Category), Images (List<string>), CreationAt (DateTime), UpdatedAt (DateTime)
- [x] T013 [P] [US2] Create Category entity in `src/MCPDemo.Domain/Entities/Category.cs` with attributes: Id (int), Name (string), Slug (string), Image (string), CreationAt (DateTime), UpdatedAt (DateTime)
- [x] T014 [P] [US2] Create PriceRange value object as immutable record in `src/MCPDemo.Domain/ValueObjects/PriceRange.cs` with Min (decimal), Max (decimal), and IsValid() method that returns false if Min < 0 or Max < Min
- [x] T015 [P] [US2] Create DomainException base class in `src/MCPDemo.Domain/Exceptions/DomainException.cs` inheriting from System.Exception with message constructor
- [x] T016 [P] [US2] Create EntityNotFoundException in `src/MCPDemo.Domain/Exceptions/EntityNotFoundException.cs` inheriting from DomainException with EntityType (string) and EntityId (string) properties

**Checkpoint**: All domain entities compile. PriceRange.IsValid() returns correct results for valid/invalid ranges. Domain project still has zero external references.

---

## Phase 4: User Story 3 — Shared Utilities (Priority: P3)

**Goal**: Provide cross-cutting utilities (Result wrapper, error model, constants, exceptions) available to all layers.

**Independent Test**: Create Result.Success and Result.Failure instances; verify null payload throws; verify ErrorResponse holds all fields; verify constants are correct.

### Implementation for User Story 3

- [x] T017 [P] [US3] Create generic Result<T> wrapper in `src/MCPDemo.Shared/Models/Result.cs` with: IsSuccess (bool), IsFailure (bool), Value (T — throws InvalidOperationException on failure), Error (string), static Success(T value) factory that throws ArgumentNullException if value is null, and static Failure(string error) factory
- [x] T018 [P] [US3] Create ErrorResponse model in `src/MCPDemo.Shared/Models/ErrorResponse.cs` with: ErrorType (string), Message (string), ToolName (string?), Timestamp (DateTime)
- [x] T019 [P] [US3] Create ApiConstants in `src/MCPDemo.Shared/Constants/ApiConstants.cs` with: BaseUrl = "https://api.escuelajs.co/api/v1", ProductsEndpoint = "products", CategoriesEndpoint = "categories"
- [x] T020 [P] [US3] Create McpToolException in `src/MCPDemo.Shared/Exceptions/McpToolException.cs` inheriting from System.Exception with ToolName (string) and ErrorType (string) properties
- [x] T021 [P] [US3] Create ExternalApiException in `src/MCPDemo.Shared/Exceptions/ExternalApiException.cs` inheriting from System.Exception with StatusCode (int) and ResponseBody (string) properties
- [x] T022 [P] [US3] Create StringExtensions in `src/MCPDemo.Shared/Extensions/StringExtensions.cs` with basic utility methods (e.g., ToSlug(), IsNullOrWhitespace() guard)

**Checkpoint**: All shared models compile. Result<T>.Success(value) works; Result<T>.Success(null) throws ArgumentNullException. Shared project still has zero project references.

---

## Phase 5: User Story 4 — Service Contracts (Priority: P4)

**Goal**: Define all service interfaces and DTOs in the Application layer so downstream implementation can code against stable contracts.

**Independent Test**: Each interface compiles. A mock implementation of any interface compiles without errors.

### DTOs for User Story 4

- [x] T023 [P] [US4] Create CreateProductDto in `src/MCPDemo.Application/DTOs/Products/CreateProductDto.cs` with: Title (string), Price (decimal), Description (string), CategoryId (int), Images (List<string>)
- [x] T024 [P] [US4] Create UpdateProductDto in `src/MCPDemo.Application/DTOs/Products/UpdateProductDto.cs` with: Title (string?), Price (decimal?), Description (string?), CategoryId (int?), Images (List<string>?)
- [x] T025 [P] [US4] Create CreateCategoryDto in `src/MCPDemo.Application/DTOs/Categories/CreateCategoryDto.cs` with: Name (string), Image (string)
- [x] T026 [P] [US4] Create UpdateCategoryDto in `src/MCPDemo.Application/DTOs/Categories/UpdateCategoryDto.cs` with: Name (string?), Image (string?)
- [x] T027 [P] [US4] Create SearchProductsDto in `src/MCPDemo.Application/DTOs/SearchProductsDto.cs` with: Title (string?), Price (decimal?), PriceMin (decimal?), PriceMax (decimal?), CategoryId (int?), CategorySlug (string?), Offset (int?), Limit (int?)
- [x] T028 [P] [US4] Create analytics response DTOs in `src/MCPDemo.Application/DTOs/Analytics/`: PriceStatisticsDto (Min, Max, Mean, Median, StdDev, Count), TopExpensiveDto (Products list, AveragePrice), CategoryReportDto (CategoryId, CategoryName, AveragePrice, MinPrice, MaxPrice, ProductCount), PriceDistributionDto (Buckets list with RangeMin, RangeMax, Count)

### Service Interfaces for User Story 4

- [x] T029 [P] [US4] Create IProductService in `src/MCPDemo.Application/Interfaces/IProductService.cs` with 8 methods: GetAllAsync, GetByIdAsync, GetBySlugAsync, CreateAsync, UpdateAsync, DeleteAsync, GetRelatedByIdAsync, GetRelatedBySlugAsync — all returning Task<Result<T>> (depends on T012, T017, T023, T024)
- [x] T030 [P] [US4] Create ICategoryService in `src/MCPDemo.Application/Interfaces/ICategoryService.cs` with 7 methods: GetAllAsync, GetByIdAsync, GetBySlugAsync, CreateAsync, UpdateAsync, DeleteAsync, GetProductsAsync — all returning Task<Result<T>> (depends on T013, T017, T025, T026)
- [x] T031 [P] [US4] Create ISearchService in `src/MCPDemo.Application/Interfaces/ISearchService.cs` with 1 method: SearchProductsAsync(SearchProductsDto) returning Task<Result<IEnumerable<Product>>> (depends on T012, T017, T027)
- [x] T032 [P] [US4] Create IAnalyticsService in `src/MCPDemo.Application/Interfaces/IAnalyticsService.cs` with 4 methods: GetPriceStatisticsAsync, GetTopExpensiveProductsAsync, GetCategoryPriceReportAsync, GetPriceDistributionAsync — all returning Task<Result<T>> (depends on T017, T028)
- [x] T033 [P] [US4] Create IPythonSandboxService in `src/MCPDemo.Application/Interfaces/IPythonSandboxService.cs` with 1 method: ExecuteAsync(string toolName, string jsonInput) returning Task<Result<string>> (depends on T017)

**Checkpoint**: All interfaces and DTOs compile. Application project references only Domain. Any interface can be mock-implemented (stub class) without errors.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup.

- [x] T034 Verify full solution build: run `dotnet build MCPDemo.sln` and confirm zero errors and zero warnings with warnings-as-errors and nullable analysis enabled
- [x] T035 [P] Verify project dependency graph matches documented architecture in plan.md — confirm Domain has zero refs, Shared has zero refs, Application refs Domain only, Infrastructure refs Domain + Shared, Api refs Application + Infrastructure + Shared
- [x] T036 [P] Run quickstart.md smoke test validation: build succeeds, entities instantiable, PriceRange validation works, Result<T> enforces non-null, interfaces mockable
- [x] T037 [P] Add XML documentation comments to all public types and interface methods for IntelliSense support

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US2 Domain Entities (Phase 3)**: Depends on Foundational (T003 specifically)
- **US3 Shared Utilities (Phase 4)**: Depends on Foundational (T004 specifically)
- **US4 Service Contracts (Phase 5)**: Depends on US2 + US3 (needs entities + Result<T> + DTOs)
- **Polish (Phase 6)**: Depends on all user stories complete

### User Story Dependencies

- **US2 (P2)**: Can start after Foundational — needs only MCPDemo.Domain project
- **US3 (P3)**: Can start after Foundational — needs only MCPDemo.Shared project
- **US4 (P4)**: Depends on US2 + US3 — interfaces reference Domain entities and Shared Result<T>
- **US2 and US3 can run in PARALLEL** since they operate on different projects

### Within Each User Story

- All tasks within US2 are parallelizable (different files in Domain)
- All tasks within US3 are parallelizable (different files in Shared)
- US4: DTOs (T023-T028) can run in parallel, then interfaces (T029-T033) can run in parallel

---

## Parallel Execution Examples

### Phase 2: Foundational (after T001-T002 complete)

```bash
# These can run in parallel (independent projects):
Task T003: Create MCPDemo.Domain project
Task T004: Create MCPDemo.Shared project

# Then these can run in parallel (depend on T003/T004):
Task T005: Create MCPDemo.Application project (needs T003)
Task T006: Create MCPDemo.Infrastructure project (needs T003, T004)

# Then (depends on T004, T005, T006):
Task T007: Create MCPDemo.Api project

# Test projects can run in parallel (after their dependencies):
Task T009: Create Application.Tests (needs T005)
Task T010: Create Infrastructure.Tests (needs T006)
Task T011: Create Integration.Tests (needs T007)
```

### Phase 3 + Phase 4: US2 and US3 in parallel

```bash
# US2: All domain files in parallel
Task T012: Product.cs
Task T013: Category.cs
Task T014: PriceRange.cs
Task T015: DomainException.cs
Task T016: EntityNotFoundException.cs

# US3: All shared files in parallel (SAME TIME as US2!)
Task T017: Result.cs
Task T018: ErrorResponse.cs
Task T019: ApiConstants.cs
Task T020: McpToolException.cs
Task T021: ExternalApiException.cs
Task T022: StringExtensions.cs
```

### Phase 5: US4 (after US2 + US3 complete)

```bash
# All DTOs in parallel:
Task T023-T028: All DTO files

# Then all interfaces in parallel (after DTOs):
Task T029-T033: All interface files
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T011)
3. **STOP and VALIDATE**: Build succeeds → US1 complete ✅
4. Complete Phase 3: Domain Entities (T012-T016) → US2 complete ✅
5. Deploy/demo: compilable solution with domain model

### Full Delivery

1. Setup + Foundational → US1 ✅
2. Domain Entities → US2 ✅
3. Shared Utilities (parallel with step 2!) → US3 ✅
4. Service Contracts → US4 ✅
5. Polish → Feature complete ✅

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- US2 and US3 are independent and can be worked on in parallel
- US4 depends on both US2 and US3 (needs entities + Result type)
- Verify tests fail before implementing (N/A — no test tasks in this phase)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
