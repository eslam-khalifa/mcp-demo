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
    [Description("Search products with flexible filters (title, price, category, etc.).")]
    public static async Task<string> search_products(
        ISearchService searchService,
        [Description("The title to search for (optional)")] string? title = null,
        [Description("The exact price to filter by (optional)")] decimal? price = null,
        [Description("The minimum price (optional)")] decimal? priceMin = null,
        [Description("The maximum price (optional)")] decimal? priceMax = null,
        [Description("The numeric category ID (optional)")] int? categoryId = null,
        [Description("The URL-friendly category slug (optional)")] string? categorySlug = null,
        [Description("The number of items to skip (optional)")] int? offset = null,
        [Description("The maximum number of items to return (optional)")] int? limit = null)
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
