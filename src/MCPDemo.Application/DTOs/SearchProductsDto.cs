/// <summary>
/// Combined filtering criteria for product search.
/// </summary>
namespace MCPDemo.Application.DTOs;

public record SearchProductsDto(
    string? Title = null,
    decimal? Price = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    int? CategoryId = null,
    string? CategorySlug = null,
    int? Offset = null,
    int? Limit = null);
