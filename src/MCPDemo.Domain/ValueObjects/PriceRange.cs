namespace MCPDemo.Domain.ValueObjects;

/// <summary>
/// Represents a price window for filtering and validation.
/// </summary>
/// <param name="Min">Maximum price in the range.</param>
/// <param name="Max">Minimum price in the range.</param>
public record PriceRange(decimal Min, decimal Max)
{
    /// <summary>
    /// Validates the price range logic.
    /// </summary>
    /// <returns>True if Min is non-negative and Min is less than or equal to Max.</returns>
    public bool IsValid() => Min >= 0 && Min <= Max;
}
