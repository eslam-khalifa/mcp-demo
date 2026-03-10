using FluentAssertions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.DTOs.Products;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Models;
using MCPDemo.Application.Services;
using MCPDemo.Domain.Entities;
using MCPDemo.Shared.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MCPDemo.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly IPlatziStoreApiClient _apiClient;
    private readonly ILogger<ProductService> _logger;
    private readonly IMetricsCollector _metrics;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _apiClient = Substitute.For<IPlatziStoreApiClient>();
        _logger = Substitute.For<ILogger<ProductService>>();
        _metrics = Substitute.For<IMetricsCollector>();
        _sut = new ProductService(_apiClient, _logger, _metrics);
    }

    private Product CreateValidProduct(int id = 1, string title = "Test Product") => new()
    {
        Id = id,
        Title = title,
        Price = 100,
        Description = "Description",
        Category = new Category { Id = 1, Name = "Category" },
        Images = new List<string> { "https://example.com/image.jpg" }
    };

    [Fact]
    public async Task GetAllAsync_ApiReturnsProducts_ReturnsSuccessWithProducts()
    {
        // Arrange
        var products = new[] { CreateValidProduct(1), CreateValidProduct(2) };
        _apiClient.GetAllProductsAsync(Arg.Any<int?>(), Arg.Any<int?>()).Returns(products);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(products);
        _metrics.Received(1).RecordExecution(nameof(_sut.GetAllAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsSuccessWithProduct()
    {
        // Arrange
        var product = CreateValidProduct(1);
        _apiClient.GetProductByIdAsync(1).Returns(product);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(product);
        _metrics.Received(1).RecordExecution(nameof(_sut.GetByIdAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task GetByIdAsync_ApiThrows_ReturnsFailureWithMessage()
    {
        // Arrange
        _apiClient.GetProductByIdAsync(1).Throws(new Exception("API Error"));

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("API Error");
        _metrics.Received(1).RecordExecution(nameof(_sut.GetByIdAsync), Arg.Any<long>(), false, nameof(Exception));
    }

    [Fact]
    public async Task GetBySlugAsync_ValidSlug_ReturnsSuccessWithProduct()
    {
        // Arrange
        var product = CreateValidProduct(1);
        _apiClient.GetProductBySlugAsync("test-slug").Returns(product);

        // Act
        var result = await _sut.GetBySlugAsync("test-slug");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(product);
        _metrics.Received(1).RecordExecution(nameof(_sut.GetBySlugAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateProductDto("", 100, "Desc", 1, new List<string> { "url" });

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product title is required");
        await _apiClient.DidNotReceive().CreateProductAsync(Arg.Any<CreateProductDto>());
    }

    [Fact]
    public async Task CreateAsync_NegativePrice_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateProductDto("Title", -1, "Desc", 1, new List<string> { "url" });

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product price must be non-negative");
    }

    [Fact]
    public async Task CreateAsync_InvalidCategoryId_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateProductDto("Title", 100, "Desc", 0, new List<string> { "url" });

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Valid category ID is required");
    }

    [Fact]
    public async Task CreateAsync_NoImages_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateProductDto("Title", 100, "Desc", 1, new List<string>());

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("At least one image URL is required");
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccessWithProduct()
    {
        // Arrange
        var dto = new CreateProductDto("Title", 100, "Desc", 1, new List<string> { "url" });
        var product = CreateValidProduct(1, "Title");
        _apiClient.CreateProductAsync(dto).Returns(product);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(product);
        _metrics.Received(1).RecordExecution(nameof(_sut.CreateAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task UpdateAsync_AllNullFields_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateProductDto();

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No fields to update");
    }

    [Fact]
    public async Task UpdateAsync_NegativePrice_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateProductDto(Price: -1);

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product price must be non-negative");
    }

    [Fact]
    public async Task UpdateAsync_InvalidCategoryId_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateProductDto(CategoryId: 0);

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Valid category ID is required");
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsSuccessWithProduct()
    {
        // Arrange
        var dto = new UpdateProductDto(Title: "Updated");
        var product = CreateValidProduct(1, "Updated");
        _apiClient.UpdateProductAsync(1, dto).Returns(product);

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(product);
        _metrics.Received(1).RecordExecution(nameof(_sut.UpdateAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsSuccessTrue()
    {
        // Arrange
        _apiClient.DeleteProductAsync(1).Returns(true);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _metrics.Received(1).RecordExecution(nameof(_sut.DeleteAsync), Arg.Any<long>(), true);
    }

    [Fact]
    public async Task GetRelatedByIdAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var products = new[] { CreateValidProduct(2) };
        _apiClient.GetRelatedProductsByIdAsync(1).Returns(products);

        // Act
        var result = await _sut.GetRelatedByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task GetRelatedBySlugAsync_ValidSlug_ReturnsSuccess()
    {
        // Arrange
        var products = new[] { CreateValidProduct(2) };
        _apiClient.GetRelatedProductsBySlugAsync("slug").Returns(products);

        // Act
        var result = await _sut.GetRelatedBySlugAsync("slug");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(products);
    }
}
