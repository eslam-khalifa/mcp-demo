namespace MCPDemo.Application.DTOs.Analytics;

public record PriceStatisticsDto(
    decimal Min,
    decimal Max,
    decimal Mean,
    decimal Median,
    decimal StdDev,
    int Count);

public record TopExpensiveDto(
    IEnumerable<MCPDemo.Domain.Entities.Product> Products,
    decimal AveragePrice);

public record CategoryReportDto(
    int CategoryId,
    string CategoryName,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    int ProductCount);

public record PriceDistributionDto(
    IEnumerable<PriceBucketDto> Buckets);

public record PriceBucketDto(
    decimal RangeMin,
    decimal RangeMax,
    int Count);
