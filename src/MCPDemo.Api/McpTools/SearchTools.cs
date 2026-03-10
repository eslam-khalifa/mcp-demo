using System.ComponentModel;
using System.Text.Json;
using MCPDemo.Application.DTOs;
using MCPDemo.Application.Interfaces;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace MCPDemo.Api.McpTools;

/// <summary>
/// Exposes search operations as discoverable MCP tools for the AI assistant.
/// These tools are thin wrappers over the <see cref="ISearchService"/>.
/// </summary>
[McpServerToolType]
public static class SearchTools
{
    /// <summary>
    /// Search products with flexible filters (title, price, category, etc.).
    /// </summary>
    /// <param name="searchService">The search service (injected).</param>
    /// <param name="title">Optional title search term.</param>
    /// <param name="price">Optional exact price filter.</param>
    /// <param name="priceMin">Optional minimum price filter.</param>
    /// <param name="priceMax">Optional maximum price filter.</param>
    /// <param name="categoryId">Optional category ID filter.</param>
    /// <param name="categorySlug">Optional category slug filter.</param>
    /// <param name="offset">Optional pagination offset.</param>
    /// <param name="limit">Optional pagination limit.</param>
    /// <returns>A JSON array of products if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Search products with flexible filters (title, price, category, etc.). Returns a JSON array (Product).")]
    public static async Task<string> search_products(
        ISearchService searchService,
        [Description("The product title to search for. String. Optional.")] string? title = null,
        [Description("Filter by exact price. Decimal. Optional.")] decimal? price = null,
        [Description("Filter by minimum price. Decimal. Optional.")] decimal? priceMin = null,
        [Description("Filter by maximum price. Decimal. Optional.")] decimal? priceMax = null,
        [Description("Filter by numeric category ID. Integer. Optional.")] int? categoryId = null,
        [Description("Filter by URL-friendly category slug. String. Optional.")] string? categorySlug = null,
        [Description("The number of items to skip for pagination. Integer. Optional. Default is 0.")] int? offset = null,
        [Description("The maximum number of items to return for pagination. Integer. Optional. Default is 10.")] int? limit = null)
    {
        var dto = new SearchProductsDto(
            Title: title,
            Price: price,
            PriceMin: priceMin,
            PriceMax: priceMax,
            CategoryId: categoryId,
            CategorySlug: categorySlug,
            Offset: offset,
            Limit: limit);

        var result = await searchService.SearchProductsAsync(dto);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }
}
