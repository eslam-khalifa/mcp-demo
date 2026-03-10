# Research: Unit & Integration Testing

**Feature**: `5-testing`  
**Date**: 2026-03-10

## Research Summary

No significant unknowns were identified in the Technical Context. All technology choices (xUnit, NSubstitute, FluentAssertions) are already installed in the test projects and align with the implementation plan. This research documents best practices and patterns for the specific testing scenarios in this project.

---

## Decision 1: Mocking Strategy for Application Service Tests

**Decision**: Use NSubstitute to mock `IPlatziStoreApiClient`, `ILogger<T>`, and `IMetricsCollector` in all Application service unit tests.

**Rationale**: NSubstitute is already installed (v5.3.0) and provides a clean, readable syntax for both stubbing return values and verifying method calls. The `.Received()` pattern aligns well with FR-010 (verifying metrics recording).

**Alternatives considered**:
- Moq: Popular but NSubstitute is already chosen and installed.
- Hand-rolled fakes: Unnecessary overhead for simple interface mocking.

**Pattern**:
```csharp
// Arrange
var apiClient = Substitute.For<IPlatziStoreApiClient>();
var logger = Substitute.For<ILogger<ProductService>>();
var metrics = Substitute.For<IMetricsCollector>();
apiClient.GetProductByIdAsync(1).Returns(expectedProduct);

var sut = new ProductService(apiClient, logger, metrics);

// Act
var result = await sut.GetByIdAsync(1);

// Assert
result.IsSuccess.Should().BeTrue();
result.Value.Should().BeEquivalentTo(expectedProduct);
metrics.Received(1).RecordExecution(Arg.Any<string>(), Arg.Any<long>(), true, null);
```

---

## Decision 2: HTTP Mocking for PlatziStoreApiClient Tests

**Decision**: Use a custom `DelegatingHandler` subclass to mock HTTP responses in `PlatziStoreApiClient` tests, injected via `HttpClient` constructor.

**Rationale**: `PlatziStoreApiClient` depends on `HttpClient` (injected via `IHttpClientFactory` pattern). The cleanest way to test it is to create a mock `HttpMessageHandler` that returns predefined responses. This avoids network calls while testing the full serialization/deserialization pipeline.

**Alternatives considered**:
- WireMock.Net: Full HTTP mocking server — overkill for unit tests.
- Wrapping HttpClient in a custom interface: Unnecessary abstraction when handler mocking works.

**Pattern**:
```csharp
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}
```

---

## Decision 3: Thread Safety Testing for InMemoryMetricsCollector

**Decision**: Use `Task.WhenAll` with multiple concurrent `RecordExecution` calls to verify thread safety, then assert final metric totals match expected values.

**Rationale**: The `InMemoryMetricsCollector` uses `ConcurrentDictionary` and `Interlocked` operations. The test needs to verify that concurrent writes don't lose data. A "stress test" with 100+ concurrent tasks is the standard approach.

**Alternatives considered**:
- Thread.Sleep-based race condition tests: Fragile, timing-dependent.
- Single-threaded sequential tests only: Would miss concurrency bugs.

**Pattern**:
```csharp
var collector = new InMemoryMetricsCollector();
var tasks = Enumerable.Range(0, 100)
    .Select(_ => Task.Run(() => collector.RecordExecution("tool", 10, true)))
    .ToArray();

await Task.WhenAll(tasks);

var metrics = collector.GetMetrics("tool");
metrics.TotalCalls.Should().Be(100);
metrics.SuccessCount.Should().Be(100);
```

---

## Decision 4: Integration Test Isolation Strategy

**Decision**: Use `[Trait("Category", "Integration")]` and `[Trait("Category", "Docker")]` to separate tests by environment requirements. Unit test runs exclude these traits by default.

**Rationale**: Integration tests depend on external systems (Platzi API, Docker). They must be runnable independently and skippable in CI environments without those dependencies. xUnit traits provide native filtering via `dotnet test --filter`.

**Alternatives considered**:
- Separate test assemblies for integration: Already have `MCPDemo.Integration.Tests` project.
- Environment variable-based skipping: Less discoverable than traits.

**CI filter commands**:
```bash
# Unit tests only (fast, no external deps)
dotnet test --filter "Category!=Integration&Category!=Docker"

# Integration tests only
dotnet test --filter "Category=Integration"

# Docker tests only
dotnet test --filter "Category=Docker"

# All tests
dotnet test
```

---

## Decision 5: PythonSandboxService Docker Argument Verification

**Decision**: Capture the `arguments` string passed to `IDockerProcessRunner.RunAsync` via NSubstitute's `Arg.Do<string>()` and assert it contains all required security flags.

**Rationale**: The Docker arguments are a critical security surface. Tests must verify that `--network=none`, `--read-only`, `--memory=256m`, `--cpus=0.5`, `--security-opt=no-new-privileges`, `--rm`, and `-i` are all present. String containment assertions with FluentAssertions provide clear failure messages.

**Pattern**:
```csharp
string capturedArgs = null;
processRunner.RunAsync(Arg.Do<string>(a => capturedArgs = a), Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(new ProcessResult(0, "42", ""));

await sut.ExecuteAsync("print(42)", null);

capturedArgs.Should().Contain("--network=none");
capturedArgs.Should().Contain("--read-only");
capturedArgs.Should().Contain("--memory=256m");
capturedArgs.Should().Contain("--cpus=0.5");
capturedArgs.Should().Contain("--security-opt=no-new-privileges");
capturedArgs.Should().Contain("--rm");
capturedArgs.Should().Contain("-i");
capturedArgs.Should().Contain("mcp-python-sandbox");
```
