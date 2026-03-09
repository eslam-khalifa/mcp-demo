using MCPDemo.Application.DTOs.Products;
using MCPDemo.Domain.Entities;
using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Interfaces;

public interface IProductService
{
    Task<Result<IEnumerable<Product>>> GetAllAsync(int? offset = null, int? limit = null);
    Task<Result<Product>> GetByIdAsync(int id);
    Task<Result<Product>> GetBySlugAsync(string slug);
    Task<Result<Product>> CreateAsync(CreateProductDto dto);
    Task<Result<Product>> UpdateAsync(int id, UpdateProductDto dto);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<IEnumerable<Product>>> GetRelatedByIdAsync(int id);
    Task<Result<IEnumerable<Product>>> GetRelatedBySlugAsync(string slug);
}
