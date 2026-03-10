using FluentAssertions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.DTOs;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Models;
using MCPDemo.Application.Services;
using MCPDemo.Domain.Entities;
using MCPDemo.Shared.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MCPDemo.Application.Tests.Services;

public class SearchServiceTests
{
    private readonly IPlatziStoreApiClient _apiClient;
    private readonly ILogger<SearchService> _logger;
    private readonly IMetricsCollector _metrics;
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _apiClient = Substitute.For<IPlatziStoreApiClient>();
        _logger = Substitute.For<ILogger<SearchService>>();
        _metrics = Substitute.For<IMetricsCollector>();
        _sut = new SearchService(_apiClient, _logger, _metrics);
    }

    [Fact]
    public async Task SearchProductsAsync_ValidFilters_ReturnsSuccess()
    {
        // Arrange
        var filters = new SearchProductsDto(Title: "Test");
        var products = new[] { new Product { Id = 1, Title = "Test Product" } };
        _apiClient.SearchProductsAsync(filters).Returns(products);

        // Act
        var result = await _sut.SearchProductsAsync(filters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(products);
        _metrics.Received(1).RecordExecution(nameof(_sut.SearchProductsAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task SearchProductsAsync_ApiThrows_ReturnsFailure()
    {
        // Arrange
        var filters = new SearchProductsDto(Title: "Test");
        _apiClient.SearchProductsAsync(filters).Throws(new Exception("API Error"));

        // Act
        var result = await _sut.SearchProductsAsync(filters);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("API Error");
        _metrics.Received(1).RecordExecution(nameof(_sut.SearchProductsAsync), Arg.Any<long>(), false, nameof(Exception));
    }
}
