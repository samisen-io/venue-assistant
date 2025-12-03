using System;

namespace VenueAssistant.MCP.Models;

public record TimelineItem
{
    public required string Task { get; init; }
    public double OffsetHoursFromEventStart { get; init; }
    public string Owner { get; init; } = string.Empty;
    public string Status { get; init; } = "pending"; // pending, in_progress, completed, delayed
    public DateTime? ScheduledTime { get; init; }
    public DateTime? ActualTime { get; init; }
    public string Notes { get; init; } = string.Empty;
}
