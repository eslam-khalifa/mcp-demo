using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.DTOs.Categories;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Models;
using MCPDemo.Domain.Entities;
using MCPDemo.Domain.Exceptions;
using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Services;

/// <summary>
/// Implementation of ICategoryService that orchestrates category operations.
/// Validates input before delegating to the API client and wraps results in Result<T>.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IPlatziStoreApiClient _apiClient;
    private readonly ILogger<CategoryService> _logger;
    private readonly IMetricsCollector _metrics;

    public CategoryService(IPlatziStoreApiClient apiClient, ILogger<CategoryService> logger, IMetricsCollector metrics)
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

    public Task<Result<IEnumerable<Category>>> GetAllAsync()
    {
        return ExecuteWithMetricsAsync(nameof(GetAllAsync), () => _apiClient.GetAllCategoriesAsync());
    }

    public Task<Result<Category>> GetByIdAsync(int id)
    {
        return ExecuteWithMetricsAsync(nameof(GetByIdAsync), () => _apiClient.GetCategoryByIdAsync(id));
    }

    public Task<Result<Category>> GetBySlugAsync(string slug)
    {
        return ExecuteWithMetricsAsync(nameof(GetBySlugAsync), () => _apiClient.GetCategoryBySlugAsync(slug));
    }

    public async Task<Result<Category>> CreateAsync(CreateCategoryDto dto)
    {
        var sw = Stopwatch.StartNew();
        const string toolName = nameof(CreateAsync);

        if (string.IsNullOrWhiteSpace(dto.Name)) return Result<Category>.Failure("Category name is required");
        if (string.IsNullOrWhiteSpace(dto.Image)) return Result<Category>.Failure("Category image URL is required");

        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName} for {CategoryName}", toolName, dto.Name);
            var category = await _apiClient.CreateCategoryAsync(dto);
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<Category>.Success(category);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed for {CategoryName}", toolName, dto.Name);
            return Result<Category>.Failure(ex.Message);
        }
    }

    public async Task<Result<Category>> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var sw = Stopwatch.StartNew();
        const string toolName = nameof(UpdateAsync);

        if (dto.Name == null && dto.Image == null) return Result<Category>.Failure("No fields to update");

        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName} for Category ID {CategoryId}", toolName, id);
            var category = await _apiClient.UpdateCategoryAsync(id, dto);
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<Category>.Success(category);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed for Category ID {CategoryId}", toolName, id);
            return Result<Category>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var sw = Stopwatch.StartNew();
        const string toolName = nameof(DeleteAsync);
        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName} for Category ID {CategoryId}", toolName, id);
            var result = await _apiClient.DeleteCategoryAsync(id);
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed for Category ID {CategoryId}", toolName, id);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public Task<Result<IEnumerable<Product>>> GetProductsAsync(int categoryId)
    {
        return ExecuteWithMetricsAsync(nameof(GetProductsAsync), () => _apiClient.GetProductsByCategoryAsync(categoryId));
    }
}
