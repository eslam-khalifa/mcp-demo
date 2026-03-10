using System.ComponentModel;
using System.Text.Json;
using MCPDemo.Application.DTOs.Products;
using MCPDemo.Application.Interfaces;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace MCPDemo.Api.McpTools;

/// <summary>
/// Exposes product operations as discoverable MCP tools for the AI assistant.
/// These tools are thin wrappers over the <see cref="IProductService"/>.
/// </summary>
[McpServerToolType]
public static class ProductTools
{
    /// <summary>
    /// List all products from the store with optional pagination.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="offset">Optional number of items to skip for pagination.</param>
    /// <param name="limit">Optional maximum number of items to return.</param>
    /// <returns>A JSON array of products if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("List all products from the store with optional pagination. Returns a JSON array (Product).")]
    public static async Task<string> get_all_products(
        IProductService productService,
        [Description("The number of items to skip for pagination. Integer. Optional. Default is 0.")] int? offset = null,
        [Description("The maximum number of items to return. Integer. Optional. Default is 10.")] int? limit = null)
    {
        var result = await productService.GetAllAsync(offset, limit);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Get a single product by its unique numeric ID.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="id">The product ID.</param>
    /// <returns>A product JSON object if found; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Get a single product by its unique numeric ID. Returns a JSON object (Product).")]
    public static async Task<string> get_product_by_id(
        IProductService productService,
        [Description("The unique numeric ID of the product. Integer. Required.")] int id)
    {
        var result = await productService.GetByIdAsync(id);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Get a single product by its unique URL-friendly slug.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="slug">The product slug.</param>
    /// <returns>A product JSON object if found; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Get a single product by its unique URL-friendly slug. Returns a JSON object (Product).")]
    public static async Task<string> get_product_by_slug(
        IProductService productService,
        [Description("The URL-friendly slug of the product. String. Required.")] string slug)
    {
        var result = await productService.GetBySlugAsync(slug);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Create a new product in the store.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="title">Title of the product.</param>
    /// <param name="price">Price of the product.</param>
    /// <param name="description">Description of the product.</param>
    /// <param name="categoryId">ID of the category this product belongs to.</param>
    /// <param name="images">Array of image URLs for the product.</param>
    /// <returns>The created product JSON object if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Create a new product in the store. Returns a JSON object (Product).")]
    public static async Task<string> create_product(
        IProductService productService,
        [Description("The title of the product. String. Required.")] string title,
        [Description("The price of the product. Decimal. Required. Must be positive.")] decimal price,
        [Description("The HTML or plain text description. String. Required.")] string description,
        [Description("The ID of the category this product belongs to. Integer. Required.")] int categoryId,
        [Description("An array of image URLs for the product. Array of Strings. Required. Must contain at least one URL.")] string[] images)
    {
        var dto = new CreateProductDto(title, price, description, categoryId, images.ToList());

        var result = await productService.CreateAsync(dto);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Update an existing product. Only provide fields that need to be changed.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="id">The numeric ID of the product to update.</param>
    /// <param name="title">Optional new title.</param>
    /// <param name="price">Optional new price.</param>
    /// <param name="description">Optional new description.</param>
    /// <param name="categoryId">Optional new category ID.</param>
    /// <param name="images">Optional new array of image URLs.</param>
    /// <returns>The updated product JSON object if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Update an existing product. Only provide fields that need to be changed. Returns a JSON object (Product).")]
    public static async Task<string> update_product(
        IProductService productService,
        [Description("The numeric ID of the product to update. Integer. Required.")] int id,
        [Description("The new title. String. Optional.")] string? title = null,
        [Description("The new price. Decimal. Optional. Must be positive.")] decimal? price = null,
        [Description("The new description. String. Optional.")] string? description = null,
        [Description("The new category ID. Integer. Optional.")] int? categoryId = null,
        [Description("An array of new image URLs. Array of Strings. Optional.")] string[]? images = null)
    {
        var dto = new UpdateProductDto(title, price, description, categoryId, images?.ToList());

        var result = await productService.UpdateAsync(id, dto);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Delete a product by its ID.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="id">The numeric ID of the product to delete.</param>
    /// <returns>A success message if deleted; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Delete a product by its unique numeric ID. Returns a plain text success message.")]
    public static async Task<string> delete_product(
        IProductService productService,
        [Description("The unique numeric ID of the product to delete. Integer. Required.")] int id)
    {
        var result = await productService.DeleteAsync(id);
        return result.IsSuccess 
            ? "Product successfully deleted." 
            : result.Error!;
    }

    /// <summary>
    /// Get products related to a specific product by its ID.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="id">The primary product ID.</param>
    /// <returns>A JSON array of related products if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Get products related to a specific product by its numeric ID. Returns a JSON array (Product).")]
    public static async Task<string> get_related_products_by_id(
        IProductService productService,
        [Description("The numeric ID of the reference product. Integer. Required.")] int id)
    {
        var result = await productService.GetRelatedByIdAsync(id);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }

    /// <summary>
    /// Get products related to a specific product by its slug.
    /// </summary>
    /// <param name="productService">The product service (injected).</param>
    /// <param name="slug">The primary product slug.</param>
    /// <returns>A JSON array of related products if successful; otherwise, an error message.</returns>
    [McpServerTool]
    [Description("Get products related to a specific product by its URL-friendly slug. Returns a JSON array (Product).")]
    public static async Task<string> get_related_products_by_slug(
        IProductService productService,
        [Description("The URL-friendly slug of the reference product. String. Required.")] string slug)
    {
        var result = await productService.GetRelatedBySlugAsync(slug);
        return result.IsSuccess 
            ? JsonSerializer.Serialize(result.Value, ToolJsonOptions.Default) 
            : result.Error!;
    }
}
