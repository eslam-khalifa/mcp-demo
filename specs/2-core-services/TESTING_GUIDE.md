# Testing Guide: Phase 2 — Core Services

This guide details how to test and verify the components implemented in Phase 2.

## 1. API Client Verification (`PlatziStoreApiClient`)

The API client is the most critical component. It handles communication with the Platzi Fake Store API, including retry logic and error mapping.

### Key Test Scenarios
- **Success Case**: Ensure JSON is deserialized to domain entities correctly.
- **404 Handling**: Verify that a `404 Not Found` from the API is translated into an `EntityNotFoundException`.
- **Retry Logic (5xx)**: Mock the `HttpClient` to return `500 Internal Server Error` and verify that the client retries exactly 2 times (3 total attempts) with exponential backoff.
- **Timeout Handling**: Verify that a `TaskCanceledException` triggers a retry.
- **Search Filters**: Verify that only non-null filters are included in the query string.

### Mocking Examples
Use `NSubstitute` to mock `IPlatziStoreApiClient` for service tests:
```csharp
var apiClient = Substitute.For<IPlatziStoreApiClient>();
apiClient.GetProductByIdAsync(1).Returns(new Product { Id = 1, Title = "Test" });
```

---

## 2. Service Layer Verification

Each service (`ProductService`, `CategoryService`, `SearchService`) should be tested for:
- **Input Validation**: Ensure invalid DTOs (e.g., negative prices, empty titles) return a `Result.Failure`.
- **Result Wrapping**: Verify that success/failure results are correctly wrapped in `Result<T>`.
- **Logging & Metrics**: Verify that `Stopwatch` is triggered and `IMetricsCollector` records the execution.

---

## 3. Verified Build
Run the following command to ensure the entire solution is healthy:
```bash
dotnet build MCPDemo.sln /p:TreatWarningsAsErrors=true
```
