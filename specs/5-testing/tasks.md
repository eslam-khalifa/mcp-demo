# Tasks: Unit & Integration Testing

**Input**: Design documents from `specs/5-testing/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: This feature IS the test implementation. All tasks are test-related.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create test directory structure and shared test utilities used across all test projects.

- [X] T001 Create directory structure for Application tests: `tests/MCPDemo.Application.Tests/Services/`
- [X] T002 [P] Create directory structure for Infrastructure tests: `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/`, `tests/MCPDemo.Infrastructure.Tests/Metrics/`, `tests/MCPDemo.Infrastructure.Tests/ExternalApi/`
- [X] T003 [P] Create directory structure for Integration tests: `tests/MCPDemo.Integration.Tests/Services/`, `tests/MCPDemo.Integration.Tests/PythonSandbox/`
- [X] T004 Add missing project references to `tests/MCPDemo.Infrastructure.Tests/MCPDemo.Infrastructure.Tests.csproj` — ensure it references `MCPDemo.Application` (needed for `IMetricsCollector`, `IPythonSandboxService`)
- [X] T005 Verify solution builds: `dotnet build MCPDemo.sln` — zero errors, zero warnings

**Checkpoint**: All test projects compile and reference correct source projects. Ready for test implementation.

---

## Phase 2: User Story 1 — Application Service Unit Tests (Priority: P1) 🎯 MVP

**Goal**: Unit tests for `ProductService`, `CategoryService`, and `SearchService` covering success paths, validation failures, error handling, and metrics recording.

**Independent Test**: Run `dotnet test tests/MCPDemo.Application.Tests/ --filter "Category!=Integration&Category!=Docker"` — all tests pass.

### Implementation for User Story 1

- [X] T006 [US1] Create `ProductServiceTests.cs` in `tests/MCPDemo.Application.Tests/Services/` — test fixture with mocked `IPlatziStoreApiClient`, `ILogger<ProductService>`, `IMetricsCollector`; include test: `GetAllAsync_ApiReturnsProducts_ReturnsSuccessWithProducts`
- [X] T007 [US1] Add test `GetByIdAsync_ValidId_ReturnsSuccessWithProduct` in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs`
- [X] T008 [US1] Add test `GetByIdAsync_ApiThrows_ReturnsFailureWithMessage` in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs`
- [X] T009 [US1] Add test `GetBySlugAsync_ValidSlug_ReturnsSuccessWithProduct` in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs`
- [X] T010 [US1] Add tests for `CreateAsync` validation in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs` — 4 tests: `CreateAsync_EmptyTitle_ReturnsFailure`, `CreateAsync_NegativePrice_ReturnsFailure`, `CreateAsync_InvalidCategoryId_ReturnsFailure`, `CreateAsync_NoImages_ReturnsFailure`
- [X] T011 [US1] Add test `CreateAsync_ValidDto_ReturnsSuccessWithProduct` in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs`
- [X] T012 [US1] Add tests for `UpdateAsync` validation in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs` — 3 tests: `UpdateAsync_AllNullFields_ReturnsFailure`, `UpdateAsync_NegativePrice_ReturnsFailure`, `UpdateAsync_InvalidCategoryId_ReturnsFailure`
- [X] T013 [US1] Add test `UpdateAsync_ValidDto_ReturnsSuccessWithProduct` in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs`
- [X] T014 [US1] Add test `DeleteAsync_ValidId_ReturnsSuccessTrue` in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs`
- [X] T015 [US1] Add tests `GetRelatedByIdAsync_ValidId_ReturnsSuccess` and `GetRelatedBySlugAsync_ValidSlug_ReturnsSuccess` in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs`
- [X] T016 [US1] Add metrics verification tests in `tests/MCPDemo.Application.Tests/Services/ProductServiceTests.cs` — 2 tests: `GetByIdAsync_Success_RecordsSuccessMetric`, `GetByIdAsync_ApiThrows_RecordsFailureMetric`
- [X] T017 [P] [US1] Create `CategoryServiceTests.cs` in `tests/MCPDemo.Application.Tests/Services/` — test fixture with mocked dependencies; tests: `GetAllAsync_ReturnsSuccess`, `GetByIdAsync_ValidId_ReturnsSuccess`, `GetBySlugAsync_ValidSlug_ReturnsSuccess`
- [X] T018 [US1] Add `CreateAsync` validation tests in `tests/MCPDemo.Application.Tests/Services/CategoryServiceTests.cs` — 2 tests: `CreateAsync_EmptyName_ReturnsFailure`, `CreateAsync_EmptyImage_ReturnsFailure`
- [X] T019 [US1] Add tests `CreateAsync_ValidDto_ReturnsSuccess`, `UpdateAsync_AllNullFields_ReturnsFailure`, `UpdateAsync_ValidDto_ReturnsSuccess`, `DeleteAsync_ValidId_ReturnsSuccess`, `GetProductsAsync_ValidCategoryId_ReturnsSuccess` in `tests/MCPDemo.Application.Tests/Services/CategoryServiceTests.cs`
- [X] T020 [US1] Add metrics verification tests in `tests/MCPDemo.Application.Tests/Services/CategoryServiceTests.cs` — `GetAllAsync_Success_RecordsSuccessMetric`, `CreateAsync_ApiThrows_RecordsFailureMetric`
- [X] T021 [P] [US1] Create `SearchServiceTests.cs` in `tests/MCPDemo.Application.Tests/Services/` — test fixture with mocked dependencies; tests: `SearchProductsAsync_ValidFilters_ReturnsSuccess`, `SearchProductsAsync_ApiThrows_ReturnsFailure`, `SearchProductsAsync_Success_RecordsSuccessMetric`, `SearchProductsAsync_Failure_RecordsFailureMetric`
- [X] T022 [US1] Run all Application tests: `dotnet test tests/MCPDemo.Application.Tests/` — verify ≥30 tests pass with zero failures

**Checkpoint**: All Application service methods have unit tests. Validation, success, failure, and metrics recording are all covered.

---

## Phase 3: User Story 2 — Infrastructure Unit Tests (Priority: P1)

**Goal**: Unit tests for `PythonSandboxService`, `InMemoryMetricsCollector`, and `PlatziStoreApiClient` covering execution flows, thread safety, Docker argument construction, and HTTP mocking.

**Independent Test**: Run `dotnet test tests/MCPDemo.Infrastructure.Tests/` — all tests pass.

### Implementation for User Story 2

- [X] T023 [P] [US2] Create `PythonSandboxServiceTests.cs` in `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/` — test fixture with mocked `IDockerProcessRunner`, `ILogger<PythonSandboxService>`, `IMetricsCollector`
- [X] T024 [P] [US2] Add test `ExecuteAsync_ValidCode_ReturnsStdout` in `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/PythonSandboxServiceTests.cs`
- [X] T025 [P] [US2] Add test `ExecuteAsync_NonZeroExitCode_ThrowsPythonSandboxException` in `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/PythonSandboxServiceTests.cs`
- [X] T026 [P] [US2] Add test `ExecuteAsync_Timeout_ThrowsPythonSandboxException` in `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/PythonSandboxServiceTests.cs`
- [X] T027 [P] [US2] Add test `ExecuteAsync_DockerArguments_ContainAllSecurityFlags` in `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/PythonSandboxServiceTests.cs` — verify `--rm`, `-i`, memory limits, CPU limits, no-network, read-only
- [X] T028 [P] [US2] Add metrics verification tests in `tests/MCPDemo.Infrastructure.Tests/PythonSandbox/PythonSandboxServiceTests.cs` — verify `RecordExecution` called on success, failure, and timeout
- [X] T029 [P] [US2] Create `InMemoryMetricsCollectorTests.cs` in `tests/MCPDemo.Infrastructure.Tests/Metrics/` — test aggregate calculations (Total, Success, Average Time)
- [X] T030 [US2] Add test `RecordExecution_ConcurrentCalls_NoDataLoss` in `tests/MCPDemo.Infrastructure.Tests/Metrics/InMemoryMetricsCollectorTests.cs` — use `Parallel.For` or multiple tasks to verify thread safety
- [X] T031 [US2] Add tests for error aggregation by type in `tests/MCPDemo.Infrastructure.Tests/Metrics/InMemoryMetricsCollectorTests.cs`
- [X] T032 [US2] Add tests for `GetMetrics` and `GetAllMetrics` in `tests/MCPDemo.Infrastructure.Tests/Metrics/InMemoryMetricsCollectorTests.cs`
- [X] T033 [P] [US2] Create `PlatziStoreApiClientTests.cs` in `tests/MCPDemo.Infrastructure.Tests/ExternalApi/` — use `MockHttpMessageHandler` to mock HTTP responses
- [X] T034 [P] [US2] Add tests for all CRUD methods in `tests/MCPDemo.Infrastructure.Tests/ExternalApi/PlatziStoreApiClientTests.cs` — verify URI construction and payload serialization
- [X] T035 [P] [US2] Add test for 404 (NotFound) handling in `tests/MCPDemo.Infrastructure.Tests/ExternalApi/PlatziStoreApiClientTests.cs`
- [X] T036 [P] [US2] Add test for 500 (ServerError) and retry logic in `tests/MCPDemo.Infrastructure.Tests/ExternalApi/PlatziStoreApiClientTests.cs`
- [X] T037 [US2] Run all Infrastructure tests: `dotnet test tests/MCPDemo.Infrastructure.Tests/` — verify PASS status for all infrastructure components

**Checkpoint**: All Infrastructure components have unit tests. Sandbox security, metrics thread safety, and HTTP error handling are verified.

---

## Phase 4: User Story 3 — Integration Tests (Priority: P2)

**Goal**: End-to-end tests that verify the full pipeline against the real Platzi API and Docker sandbox.

**Independent Test**: Run `dotnet test tests/MCPDemo.Integration.Tests/ --filter "Category=Integration|Category=Docker"` — all tests pass (requires network + Docker).

### Implementation for User Story 3

- [X] T038 [P] [US3] Create `ProductIntegrationTests.cs` in `tests/MCPDemo.Integration.Tests/Services/` — tagged with `[Trait("Category", "Integration")]`; test: `Integration_GetAllProducts_ReturnsRealData` (verify logic + live API)
- [X] T039 [US3] Add test `Integration_GetProductById_ReturnsValidProduct` in `tests/MCPDemo.Integration.Tests/Services/ProductIntegrationTests.cs`
- [X] T040 [US3] Add test `Integration_SearchProducts_ReturnsFilteredResults` in `tests/MCPDemo.Integration.Tests/Services/ProductIntegrationTests.cs`
- [X] T041 [US3] Add category/search integration tests in `tests/MCPDemo.Integration.Tests/Services/ProductIntegrationTests.cs` (Categories, Search)
- [X] T042 [P] [US3] Create `PythonIntegrationTests.cs` in `tests/MCPDemo.Integration.Tests/PythonSandbox/` — tagged with `[Trait("Category", "Docker")]`; test: `Integration_ExecuteHello_ReturnsCorrectOutput` (verify connectivity to local Docker engine)
- [X] T043 [US3] Add test `Integration_ExecuteWithData_ReturnsProcessedJson` in `tests/MCPDemo.Integration.Tests/PythonSandbox/PythonIntegrationTests.cs` — verify data passing to/from Python sandbox
- [X] T044 [US3] Run all Integration tests: `dotnet test tests/MCPDemo.Integration.Tests/` — verify PASS status (requires Docker and internet)

**Checkpoint**: Full pipeline verified end-to-end. All layers integrate correctly.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, code coverage, and solution health.

- [ ] T044 Run all tests together: `dotnet test MCPDemo.sln` — verify ≥50 total tests, zero failures
- [X] T045 [US4] Generate final code coverage report: `dotnet test --collect:"XPlat Code Coverage"` — verify ≥ 40 tests in total
- [X] T046 [US4] Update `README.md` and `quickstart.md` with walkthrough and testing instructions
- [X] T047 [US4] Perform final build of the entire solution: `dotnet build MCPDemo.sln`
- [X] T048 [US4] Final Project Walkthrough and hand-off to USER

**Checkpoint**: Feature complete. 100% test pass rate with coverage verification.
 builds cleanly.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **US1: Application Tests (Phase 2)**: Depends on Setup — can start after T005
- **US2: Infrastructure Tests (Phase 3)**: Depends on Setup — can start after T005
- **US3: Integration Tests (Phase 4)**: Depends on Setup — can start after T005 (independent of US1/US2)
- **Polish (Phase 5)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 (Setup) → US1 (Application Tests, P1 MVP)
                 → US2 (Infrastructure Tests, P1)   [parallel with US1]
                 → US3 (Integration Tests, P2)       [parallel with US1/US2]
                 → Phase 5 (Polish)
```

- **US1 (Application Tests)**: MVP — implement first. Highest value coverage.
- **US2 (Infrastructure Tests)**: Can run in parallel with US1 (different test project, different files).
- **US3 (Integration Tests)**: Can run in parallel but requires external dependencies (network, Docker).

### Parallel Opportunities

**Within Phase 1**: T001, T002, T003, T004 can all run in parallel (different directories/files)

**Within Phase 2 (US1)**: T017 (`CategoryServiceTests.cs`) and T021 (`SearchServiceTests.cs`) can start in parallel with T006 (`ProductServiceTests.cs`) since they are different files

**Within Phase 3 (US2)**: T023 (`PythonSandboxServiceTests.cs`), T029 (`InMemoryMetricsCollectorTests.cs`), T033 (`PlatziStoreApiClientTests.cs`) can all start in parallel — different files, no dependencies

**Within Phase 4 (US3)**: T038 (`ServiceIntegrationTests.cs`) and T040 (`SandboxIntegrationTests.cs`) can run in parallel — different files

**Cross-phase**: US1 and US2 can be implemented entirely in parallel since they target different test projects

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (create directories, verify project refs)
2. Complete Phase 2: US1 — Application service unit tests
3. **STOP and VALIDATE**: Run `dotnet test tests/MCPDemo.Application.Tests/` — ≥30 tests pass
4. Core business logic is now covered — this is the MVP

### Incremental Delivery

1. Setup → test projects ready (no test files yet)
2. US1 (Application Tests) → ≥30 unit tests covering all service methods (MVP!)
3. US2 (Infrastructure Tests) → ≥15 unit tests covering sandbox, metrics, API client
4. US3 (Integration Tests) → ≥5 integration tests verifying full pipeline
5. Polish → ≥50 total tests, ≥80% coverage, solution builds cleanly

---

## Notes

- All test files follow the existing Clean Architecture layer separation
- Test naming convention: `MethodName_StateUnderTest_ExpectedBehavior`
- NSubstitute is used for all interface mocking; FluentAssertions for assertions
- Integration tests use `[Trait("Category", "Integration")]` or `[Trait("Category", "Docker")]` for filtering
- Total tasks: 48
