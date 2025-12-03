using System;

namespace VenueAssistant.MCP.Models;

public record EventVendor
{
    public required string EventId { get; init; }
    public required string VendorId { get; init; }
    public string Role { get; init; } = "primary"; // primary, backup, alternate
    public string Status { get; init; } = "pending"; // pending, confirmed, declined, failed, completed
    public decimal QuoteAmount { get; init; } = 0;
    public DateTime? QuoteDate { get; init; }
    public decimal? ActualCost { get; init; }
    public DateTime? ConfirmedDate { get; init; }
    public TimeOnly? ArrivalTime { get; init; }
    public TimeOnly? CompletionTime { get; init; }
    public int QualityRating { get; init; } = 0; // 1-5
    public string Notes { get; init; } = string.Empty;
    public bool RiskFlag { get; init; } = false;
    public string RiskReason { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}
