using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.DTOs.Products;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Models;
using MCPDemo.Domain.Entities;
using MCPDemo.Domain.Exceptions;
using MCPDemo.Shared.Exceptions;
using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Services;

/// <summary>
/// Implementation of IProductService that orchestrates product operations.
/// Validates input before delegating to the API client and wraps results in Result<T>.
/// </summary>
public class ProductService : IProductService
{
    private readonly IPlatziStoreApiClient _apiClient;
    private readonly ILogger<ProductService> _logger;
    private readonly IMetricsCollector _metrics;

    public ProductService(IPlatziStoreApiClient apiClient, ILogger<ProductService> logger, IMetricsCollector metrics)
    {
        _apiClient = apiClient;
        _logger = logger;
        _metrics = metrics;
    }

    private async Task<Result<T>> ExecuteWithMetricsAsync<T>(string toolName, Func<Task<T>> action)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName}", toolName);
            var result = await action();
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed with {ErrorType}", toolName, errorType);
            return Result<T>.Failure(ex.Message);
        }
    }

    public Task<Result<IEnumerable<Product>>> GetAllAsync(int? offset = null, int? limit = null)
    {
        return ExecuteWithMetricsAsync(nameof(GetAllAsync), () => _apiClient.GetAllProductsAsync(offset, limit));
    }

    public Task<Result<Product>> GetByIdAsync(int id)
    {
        return ExecuteWithMetricsAsync(nameof(GetByIdAsync), () => _apiClient.GetProductByIdAsync(id));
    }

    public Task<Result<Product>> GetBySlugAsync(string slug)
    {
        return ExecuteWithMetricsAsync(nameof(GetBySlugAsync), () => _apiClient.GetProductBySlugAsync(slug));
    }

    public async Task<Result<Product>> CreateAsync(CreateProductDto dto)
    {
        var sw = Stopwatch.StartNew();
        const string toolName = nameof(CreateAsync);

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result<Product>.Failure("Product title is required");
        }

        if (dto.Price < 0)
        {
            return Result<Product>.Failure("Product price must be non-negative");
        }

        if (dto.CategoryId <= 0)
        {
            return Result<Product>.Failure("Valid category ID is required");
        }

        if (dto.Images == null || !dto.Images.Any())
        {
            return Result<Product>.Failure("At least one image URL is required");
        }

        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName} for {ProductTitle}", toolName, dto.Title);
            var product = await _apiClient.CreateProductAsync(dto);
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed for {ProductTitle}", toolName, dto.Title);
            return Result<Product>.Failure(ex.Message);
        }
    }

    public async Task<Result<Product>> UpdateAsync(int id, UpdateProductDto dto)
    {
        var sw = Stopwatch.StartNew();
        const string toolName = nameof(UpdateAsync);

        if (dto.Title == null &&
            dto.Price == null &&
            dto.Description == null &&
            dto.CategoryId == null &&
            dto.Images == null)
        {
            return Result<Product>.Failure("No fields to update");
        }

        if (dto.Price.HasValue && dto.Price.Value < 0)
        {
            return Result<Product>.Failure("Product price must be non-negative");
        }

        if (dto.CategoryId.HasValue && dto.CategoryId.Value <= 0)
        {
            return Result<Product>.Failure("Valid category ID is required");
        }

        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName} for Product ID {ProductId}", toolName, id);
            var product = await _apiClient.UpdateProductAsync(id, dto);
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed for Product ID {ProductId}", toolName, id);
            return Result<Product>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var sw = Stopwatch.StartNew();
        const string toolName = nameof(DeleteAsync);
        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName} for Product ID {ProductId}", toolName, id);
            var result = await _apiClient.DeleteProductAsync(id);
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed for Product ID {ProductId}", toolName, id);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public Task<Result<IEnumerable<Product>>> GetRelatedByIdAsync(int id)
    {
        return ExecuteWithMetricsAsync(nameof(GetRelatedByIdAsync), () => _apiClient.GetRelatedProductsByIdAsync(id));
    }

    public Task<Result<IEnumerable<Product>>> GetRelatedBySlugAsync(string slug)
    {
        return ExecuteWithMetricsAsync(nameof(GetRelatedBySlugAsync), () => _apiClient.GetRelatedProductsBySlugAsync(slug));
    }
}
