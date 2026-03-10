# Feature Specification: Unit & Integration Testing

**Feature**: `5-testing`  
**Created**: 2026-03-10  
**Status**: Draft  
**Input**: Phase 5 from implementation-plan.md — Write unit tests for Application services, Infrastructure components, and integration tests for MCP tool end-to-end flows.

## Assumptions

- Phases 1–4 are complete: Foundation, Core Services, MCP Server & C# Tools, and Python Sandbox are implemented and building successfully.
- Test projects (`MCPDemo.Application.Tests`, `MCPDemo.Infrastructure.Tests`, `MCPDemo.Integration.Tests`) already exist in `tests/` with the correct NuGet packages installed (xUnit, NSubstitute, FluentAssertions, coverlet.collector).
- No test source files exist yet — all test classes will be new.
- The `Result<T>` pattern is used by all Application services; tests must verify both `Success` and `Failure` results.
- The Python sandbox Docker image (`mcp-python-sandbox`) is built and available locally (required for integration tests).
- Integration tests that call the Platzi Fake Store API are network-dependent and should be tagged with `[Trait("Category", "Integration")]`.
- Integration tests that require Docker should be tagged with `[Trait("Category", "Docker")]`.
- The test naming convention follows `MethodName_StateUnderTest_ExpectedBehavior`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Application Service Unit Tests (Priority: P1)

A developer needs confidence that every Application-layer service (`ProductService`, `CategoryService`, `SearchService`) behaves correctly in isolation. Each service method should be tested for success paths, validation failures, and external error handling by mocking the `IPlatziStoreApiClient`, `ILogger`, and `IMetricsCollector` dependencies.

**Why this priority**: Application services contain the core business logic, input validation, and `Result<T>` wrapping. Bugs here directly affect every MCP tool. This is the highest-value test layer.

**Independent Test**: Run `dotnet test --filter "FullyQualifiedName~Application"` and verify all tests pass with zero failures. Each test can be demonstrated independently.

**Acceptance Scenarios**:

1. **Given** a `ProductService` with a mocked API client, **When** `GetByIdAsync` is called with a valid product ID and the API client returns a product, **Then** the result is `Success` containing the expected product.
2. **Given** a `ProductService` with a mocked API client, **When** the API client throws an exception during `GetByIdAsync`, **Then** the result is `Failure` with the exception message.
3. **Given** a `ProductService`, **When** `CreateAsync` is called with an empty title, **Then** the result is `Failure("Product title is required")` without calling the API client.
4. **Given** a `ProductService`, **When** `CreateAsync` is called with a negative price, **Then** the result is `Failure("Product price must be non-negative")`.
5. **Given** a `ProductService`, **When** `CreateAsync` is called with a zero or negative category ID, **Then** the result is `Failure("Valid category ID is required")`.
6. **Given** a `ProductService`, **When** `CreateAsync` is called with no images, **Then** the result is `Failure("At least one image URL is required")`.
7. **Given** a `ProductService`, **When** `UpdateAsync` is called with all-null fields, **Then** the result is `Failure("No fields to update")`.
8. **Given** a `CategoryService` with a mocked API client, **When** `GetAllAsync` is called and the API client returns categories, **Then** the result is `Success` containing all categories.
9. **Given** a `CategoryService`, **When** `CreateAsync` is called with an empty name, **Then** the result is `Failure("Category name is required")`.
10. **Given** a `CategoryService`, **When** `UpdateAsync` is called with all-null fields, **Then** the result is `Failure("No fields to update")`.
11. **Given** a `SearchService` with a mocked API client, **When** `SearchProductsAsync` is called with valid filters, **Then** the result is `Success` with matching products.
12. **Given** any service method that succeeds, **When** the method completes, **Then** a successful metric is recorded via `IMetricsCollector`.
13. **Given** any service method that fails with an exception, **When** the exception is caught, **Then** a failure metric is recorded via `IMetricsCollector` with the correct error type.

---

### User Story 2 — Infrastructure Unit Tests (Priority: P1)

A developer needs assurance that Infrastructure components (`PlatziStoreApiClient`, `PythonSandboxService`, `InMemoryMetricsCollector`) work correctly. The API client should correctly serialize/deserialize HTTP requests/responses and handle retries. The sandbox service should construct correct Docker arguments and handle timeouts. The metrics collector should be thread-safe and accurate.

**Why this priority**: Infrastructure is the boundary between the application and external systems. Bugs here cause silent data corruption, security bypasses, or runtime failures that are hard to diagnose.

**Independent Test**: Run `dotnet test --filter "FullyQualifiedName~Infrastructure"` and verify all tests pass.

**Acceptance Scenarios**:

1. **Given** a `PythonSandboxService` with a mocked `IDockerProcessRunner`, **When** `ExecuteAsync` is called with valid code, and the process returns exit code 0, **Then** the standard output is returned.
2. **Given** a `PythonSandboxService` with a mocked `IDockerProcessRunner`, **When** `ExecuteAsync` is called and the process returns a non-zero exit code, **Then** a `PythonSandboxException` is thrown with the error message.
3. **Given** a `PythonSandboxService` with a mocked `IDockerProcessRunner`, **When** `ExecuteAsync` is called and the cancellation token fires (timeout), **Then** a `PythonSandboxException` is thrown indicating a timeout.
4. **Given** a `PythonSandboxService`, **When** `ExecuteAsync` constructs the Docker command, **Then** the arguments include `--rm`, `-i`, `--memory=256m`, `--cpus=0.5`, `--network=none`, `--read-only`, `--security-opt=no-new-privileges`, and the image name `mcp-python-sandbox`.
5. **Given** an `InMemoryMetricsCollector`, **When** multiple successful executions are recorded for the same tool, **Then** `GetMetrics` returns the correct `TotalCalls`, `SuccessCount`, and `AverageExecutionTimeMs`.
6. **Given** an `InMemoryMetricsCollector`, **When** a failure with an error type is recorded, **Then** `GetMetrics` returns the correct `FailureCount` and `ErrorsByType` dictionary.
7. **Given** an `InMemoryMetricsCollector`, **When** metrics are recorded concurrently from multiple threads, **Then** no data is lost or corrupted (thread safety).
8. **Given** a `PlatziStoreApiClient` with a mocked `HttpMessageHandler`, **When** `GetAllProductsAsync` is called, **Then** the correct URL is constructed and products are deserialized correctly.
9. **Given** a `PlatziStoreApiClient` with a mocked `HttpMessageHandler`, **When** the API returns a 404, **Then** an appropriate exception is thrown.

---

### User Story 3 — Integration Tests (Priority: P2)

A developer needs to verify that the full MCP tool pipeline works end-to-end: from tool invocation through service logic, API client calls (to the real Platzi API), and Python sandbox execution (via Docker). These tests confirm that all layers integrate correctly.

**Why this priority**: Integration tests catch issues that unit tests miss — serialization mismatches, real API contract changes, Docker configuration errors — but they are slower, network-dependent, and require Docker. They are essential but secondary to unit tests.

**Independent Test**: Run `dotnet test --filter "Category=Integration"` and verify results. These tests require network access and Docker.

**Acceptance Scenarios**:

1. **Given** the full service stack is configured, **When** `ProductService.GetAllAsync()` is called against the real Platzi API, **Then** a non-empty list of products is returned.
2. **Given** the full service stack is configured, **When** `CategoryService.GetAllAsync()` is called against the real Platzi API, **Then** a non-empty list of categories is returned.
3. **Given** Docker is running and the sandbox image is built, **When** `PythonSandboxService.ExecuteAsync("print(42)", null)` is called, **Then** the output is `"42"`.
4. **Given** Docker is running and the sandbox image is built, **When** `PythonSandboxService.ExecuteAsync` is called with code that references `data` and a valid JSON data string, **Then** the code can access and process the data correctly.
5. **Given** Docker is running and the sandbox image is built, **When** `PythonSandboxService.ExecuteAsync` is called with code containing a syntax error, **Then** a `PythonSandboxException` is thrown with a descriptive error message.

---

### Edge Cases

- What happens when the Platzi API changes its response format? → Deserialization tests should catch contract changes via explicit property assertions.
- What happens if Docker is not installed or the sandbox image is not built? → Docker-dependent tests should be skipped gracefully using `[Trait("Category", "Docker")]` and a runtime environment check.
- What happens when `IMetricsCollector.RecordExecution` is called with a tool name that has never been seen before? → It should create a new entry automatically.
- What happens when a service method is called and the logger or metrics collector throws? → The service should not swallow these errors silently.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST have unit tests for every public method of `ProductService` (9 methods: `GetAllAsync`, `GetByIdAsync`, `GetBySlugAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetRelatedByIdAsync`, `GetRelatedBySlugAsync`, plus the validation paths in `CreateAsync`, `UpdateAsync`, and `DeleteAsync`).
- **FR-002**: System MUST have unit tests for every public method of `CategoryService` (8 methods: `GetAllAsync`, `GetByIdAsync`, `GetBySlugAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetProductsAsync`, plus validation paths).
- **FR-003**: System MUST have unit tests for `SearchService.SearchProductsAsync` covering success, failure, and metric recording.
- **FR-004**: System MUST have unit tests for `PythonSandboxService` covering success, non-zero exit code, timeout, unexpected exceptions, and correct Docker argument construction.
- **FR-005**: System MUST have unit tests for `InMemoryMetricsCollector` covering metric recording accuracy, retrieval, and thread safety.
- **FR-006**: System MUST have unit tests for `PlatziStoreApiClient` covering at least product list retrieval, single product retrieval, and error responses (404, 500), using a mocked `HttpMessageHandler`.
- **FR-007**: System MUST have integration tests for `ProductService` and `CategoryService` calling the real Platzi API, tagged with `[Trait("Category", "Integration")]`.
- **FR-008**: System MUST have integration tests for `PythonSandboxService` executing real Python code via Docker, tagged with `[Trait("Category", "Docker")]`.
- **FR-009**: All tests MUST follow the naming convention `MethodName_StateUnderTest_ExpectedBehavior`.
- **FR-010**: All Application service tests MUST verify that `IMetricsCollector.RecordExecution` is called with the correct arguments on both success and failure.
- **FR-011**: All tests MUST use NSubstitute for mocking interfaces and FluentAssertions for readable assertion syntax.
- **FR-012**: System MUST achieve a minimum of 80% code coverage across `MCPDemo.Application` and `MCPDemo.Infrastructure` projects (excluding auto-generated code).

### Key Entities

- **Test Fixture**: A class that groups related tests for a single service or component, providing shared setup (e.g., mocked dependencies) via the constructor.
- **Mock**: A substitute implementation of an interface (`IPlatziStoreApiClient`, `IDockerProcessRunner`, `ILogger`, `IMetricsCollector`) created via NSubstitute that allows controlling behavior and verifying interactions.
- **Test Trait**: A metadata tag (`[Trait("Category", "Integration")]` or `[Trait("Category", "Docker")]`) used to filter tests by execution environment requirements.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Running `dotnet test` produces at least 50 passing test cases across all three test projects.
- **SC-002**: All unit tests complete in under 10 seconds total (no network or Docker dependencies).
- **SC-003**: Code coverage for `MCPDemo.Application` services is at least 80%.
- **SC-004**: Code coverage for `MCPDemo.Infrastructure` components (sandbox service, metrics collector) is at least 80%.
- **SC-005**: Zero test failures when running `dotnet test --filter "Category!=Integration&Category!=Docker"` (unit tests only).
- **SC-006**: Integration tests with the real Platzi API pass when network is available.
- **SC-007**: Docker integration tests pass when Docker is running and the sandbox image is built.
- **SC-008**: The solution continues to build with zero errors and zero warnings after test files are added.
