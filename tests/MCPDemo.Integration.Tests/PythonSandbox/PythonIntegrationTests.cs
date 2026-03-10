using FluentAssertions;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.Interfaces;
using MCPDemo.Infrastructure.PythonSandbox;
using NSubstitute;
using Xunit;

namespace MCPDemo.Integration.Tests.PythonSandbox;

[Trait("Category", "Docker")]
public class PythonIntegrationTests
{
    private readonly IPythonSandboxService _sut;

    public PythonIntegrationTests()
    {
        var processRunner = new DockerProcessRunner();
        var logger = Substitute.For<ILogger<PythonSandboxService>>();
        var metrics = Substitute.For<IMetricsCollector>();
        _sut = new PythonSandboxService(processRunner, logger, metrics);
    }

    [Fact]
    public async Task Integration_ExecuteHello_ReturnsCorrectOutput()
    {
        // Act
        var result = await _sut.ExecuteAsync("print('Hello from Integration Test')", null);

        // Assert
        result.Trim().Should().Be("Hello from Integration Test");
    }

    [Fact]
    public async Task Integration_ExecuteWithData_ReturnsProcessedJson()
    {
        // Arrange
        var code = "print(f'Input: {data}')";
        var jsonData = "{\"test\": \"value\"}";

        // Act
        var result = await _sut.ExecuteAsync(code, jsonData);

        // Assert
        result.Trim().Should().Contain("Input: {'test': 'value'}");
    }
}
