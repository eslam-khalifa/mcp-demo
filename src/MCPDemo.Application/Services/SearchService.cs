using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MCPDemo.Application.DTOs;
using MCPDemo.Application.Interfaces;
using MCPDemo.Application.Models;
using MCPDemo.Domain.Entities;
using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Services;

/// <summary>
/// Implementation of ISearchService that orchestrates product search operations.
/// Delegates filtering criteria to the API client and wraps results in Result<T>.
/// </summary>
public class SearchService : ISearchService
{
    private readonly IPlatziStoreApiClient _apiClient;
    private readonly ILogger<SearchService> _logger;
    private readonly IMetricsCollector _metrics;

    public SearchService(IPlatziStoreApiClient apiClient, ILogger<SearchService> logger, IMetricsCollector metrics)
    {
        _apiClient = apiClient;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<Result<IEnumerable<Product>>> SearchProductsAsync(SearchProductsDto filters)
    {
        var sw = Stopwatch.StartNew();
        const string toolName = nameof(SearchProductsAsync);
        try
        {
            _logger.LogInformation("Executing MCP Tool: {ToolName} with custom filters", toolName);
            var products = await _apiClient.SearchProductsAsync(filters);
            sw.Stop();
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, true);
            return Result<IEnumerable<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var errorType = ex.GetType().Name;
            _metrics.RecordExecution(toolName, sw.ElapsedMilliseconds, false, errorType);
            _logger.LogError(ex, "MCP Tool {ToolName} failed with {ErrorType}", toolName, errorType);
            return Result<IEnumerable<Product>>.Failure(ex.Message);
        }
    }
}
