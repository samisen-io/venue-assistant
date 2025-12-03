using System;

namespace VenueAssistant.MCP.Models;

public record Vendor
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string VenueId { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; } // catering, AV, florals, parking, etc.
    public required string ContactEmail { get; init; }
    public required string ContactPhone { get; init; }
    public string Website { get; init; } = string.Empty;
    public decimal CostPerUnit { get; init; } = 0;
    public int ReliabilityScore { get; init; } = 0; // 0-100
    public int TotalEvents { get; init; } = 0;
    public int OnTimeCount { get; init; } = 0;
    public decimal AverageQualityRating { get; init; } = 0; // 0-5
    public decimal OnTimePercentage { get; init; } = 0; // 0-100
    public string Notes { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}
