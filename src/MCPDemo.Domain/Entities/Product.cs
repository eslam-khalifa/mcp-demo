namespace MCPDemo.Domain.Entities;

/// <summary>
/// Represents a store item with full details.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// The category this product belongs to.
    /// </summary>
    public Category Category { get; set; } = default!;
    
    /// <summary>
    /// List of URLs for product images.
    /// </summary>
    public List<string> Images { get; set; } = new();
    
    public DateTime CreationAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
