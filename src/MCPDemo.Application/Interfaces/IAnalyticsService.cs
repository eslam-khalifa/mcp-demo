using MCPDemo.Application.DTOs.Analytics;
using MCPDemo.Shared.Models;

namespace MCPDemo.Application.Interfaces;

public interface IAnalyticsService
{
    Task<Result<PriceStatisticsDto>> GetPriceStatisticsAsync(int? categoryId = null);
    Task<Result<TopExpensiveDto>> GetTopExpensiveProductsAsync(int n, int? categoryId = null);
    Task<Result<IEnumerable<CategoryReportDto>>> GetCategoryPriceReportAsync();
    Task<Result<PriceDistributionDto>> GetPriceDistributionAsync(int? bucketCount = null);
}
