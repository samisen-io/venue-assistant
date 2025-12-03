using System;
using System.Collections.Generic;

namespace VenueAssistant.MCP.Models;

public record Venue
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Phone { get; init; }
    public int Capacity { get; init; }
    public List<string> PreferredVendorIds { get; init; } = new();
    public string SubscriptionTier { get; init; } = "starter"; // starter, growth, enterprise
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}
