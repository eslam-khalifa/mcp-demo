using MCPDemo.Application.DTOs.Categories;
using MCPDemo.Domain.Entities;
using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<IEnumerable<Category>>> GetAllAsync();
    Task<Result<Category>> GetByIdAsync(int id);
    Task<Result<Category>> GetBySlugAsync(string slug);
    Task<Result<Category>> CreateAsync(CreateCategoryDto dto);
    Task<Result<Category>> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<IEnumerable<Product>>> GetProductsAsync(int categoryId);
}
