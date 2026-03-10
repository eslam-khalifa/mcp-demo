using FluentAssertions;
using MCPDemo.Infrastructure.Metrics;
using Xunit;

namespace MCPDemo.Infrastructure.Tests.Metrics;

public class InMemoryMetricsCollectorTests
{
    private readonly InMemoryMetricsCollector _sut;

    public InMemoryMetricsCollectorTests()
    {
        _sut = new InMemoryMetricsCollector();
    }

    [Fact]
    public void RecordExecution_SingleSuccess_CorrectMetrics()
    {
        // Act
        _sut.RecordExecution("test_tool", 100, true);

        // Assert
        var metrics = _sut.GetMetrics("test_tool");
        metrics.TotalCalls.Should().Be(1);
        metrics.SuccessCount.Should().Be(1);
        metrics.AverageExecutionTimeMs.Should().Be(100);
    }

    [Fact]
    public void RecordExecution_MultipleSuccess_CorrectAverageTime()
    {
        // Act
        _sut.RecordExecution("test_tool", 100, true);
        _sut.RecordExecution("test_tool", 200, true);

        // Assert
        var metrics = _sut.GetMetrics("test_tool");
        metrics.TotalCalls.Should().Be(2);
        metrics.AverageExecutionTimeMs.Should().Be(150);
    }

    [Fact]
    public void RecordExecution_Failure_CorrectFailureCountAndErrorType()
    {
        // Act
        _sut.RecordExecution("test_tool", 50, false, "TestError");

        // Assert
        var metrics = _sut.GetMetrics("test_tool");
        metrics.TotalCalls.Should().Be(1);
        metrics.FailureCount.Should().Be(1);
        metrics.ErrorsByType.Should().ContainKey("TestError").WhoseValue.Should().Be(1);
    }

    [Fact]
    public void GetMetrics_UnknownTool_ReturnsZeroValueMetrics()
    {
        // Act
        var metrics = _sut.GetMetrics("ghost_tool");

        // Assert
        metrics.TotalCalls.Should().Be(0);
        metrics.SuccessCount.Should().Be(0);
    }

    [Fact]
    public async Task RecordExecution_ConcurrentCalls_NoDataLoss()
    {
        // Arrange
        int callCount = 1000;
        var tasks = new Task[callCount];

        // Act
        for (int i = 0; i < callCount; i++)
        {
            tasks[i] = Task.Run(() => _sut.RecordExecution("concurrent_tool", 10, true));
        }
        await Task.WhenAll(tasks);

        // Assert
        var metrics = _sut.GetMetrics("concurrent_tool");
        metrics.TotalCalls.Should().Be(callCount);
        metrics.SuccessCount.Should().Be(callCount);
    }

    [Fact]
    public void GetAllMetrics_MultipleTool_ReturnsAllEntries()
    {
        // Arrange
        _sut.RecordExecution("tool1", 10, true);
        _sut.RecordExecution("tool2", 20, true);

        // Act
        var allMetrics = _sut.GetAllMetrics();

        // Assert
        allMetrics.Should().HaveCount(2);
        allMetrics.Should().ContainKey("tool1");
        allMetrics.Should().ContainKey("tool2");
    }
}
