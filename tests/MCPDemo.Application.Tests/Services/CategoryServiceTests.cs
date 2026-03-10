using FluentAssertions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.DTOs.Categories;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Models;
using MCPDemo.Application.Services;
using MCPDemo.Domain.Entities;
using MCPDemo.Shared.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MCPDemo.Application.Tests.Services;

public class CategoryServiceTests
{
    private readonly IPlatziStoreApiClient _apiClient;
    private readonly ILogger<CategoryService> _logger;
    private readonly IMetricsCollector _metrics;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _apiClient = Substitute.For<IPlatziStoreApiClient>();
        _logger = Substitute.For<ILogger<CategoryService>>();
        _metrics = Substitute.For<IMetricsCollector>();
        _sut = new CategoryService(_apiClient, _logger, _metrics);
    }

    private Category CreateValidCategory(int id = 1, string name = "Test Category") => new()
    {
        Id = id,
        Name = name,
        Image = "https://example.com/image.jpg"
    };

    [Fact]
    public async Task GetAllAsync_ReturnsSuccess()
    {
        // Arrange
        var categories = new[] { CreateValidCategory(1), CreateValidCategory(2) };
        _apiClient.GetAllCategoriesAsync().Returns(categories);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(categories);
        _metrics.Received(1).RecordExecution(nameof(_sut.GetAllAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var category = CreateValidCategory(1);
        _apiClient.GetCategoryByIdAsync(1).Returns(category);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task GetBySlugAsync_ValidSlug_ReturnsSuccess()
    {
        // Arrange
        var category = CreateValidCategory(1);
        _apiClient.GetCategoryBySlugAsync("slug").Returns(category);

        // Act
        var result = await _sut.GetBySlugAsync("slug");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateCategoryDto("", "url");

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Category name is required");
    }

    [Fact]
    public async Task CreateAsync_EmptyImage_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateCategoryDto("Category", "");

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Category image URL is required");
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateCategoryDto("Category", "url");
        var category = CreateValidCategory(1, "Category");
        _apiClient.CreateCategoryAsync(dto).Returns(category);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(category);
        _metrics.Received(1).RecordExecution(nameof(_sut.CreateAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task UpdateAsync_AllNullFields_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateCategoryDto();

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No fields to update");
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new UpdateCategoryDto(Name: "Updated");
        var category = CreateValidCategory(1, "Updated");
        _apiClient.UpdateCategoryAsync(1, dto).Returns(category);

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        _apiClient.DeleteCategoryAsync(1).Returns(true);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsAsync_ValidCategoryId_ReturnsSuccess()
    {
        // Arrange
        var products = new[] { new Product { Id = 1, Title = "P1" } };
        _apiClient.GetProductsByCategoryAsync(1).Returns(products);

        // Act
        var result = await _sut.GetProductsAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task CreateAsync_ApiThrows_RecordsFailureMetric()
    {
        // Arrange
        var dto = new CreateCategoryDto("Category", "url");
        _apiClient.CreateCategoryAsync(dto).Throws(new Exception("API Error"));

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _metrics.Received(1).RecordExecution(nameof(_sut.CreateAsync), Arg.Any<long>(), false, nameof(Exception));
    }
}
