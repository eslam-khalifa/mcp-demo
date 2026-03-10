using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.Interfaces;
using MCPDemo.Infrastructure.PythonSandbox;
using MCPDemo.Shared.Exceptions;
using MCPDemo.Shared.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MCPDemo.Infrastructure.Tests.PythonSandbox;

public class PythonSandboxServiceTests
{
    private readonly IDockerProcessRunner _processRunner;
    private readonly ILogger<PythonSandboxService> _logger;
    private readonly IMetricsCollector _metrics;
    private readonly PythonSandboxService _sut;

    public PythonSandboxServiceTests()
    {
        _processRunner = Substitute.For<IDockerProcessRunner>();
        _logger = Substitute.For<ILogger<PythonSandboxService>>();
        _metrics = Substitute.For<IMetricsCollector>();
        _sut = new PythonSandboxService(_processRunner, _logger, _metrics);
    }

    [Fact]
    public async Task ExecuteAsync_ValidCode_ReturnsStdout()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, "42\n", ""));

        // Act
        var result = await _sut.ExecuteAsync("print(42)", null);

        // Assert
        result.Trim().Should().Be("42");
        _metrics.Received(1).RecordExecution("__PythonSandbox__", Arg.Any<long>(), true);
    }

    [Fact]
    public async Task ExecuteAsync_NonZeroExitCode_ThrowsPythonSandboxException()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(1, "", "SyntaxError: invalid syntax"));

        // Act
        var act = () => _sut.ExecuteAsync("invalid code", null);

        // Assert
        await act.Should().ThrowAsync<PythonSandboxException>()
            .WithMessage("*SyntaxError: invalid syntax*");
        _metrics.Received(1).RecordExecution("__PythonSandbox__", Arg.Any<long>(), false, "ExecutionError");
    }

    [Fact]
    public async Task ExecuteAsync_Timeout_ThrowsPythonSandboxException()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new OperationCanceledException());

        // Act
        var act = () => _sut.ExecuteAsync("slow code", null);

        // Assert
        await act.Should().ThrowAsync<PythonSandboxException>()
            .WithMessage("*timed out*");
        _metrics.Received(1).RecordExecution("__PythonSandbox__", Arg.Any<long>(), false, "Timeout");
    }

    [Fact]
    public async Task ExecuteAsync_UnexpectedException_ThrowsPythonSandboxException()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Docker error"));

        // Act
        var act = () => _sut.ExecuteAsync("print(1)", null);

        // Assert
        await act.Should().ThrowAsync<PythonSandboxException>()
            .WithMessage("*Docker error*");
        _metrics.Received(1).RecordExecution("__PythonSandbox__", Arg.Any<long>(), false, "InvalidOperationException");
    }

    [Fact]
    public async Task ExecuteAsync_DockerArguments_ContainAllSecurityFlags()
    {
        // Arrange
        string capturedArgs = null!;
        _processRunner.RunAsync(Arg.Do<string>(args => capturedArgs = args), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, "42", ""));

        // Act
        await _sut.ExecuteAsync("print(42)", "{}");

        // Assert
        capturedArgs.Should().Contain("--rm");
        capturedArgs.Should().Contain("-i");
        capturedArgs.Should().Contain("--memory=256m");
        capturedArgs.Should().Contain("--cpus=0.5");
        capturedArgs.Should().Contain("--network=none");
        capturedArgs.Should().Contain("--read-only");
        capturedArgs.Should().Contain("--security-opt=no-new-privileges");
        capturedArgs.Should().Contain("mcp-python-sandbox");
    }
}
