using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class ExtractEventRequirementsTool : IMcpTool
{
    private readonly EventIntakeService _service;

    public ExtractEventRequirementsTool(EventIntakeService service)
    {
        _service = service;
    }

    public string Name => "extract_event_requirements";
    public string Description => "Parse natural language conversation to extract event requirements and create an event.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "venue_id": { "type": "string", "description": "ID of the venue" },
        "conversation_text": { "type": "string", "description": "Natural language description of event needs" }
      },
      "required": ["venue_id", "conversation_text"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var venueId = args["venue_id"]?.GetValue<string>() ?? string.Empty;
        var conversation = args["conversation_text"]?.GetValue<string>() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(venueId) || string.IsNullOrWhiteSpace(conversation))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "venue_id and conversation_text are required." });
        }

        var result = _service.ExtractFromConversation(venueId, conversation);
        return Task.FromResult(new ToolResponse
        {
            Success = result.Success,
            Message = result.Summary,
            Data = new
            {
                event_id = result.Event.Id,
                summary = result.Summary,
                next_steps = result.NextSteps
            }
        });
    }
}
