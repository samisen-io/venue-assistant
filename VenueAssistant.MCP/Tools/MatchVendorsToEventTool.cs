using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class MatchVendorsToEventTool : IMcpTool
{
    private readonly VendorMatchingService _service;

    public MatchVendorsToEventTool(VendorMatchingService service)
    {
        _service = service;
    }

    public string Name => "match_vendors_to_event";
    public string Description => "Find and rank best vendors for an event.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "event_id": { "type": "string", "description": "ID of the event" },
        "vendor_categories": { "type": "array", "items": { "type": "string" }, "description": "Categories like catering, AV, florals" }
      },
      "required": ["event_id", "vendor_categories"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var eventId = args["event_id"]?.GetValue<string>() ?? string.Empty;
        var categories = args["vendor_categories"]?.AsArray().Select(x => x!.GetValue<string>()).ToList() ?? new List<string>();

        if (string.IsNullOrWhiteSpace(eventId) || categories.Count == 0)
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "event_id and vendor_categories are required." });
        }

        var result = _service.MatchVendors(eventId, categories);

        return Task.FromResult(new ToolResponse
        {
            Success = result.Success,
            Message = result.Summary,
            Data = new
            {
                event_id = result.EventId,
                matches = result.Matches
            }
        });
    }
}
