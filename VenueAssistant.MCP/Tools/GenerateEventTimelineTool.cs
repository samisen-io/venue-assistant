using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class GenerateEventTimelineTool : IMcpTool
{
    private readonly TimelineGenerationService _service;

    public GenerateEventTimelineTool(TimelineGenerationService service)
    {
        _service = service;
    }

    public string Name => "generate_event_timeline";
    public string Description => "Auto-generate timeline milestones for an event.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "event_id": { "type": "string" },
        "event_type": { "type": "string" },
        "event_date": { "type": "string", "description": "ISO format date" },
        "setup_hours_before": { "type": "number" }
      },
      "required": ["event_id", "event_type", "event_date"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var eventId = args["event_id"]?.GetValue<string>() ?? string.Empty;
        var eventType = args["event_type"]?.GetValue<string>() ?? string.Empty;
        var eventDateStr = args["event_date"]?.GetValue<string>() ?? string.Empty;
        var setupHours = args["setup_hours_before"]?.GetValue<double?>() ?? 4;

        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(eventType) || string.IsNullOrWhiteSpace(eventDateStr))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "event_id, event_type, and event_date are required." });
        }

        if (!DateTime.TryParse(eventDateStr, out var eventDate))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "event_date must be ISO format." });
        }

        var result = _service.GenerateTimeline(eventId, eventType, eventDate, (int)setupHours);

        return Task.FromResult(new ToolResponse
        {
            Success = result.Success,
            Message = result.Summary,
            Data = new
            {
                event_id = result.EventId,
                timeline = result.Timeline
            }
        });
    }
}
