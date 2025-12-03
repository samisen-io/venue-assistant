using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class UpdateVendorPerformanceTool : IMcpTool
{
    private readonly VendorManagementService _service;

    public UpdateVendorPerformanceTool(VendorManagementService service)
    {
        _service = service;
    }

    public string Name => "update_vendor_performance";
    public string Description => "Update vendor performance metrics after an event.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "vendor_id": { "type": "string" },
        "event_id": { "type": "string" },
        "venue_id": { "type": "string" },
        "on_time": { "type": "boolean" },
        "quality_rating": { "type": "number", "minimum": 1, "maximum": 5 },
        "cost_accurate": { "type": "boolean" },
        "notes": { "type": "string" }
      },
      "required": ["vendor_id", "event_id", "venue_id"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var vendorId = args["vendor_id"]?.GetValue<string>() ?? string.Empty;
        var eventId = args["event_id"]?.GetValue<string>() ?? string.Empty;
        var venueId = args["venue_id"]?.GetValue<string>() ?? string.Empty;
        var onTime = args["on_time"]?.GetValue<bool?>() ?? false;
        var qualityRating = args["quality_rating"]?.GetValue<decimal?>() ?? 0;
        var costAccurate = args["cost_accurate"]?.GetValue<bool?>() ?? true;
        var notes = args["notes"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(vendorId) || string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(venueId))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "vendor_id, event_id, and venue_id are required." });
        }

        _service.UpdatePerformance(venueId, vendorId, new PerformanceUpdate
        {
            OnTime = onTime,
            QualityRating = qualityRating,
            CostAccurate = costAccurate,
            Notes = notes
        });

        return Task.FromResult(new ToolResponse
        {
            Success = true,
            Message = "Vendor performance updated.",
            Data = new { vendor_id = vendorId, event_id = eventId, venue_id = venueId }
        });
    }
}
