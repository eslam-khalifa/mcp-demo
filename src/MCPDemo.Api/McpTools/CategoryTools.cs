using System.ComponentModel;
using System.Text.Json;
using MCPDemo.Application.DTOs.Categories;
using MCPDemo.Application.Interfaces;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace MCPDemo.Api.McpTools;

/// <summary>
/// Exposes category operations as discoverable MCP tools for the AI assistant.
/// These tools are thin wrappers over the <see cref="ICategoryService"/>.
/// </summary>
[McpServerToolType]
public static class CategoryTools
{
    /// <summary>
    /// List all product categories.
    /// </summary>
    /// <param name="categoryService">The category service (injected).</param>
    /// <returns>A JSON array of categories if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("List all product categories. Returns a JSON array (Category).")]
    public static async Task<string> get_all_categories(ICategoryService categoryService)
    {
        var result = await categoryService.GetAllAsync();
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Get a single category by its unique numeric ID.
    /// </summary>
    /// <param name="categoryService">The category service (injected).</param>
    /// <param name="id">The category ID.</param>
    /// <returns>A category JSON object if found; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Get a single category by its unique numeric ID. Returns a JSON object (Category).")]
    public static async Task<string> get_category_by_id(
        ICategoryService categoryService,
        [Description("The unique numeric ID of the category. Integer. Required.")] int id)
    {
        var result = await categoryService.GetByIdAsync(id);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Get a single category by its unique URL-friendly slug.
    /// </summary>
    /// <param name="categoryService">The category service (injected).</param>
    /// <param name="slug">The category slug.</param>
    /// <returns>A category JSON object if found; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Get a single category by its unique URL-friendly slug. Returns a JSON object (Category).")]
    public static async Task<string> get_category_by_slug(
        ICategoryService categoryService,
        [Description("The URL-friendly slug of the category. String. Required.")] string slug)
    {
        var result = await categoryService.GetBySlugAsync(slug);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Create a new category in the store.
    /// </summary>
    /// <param name="categoryService">The category service (injected).</param>
    /// <param name="name">The name of the category.</param>
    /// <param name="image">The image URL for the category.</param>
    /// <returns>The created category JSON object if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Create a new category in the store. Returns a JSON object (Category).")]
    public static async Task<string> create_category(
        ICategoryService categoryService,
        [Description("The name of the category. String. Required.")] string name,
        [Description("The image URL for the category. String. Required.")] string image)
    {
        var dto = new CreateCategoryDto(name, image);
        var result = await categoryService.CreateAsync(dto);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Update an existing category. Only provide fields that need to be changed.
    /// </summary>
    /// <param name="categoryService">The category service (injected).</param>
    /// <param name="id">The numeric ID of the category to update.</param>
    /// <param name="name">Optional new name.</param>
    /// <param name="image">Optional new image URL.</param>
    /// <returns>The updated category JSON object if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Update an existing category. Only provide fields that need to be changed. Returns a JSON object (Category).")]
    public static async Task<string> update_category(
        ICategoryService categoryService,
        [Description("The numeric ID of the category to update. Integer. Required.")] int id,
        [Description("The new name of the category. String. Optional.")] string? name = null,
        [Description("The new image URL for the category. String. Optional.")] string? image = null)
    {
        var dto = new UpdateCategoryDto(name, image);
        var result = await categoryService.UpdateAsync(id, dto);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Delete a category by its ID.
    /// </summary>
    /// <param name="categoryService">The category service (injected).</param>
    /// <param name="id">The numeric ID of the category to delete.</param>
    /// <returns>A success message if deleted; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Delete a category by its unique numeric ID. Returns a plain text success message.")]
    public static async Task<string> delete_category(
        ICategoryService categoryService,
        [Description("The unique numeric ID of the category to delete. Integer. Required.")] int id)
    {
        var result = await categoryService.DeleteAsync(id);
        return result.IsSuccess 
            ? "Category successfully deleted." 
            : result.Error!;
    }

    /// <summary>
    /// Get all products belonging to a specific category.
    /// </summary>
    /// <param name="categoryService">The category service (injected).</param>
    /// <param name="categoryId">The numeric ID of the category.</param>
    /// <returns>A JSON array of products if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Get all products belonging to a specific category. Returns a JSON array (Product).")]
    public static async Task<string> get_products_by_category(
        ICategoryService categoryService,
        [Description("The numeric ID of the category. Integer. Required.")] int categoryId)
    {
        var result = await categoryService.GetProductsAsync(categoryId);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }
}
