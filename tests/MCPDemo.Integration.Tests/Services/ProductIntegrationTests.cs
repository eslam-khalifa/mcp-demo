using FluentAssertions;
using Microsoft.Extensions.Logging;
using MCPDemo.Infrastructure.ExternalApi;
using NSubstitute;
using Xunit;

namespace MCPDemo.Integration.Tests.Services;

[Trait("Category", "Integration")]
public class ProductIntegrationTests
{
    private readonly PlatziStoreApiClient _apiClient;

    public ProductIntegrationTests()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.escuelajs.co/api/v1/") };
        var logger = Substitute.For<ILogger<PlatziStoreApiClient>>();
        _apiClient = new PlatziStoreApiClient(httpClient, logger);
    }

    [Fact]
    public async Task Integration_GetAllProducts_ReturnsRealData()
    {
        // Act
        var products = await _apiClient.GetAllProductsAsync(limit: 5);

        // Assert
        products.Should().NotBeEmpty();
        products.Count().Should().BeLessThanOrEqualTo(5);
        products.First().Title.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Integration_GetProductById_ReturnsValidProduct()
    {
        // Arrange - ID 1 is usually available on Platzi
        int testId = 1;

        // Act
        var product = await _apiClient.GetProductByIdAsync(testId);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().Be(testId);
        product.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Integration_SearchProducts_ReturnsFilteredResults()
    {
        // Arrange
        var products = await _apiClient.GetAllProductsAsync(limit: 1);
        var firstProductTitle = products.First().Title;
        var filters = new Application.DTOs.SearchProductsDto(Title: firstProductTitle);

        // Act
        var results = await _apiClient.SearchProductsAsync(filters);

        // Assert
        results.Should().NotBeEmpty();
        results.Any(p => p.Title.Contains(firstProductTitle, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }
}
