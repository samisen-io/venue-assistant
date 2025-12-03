using VenueAssistant.MCP.Models;
using VenueAssistant.MCP.Utils;

namespace VenueAssistant.MCP.Services;

public class TimelineGenerationService
{
    private readonly StorageService _storage;
    private readonly TimelineCalculator _calculator;

    public TimelineGenerationService(StorageService storage, TimelineCalculator calculator)
    {
        _storage = storage;
        _calculator = calculator;
    }

    public TimelineResult GenerateTimeline(string eventId, string eventType, DateTime eventDate, int setupHoursBefore = 4)
    {
        var timeline = _calculator.BuildTimeline(eventType, eventDate, setupHoursBefore);

        _storage.UpdateEvent(eventId, evt => evt with { Timeline = timeline });

        return new TimelineResult
        {
            Success = true,
            EventId = eventId,
            Timeline = timeline,
            Summary = GenerateTimelineSummary(timeline)
        };
    }

    private static string GenerateTimelineSummary(List<TimelineItem> timeline)
    {
        var first = timeline.FirstOrDefault();
        var last = timeline.LastOrDefault();
        return $"Timeline created ({timeline.Count} items) from {first?.Task ?? "start"} to {last?.Task ?? "end"}.";
    }
}

public record TimelineResult
{
    public bool Success { get; init; }
    public required string EventId { get; init; }
    public List<TimelineItem> Timeline { get; init; } = new();
    public string Summary { get; init; } = string.Empty;
}
