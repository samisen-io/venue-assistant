using System.Text.RegularExpressions;
using VenueAssistant.MCP.Models;
using VenueAssistant.MCP.Utils;

namespace VenueAssistant.MCP.Services;

public class EventIntakeService
{
    private readonly StorageService _storage;

    public EventIntakeService(StorageService storage)
    {
        _storage = storage;
    }

    public EventIntakeResult ExtractFromConversation(string venueId, string conversationText)
    {
        var requirements = ParseConversation(conversationText);
        var evt = CreateEvent(venueId, requirements);

        return new EventIntakeResult
        {
            Success = true,
            Event = evt,
            Summary = GenerateSummary(evt),
            NextSteps = new[]
            {
                "Confirm event details",
                "Run vendor matching",
                "Generate timeline"
            }
        };
    }

    public Event CreateEvent(string venueId, EventRequirements requirements)
    {
        var evt = new Event
        {
            Id = IdGenerator.NewId(),
            VenueId = venueId,
            EventName = requirements.EventName,
            EventType = requirements.EventType,
            EventDate = requirements.EventDate,
            EventTime = requirements.EventTime,
            GuestCount = requirements.GuestCount,
            BudgetTotal = requirements.BudgetTotal,
            Description = requirements.Description,
            SpecialRequirements = requirements.SpecialRequirements,
            CreatedBy = requirements.CreatedBy
        };

        _storage.AddEvent(evt);
        return evt;
    }

    private EventRequirements ParseConversation(string text)
    {
        var lower = text.ToLowerInvariant();
        var eventType = MatchFirst(lower, "gala|conference|wedding|corporate|party|summit") ?? "event";
        var guestCount = MatchInt(text, @"(\d{2,5})\s*(guests|people|attendees)") ?? 0;
        var budget = MatchDecimal(text, @"\$(\d+(?:,\d{3})*(?:\.\d{1,2})?)") ?? 0;
        var date = MatchDate(text) ?? DateTime.UtcNow.Date.AddDays(30);

        return new EventRequirements
        {
            EventName = $"New {eventType}",
            EventType = eventType,
            EventDate = date,
            EventTime = new TimeOnly(17, 0),
            GuestCount = guestCount,
            BudgetTotal = budget,
            RequiredVendorCategories = new List<string>(),
            Description = text.Trim(),
            SpecialRequirements = new Dictionary<string, object>(),
            CreatedBy = "claude"
        };
    }

    private static string? MatchFirst(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static int? MatchInt(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return null;
        if (int.TryParse(match.Groups[1].Value, out var value))
        {
            return value;
        }
        return null;
    }

    private static decimal? MatchDecimal(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return null;
        var cleaned = match.Groups[1].Value.Replace(",", "");
        if (decimal.TryParse(cleaned, out var value))
        {
            return value;
        }
        return null;
    }

    private static DateTime? MatchDate(string text)
    {
        var match = Regex.Match(text, @"(\d{4}-\d{2}-\d{2})");
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var dt))
        {
            return dt;
        }
        return null;
    }

    private static string GenerateSummary(Event evt)
    {
        return $"Event: {evt.EventName}, Type: {evt.EventType}, Date: {evt.EventDate:yyyy-MM-dd}, Guests: {evt.GuestCount}, Budget: {evt.BudgetTotal:C0}.";
    }
}

public record EventIntakeResult
{
    public bool Success { get; init; }
    public required Event Event { get; init; }
    public required string Summary { get; init; }
    public IEnumerable<string> NextSteps { get; init; } = Enumerable.Empty<string>();
}

public record EventRequirements
{
    public required string EventName { get; init; }
    public required string EventType { get; init; }
    public required DateTime EventDate { get; init; }
    public required TimeOnly EventTime { get; init; }
    public int GuestCount { get; init; }
    public decimal BudgetTotal { get; init; }
    public List<string> RequiredVendorCategories { get; init; } = new();
    public string Description { get; init; } = string.Empty;
    public Dictionary<string, object> SpecialRequirements { get; init; } = new();
    public string CreatedBy { get; init; } = "claude";
}
