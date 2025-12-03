using System;

namespace VenueAssistant.MCP.Models;

public record Communication
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string EventId { get; init; }
    public required string VendorId { get; init; }
    public string Type { get; init; } = "email"; // email, sms, api, note
    public string Subject { get; init; } = string.Empty;
    public required string Body { get; init; }
    public DateTime? SentAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime? ResponseReceivedAt { get; init; }
    public string ResponseContent { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
