using System.Net;
using System.Text.Json;
using FluentAssertions;
using MCPDemo.Infrastructure.ExternalApi;
using MCPDemo.Domain.Entities;
using MCPDemo.Domain.Exceptions;
using MCPDemo.Shared.Exceptions;
using Xunit;

namespace MCPDemo.Infrastructure.Tests.ExternalApi;

public class PlatziStoreApiClientTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> SendAsyncAction { get; set; } = null!;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsyncAction(request, cancellationToken);
        }
    }

    private readonly MockHttpMessageHandler _handler;
    private readonly HttpClient _httpClient;
    private readonly Microsoft.Extensions.Logging.ILogger<PlatziStoreApiClient> _logger;
    private readonly PlatziStoreApiClient _sut;

    public PlatziStoreApiClientTests()
    {
        _handler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://api.escuelajs.co/api/v1/") };
        _logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<PlatziStoreApiClient>>();
        _sut = new PlatziStoreApiClient(_httpClient, _logger);
    }

    [Fact]
    public async Task GetAllProductsAsync_ValidResponse_ReturnsProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Title = "Product 1", Price = 10 }
        };
        var json = JsonSerializer.Serialize(products, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _handler.SendAsyncAction = (req, ct) => Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Product 1");
    }

    [Fact]
    public async Task GetProductByIdAsync_ValidResponse_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Title = "Product 1", Price = 10 };
        var json = JsonSerializer.Serialize(product, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _handler.SendAsyncAction = (req, ct) => Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        // Act
        var result = await _sut.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Product 1");
    }

    [Fact]
    public async Task GetProductByIdAsync_NotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        _handler.SendAsyncAction = (req, ct) => Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        });

        // Act
        var act = () => _sut.GetProductByIdAsync(1);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetAllProductsAsync_ServerError_ThrowsExternalApiException()
    {
        // Arrange
        _handler.SendAsyncAction = (req, ct) => Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        });

        // Act
        var act = () => _sut.GetAllProductsAsync();

        // Assert
        await act.Should().ThrowAsync<ExternalApiException>();
    }
}
