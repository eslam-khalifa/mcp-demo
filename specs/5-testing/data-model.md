# Data Model: Unit & Integration Testing

**Feature**: `5-testing`  
**Date**: 2026-03-10

## Test Fixture Patterns

This feature introduces no new domain entities or data models. Instead, it defines **test fixtures** — reusable arrangements of mocked dependencies used across test classes.

---

### ProductService Test Fixture

| Dependency | Type | Mock Strategy |
|------------|------|---------------|
| `IPlatziStoreApiClient` | Interface | NSubstitute — stub return values per test |
| `ILogger<ProductService>` | Interface | NSubstitute — no verification needed (logging is observable but not asserted) |
| `IMetricsCollector` | Interface | NSubstitute — `.Received()` verification on every test |

**Test Data Factories**:

| Factory Method | Returns | Purpose |
|----------------|---------|---------|
| `CreateValidProduct()` | `Product` | A fully populated product entity for success-path tests |
| `CreateValidCreateDto()` | `CreateProductDto` | Valid DTO with title, price, description, categoryId, images |
| `CreateValidUpdateDto()` | `UpdateProductDto` | DTO with at least one non-null field |

---

### CategoryService Test Fixture

| Dependency | Type | Mock Strategy |
|------------|------|---------------|
| `IPlatziStoreApiClient` | Interface | NSubstitute — stub return values per test |
| `ILogger<CategoryService>` | Interface | NSubstitute — no verification needed |
| `IMetricsCollector` | Interface | NSubstitute — `.Received()` verification |

**Test Data Factories**:

| Factory Method | Returns | Purpose |
|----------------|---------|---------|
| `CreateValidCategory()` | `Category` | A fully populated category entity |
| `CreateValidCreateDto()` | `CreateCategoryDto` | Valid DTO with name and image |
| `CreateValidUpdateDto()` | `UpdateCategoryDto` | DTO with at least one non-null field |

---

### SearchService Test Fixture

| Dependency | Type | Mock Strategy |
|------------|------|---------------|
| `IPlatziStoreApiClient` | Interface | NSubstitute — stub `SearchProductsAsync` |
| `ILogger<SearchService>` | Interface | NSubstitute — no verification needed |
| `IMetricsCollector` | Interface | NSubstitute — `.Received()` verification |

---

### PythonSandboxService Test Fixture

| Dependency | Type | Mock Strategy |
|------------|------|---------------|
| `IDockerProcessRunner` | Interface | NSubstitute — stub `RunAsync` with `ProcessResult` |
| `ILogger<PythonSandboxService>` | Interface | NSubstitute — no verification needed |
| `IMetricsCollector` | Interface | NSubstitute — `.Received()` verification |

**Key Test Scenarios**:

| Scenario | `ProcessResult` Setup | Expected Outcome |
|----------|----------------------|------------------|
| Success | `ExitCode=0, StdOut="42", StdErr=""` | Returns `"42"` |
| Execution error | `ExitCode=1, StdOut="", StdErr="error msg"` | Throws `PythonSandboxException` |
| Timeout | `RunAsync` throws `OperationCanceledException` | Throws `PythonSandboxException` with timeout message |
| Unexpected error | `RunAsync` throws `InvalidOperationException` | Throws `PythonSandboxException` wrapping original |

---

### InMemoryMetricsCollector (No Mocks)

Tested directly — no mocked dependencies. Uses real `ConcurrentDictionary` internals.

| Test Category | Input | Expected Output |
|---------------|-------|-----------------|
| Single tool recording | `RecordExecution("tool_a", 100, true)` | `TotalCalls=1, SuccessCount=1, AvgTime=100` |
| Multiple calls | 3× `RecordExecution("tool_a", ...)` | `TotalCalls=3, correct averages` |
| Failure recording | `RecordExecution("tool_a", 50, false, "TimeoutError")` | `FailureCount=1, ErrorsByType["TimeoutError"]=1` |
| Unknown tool query | `GetMetrics("nonexistent")` | Returns zero-value metrics |
| Thread safety | 100 concurrent `RecordExecution` calls | `TotalCalls=100` (no data loss) |

---

### PlatziStoreApiClient Test Fixture

| Dependency | Type | Mock Strategy |
|------------|------|---------------|
| `HttpMessageHandler` | Abstract class | Custom `MockHttpMessageHandler` — returns predefined `HttpResponseMessage` |

**Response Templates**:

| Scenario | HTTP Status | Response Body |
|----------|-------------|---------------|
| Product list | 200 | `[{"id":1,"title":"Laptop","price":999,...}]` |
| Single product | 200 | `{"id":1,"title":"Laptop","price":999,...}` |
| Not found | 404 | Error response body |
| Server error | 500 | Error response body |

---

## Integration Test Data

Integration tests use **real data** from the Platzi Fake Store API. No test data factories are needed — assertions verify structural properties (non-empty lists, valid IDs, etc.) rather than exact values.

### Docker Sandbox Integration

| Test Case | Python Code | Data Input | Expected Output Pattern |
|-----------|-------------|------------|------------------------|
| Simple execution | `print(42)` | `null` | `"42"` |
| Data processing | `print(len(data))` | `[1,2,3]` | `"3"` |
| Syntax error | `print(` | `null` | Exception with "SyntaxError" |
