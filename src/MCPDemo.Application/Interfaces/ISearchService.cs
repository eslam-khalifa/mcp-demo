using MCPDemo.Application.DTOs;
using MCPDemo.Domain.Entities;
using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Interfaces;

public interface ISearchService
{
    Task<Result<IEnumerable<Product>>> SearchProductsAsync(SearchProductsDto filters);
}
