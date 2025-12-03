using System;
using System.Collections.Generic;

namespace VenueAssistant.MCP.Models;

public record Event
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string VenueId { get; init; }
    public required string EventName { get; init; }
    public required string EventType { get; init; } // gala, conference, wedding, corporate
    public DateTime EventDate { get; init; }
    public TimeOnly EventTime { get; init; }
    public int GuestCount { get; init; }
    public decimal BudgetTotal { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, object> SpecialRequirements { get; init; } = new();
    public string Status { get; init; } = "planning"; // planning, confirmed, in_progress, completed
    public required string CreatedBy { get; init; }
    public List<TimelineItem> Timeline { get; init; } = new();
    public Dictionary<string, decimal> BudgetBreakdown { get; init; } = new();
    public List<string> AssignedVendorIds { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; init; }
}
