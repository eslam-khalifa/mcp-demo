using MCPDemo.Application.DTOs;
using MCPDemo.Application.DTOs.Categories;
using MCPDemo.Application.DTOs.Products;
using MCPDemo.Domain.Entities;

namespace MCPDemo.Application.Interfaces;

/// <summary>
/// Abstraction for all Platzi Fake Store API communication.
/// Defined in Application layer; implemented in Infrastructure.
/// </summary>
public interface IPlatziStoreApiClient
{
    // ── Products ──────────────────────────────────────────────
    Task<IEnumerable<Product>> GetAllProductsAsync(int? offset = null, int? limit = null);
    Task<Product> GetProductByIdAsync(int id);
    Task<Product> GetProductBySlugAsync(string slug);
    Task<Product> CreateProductAsync(CreateProductDto dto);
    Task<Product> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
    Task<IEnumerable<Product>> GetRelatedProductsByIdAsync(int id);
    Task<IEnumerable<Product>> GetRelatedProductsBySlugAsync(string slug);

    // ── Categories ────────────────────────────────────────────
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category> GetCategoryByIdAsync(int id);
    Task<Category> GetCategoryBySlugAsync(string slug);
    Task<Category> CreateCategoryAsync(CreateCategoryDto dto);
    Task<Category> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

    // ── Search ────────────────────────────────────────────────
    Task<IEnumerable<Product>> SearchProductsAsync(SearchProductsDto filters);
}
