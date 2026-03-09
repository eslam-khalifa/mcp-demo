/// <summary>
/// DTOs for Product operations.
/// </summary>
namespace MCPDemo.Application.DTOs.Products;

public record CreateProductDto(
    string Title,
    decimal Price,
    string Description,
    int CategoryId,
    List<string> Images);

public record UpdateProductDto(
    string? Title = null,
    decimal? Price = null,
    string? Description = null,
    int? CategoryId = null,
    List<string>? Images = null);
