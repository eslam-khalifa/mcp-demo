using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.DTOs;
using MCPDemo.Application.DTOs.Categories;
using MCPDemo.Application.DTOs.Products;
using MCPDemo.Application.Interfaces;
using MCPDemo.Domain.Entities;
using MCPDemo.Domain.Exceptions;
using MCPDemo.Shared.Exceptions;

namespace MCPDemo.Infrastructure.ExternalApi;

public class PlatziStoreApiClient : IPlatziStoreApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlatziStoreApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PlatziStoreApiClient(HttpClient httpClient, ILogger<PlatziStoreApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task<T> SendWithRetryAsync<T>(Func<Task<HttpResponseMessage>> requestFunc, string entityType, string entityId)
    {
        int maxRetries = 2;
        int attempt = 0;

        while (true)
        {
            attempt++;
            try
            {
                var response = await requestFunc();

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                    return result ?? throw new ExternalApiException((int)response.StatusCode, string.Empty, "API returned empty payload");
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new EntityNotFoundException(entityType, entityId);
                }

                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new ExternalApiException((int)response.StatusCode, errorBody, $"API returned error: {response.StatusCode}");
                }

                if (attempt > maxRetries)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new ExternalApiException((int)response.StatusCode, errorBody, $"API failed after {maxRetries} retries: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException) when (attempt <= maxRetries)
            {
                _logger.LogWarning("Request for {EntityType} {EntityId} timed out. Attempt {Attempt} of {MaxRetries}.", entityType, entityId, attempt, maxRetries + 1);
            }
            catch (Exception ex) when (ex is not EntityNotFoundException && ex is not ExternalApiException && attempt <= maxRetries)
            {
                _logger.LogWarning(ex, "Request for {EntityType} {EntityId} failed. Attempt {Attempt} of {MaxRetries}.", entityType, entityId, attempt, maxRetries + 1);
            }

            await Task.Delay(500 * attempt);
        }
    }

    private async Task<bool> SendDeleteWithRetryAsync(Func<Task<HttpResponseMessage>> requestFunc, string entityType, string entityId)
    {
        int maxRetries = 2;
        int attempt = 0;

        while (true)
        {
            attempt++;
            try
            {
                var response = await requestFunc();

                if (response.IsSuccessStatusCode) return true;

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new EntityNotFoundException(entityType, entityId);
                }

                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new ExternalApiException((int)response.StatusCode, errorBody, $"API returned error: {response.StatusCode}");
                }

                if (attempt > maxRetries)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new ExternalApiException((int)response.StatusCode, errorBody, $"API failed after {maxRetries} retries: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException) when (attempt <= maxRetries)
            {
                _logger.LogWarning("Delete request for {EntityType} {EntityId} timed out. Attempt {Attempt}.", entityType, entityId, attempt);
            }
            catch (Exception ex) when (ex is not EntityNotFoundException && ex is not ExternalApiException && attempt <= maxRetries)
            {
                _logger.LogWarning(ex, "Delete request for {EntityType} {EntityId} failed. Attempt {Attempt}.", entityType, entityId, attempt);
            }

            await Task.Delay(500 * attempt);
        }
    }

    // ── Products ──────────────────────────────────────────────

    public async Task<IEnumerable<Product>> GetAllProductsAsync(int? offset = null, int? limit = null)
    {
        var url = "products";
        var queryParams = new List<string>();
        
        // Platzi API requires both offset and limit for pagination to work correctly
        var actualOffset = offset ?? (limit.HasValue ? 0 : null);
        
        if (actualOffset.HasValue) queryParams.Add($"offset={actualOffset}");
        if (limit.HasValue) queryParams.Add($"limit={limit}");
        if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

        return await SendWithRetryAsync<IEnumerable<Product>>(() => _httpClient.GetAsync(url), "Product", "List");
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await SendWithRetryAsync<Product>(() => _httpClient.GetAsync($"products/{id}"), "Product", id.ToString());
    }

    public async Task<Product> GetProductBySlugAsync(string slug)
    {
        return await SendWithRetryAsync<Product>(() => _httpClient.GetAsync($"products/slug/{slug}"), "Product", slug);
    }

    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        return await SendWithRetryAsync<Product>(() => _httpClient.PostAsJsonAsync("products", dto, _jsonOptions), "Product", "New");
    }

    public async Task<Product> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        return await SendWithRetryAsync<Product>(() => _httpClient.PutAsJsonAsync($"products/{id}", dto, _jsonOptions), "Product", id.ToString());
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await SendDeleteWithRetryAsync(() => _httpClient.DeleteAsync($"products/{id}"), "Product", id.ToString());
    }

    public async Task<IEnumerable<Product>> GetRelatedProductsByIdAsync(int id)
    {
        return await SendWithRetryAsync<IEnumerable<Product>>(() => _httpClient.GetAsync($"products/{id}/related"), "Product", $"{id}/related");
    }

    public async Task<IEnumerable<Product>> GetRelatedProductsBySlugAsync(string slug)
    {
        return await SendWithRetryAsync<IEnumerable<Product>>(() => _httpClient.GetAsync($"products/slug/{slug}/related"), "Product", $"{slug}/related");
    }

    // ── Categories ────────────────────────────────────────────

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await SendWithRetryAsync<IEnumerable<Category>>(() => _httpClient.GetAsync("categories"), "Category", "List");
    }

    public async Task<Category> GetCategoryByIdAsync(int id)
    {
        return await SendWithRetryAsync<Category>(() => _httpClient.GetAsync($"categories/{id}"), "Category", id.ToString());
    }

    public async Task<Category> GetCategoryBySlugAsync(string slug)
    {
        return await SendWithRetryAsync<Category>(() => _httpClient.GetAsync($"categories/slug/{slug}"), "Category", slug);
    }

    public async Task<Category> CreateCategoryAsync(CreateCategoryDto dto)
    {
        return await SendWithRetryAsync<Category>(() => _httpClient.PostAsJsonAsync("categories", dto, _jsonOptions), "Category", "New");
    }

    public async Task<Category> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        return await SendWithRetryAsync<Category>(() => _httpClient.PutAsJsonAsync($"categories/{id}", dto, _jsonOptions), "Category", id.ToString());
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await SendDeleteWithRetryAsync(() => _httpClient.DeleteAsync($"categories/{id}"), "Category", id.ToString());
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await SendWithRetryAsync<IEnumerable<Product>>(() => _httpClient.GetAsync($"categories/{categoryId}/products"), "Category", $"{categoryId}/products");
    }

    // ── Search ────────────────────────────────────────────────

    public async Task<IEnumerable<Product>> SearchProductsAsync(SearchProductsDto filters)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(filters.Title)) queryParams.Add($"title={filters.Title}");
        if (filters.Price.HasValue) queryParams.Add($"price={filters.Price}");
        if (filters.PriceMin.HasValue) queryParams.Add($"price_min={filters.PriceMin}");
        if (filters.PriceMax.HasValue) queryParams.Add($"price_max={filters.PriceMax}");
        if (filters.CategoryId.HasValue) queryParams.Add($"categoryId={filters.CategoryId}");
        if (!string.IsNullOrWhiteSpace(filters.CategorySlug)) queryParams.Add($"categorySlug={filters.CategorySlug}");
        if (filters.Offset.HasValue) queryParams.Add($"offset={filters.Offset}");
        if (filters.Limit.HasValue) queryParams.Add($"limit={filters.Limit}");

        var url = "products";
        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        return await SendWithRetryAsync<IEnumerable<Product>>(() => _httpClient.GetAsync(url), "Search", "Filtered");
    }
}
