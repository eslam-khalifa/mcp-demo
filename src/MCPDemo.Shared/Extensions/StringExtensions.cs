using System.Text.RegularExpressions;

namespace MCPDemo.Shared.Extensions;

/// <summary>
/// basic string utility extensions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string into a URL-friendly slug.
    /// </summary>
    public static string ToSlug(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var slug = text.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", " ").Trim();
        slug = slug.Replace(" ", "-");

        return slug;
    }
}
