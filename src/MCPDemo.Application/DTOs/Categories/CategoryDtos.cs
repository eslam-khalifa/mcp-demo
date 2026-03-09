namespace MCPDemo.Application.DTOs.Categories;

public record CreateCategoryDto(
    string Name,
    string Image);

public record UpdateCategoryDto(
    string? Name = null,
    string? Image = null);
