using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class PredictVendorRisksTool : IMcpTool
{
    private readonly RiskPredictionService _service;

    public PredictVendorRisksTool(RiskPredictionService service)
    {
        _service = service;
    }

    public string Name => "predict_vendor_risks";
    public string Description => "Analyze vendor history to predict delays/failures for an event.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "event_id": { "type": "string" },
        "vendor_id": { "type": "string" }
      },
      "required": ["event_id", "vendor_id"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var eventId = args["event_id"]?.GetValue<string>() ?? string.Empty;
        var vendorId = args["vendor_id"]?.GetValue<string>() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(vendorId))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "event_id and vendor_id are required." });
        }

        var result = _service.PredictRisks(eventId, vendorId);

        return Task.FromResult(new ToolResponse
        {
            Success = result.Success,
            Message = result.Recommendation?.Action ?? result.Error ?? string.Empty,
            Data = new
            {
                event_id = result.EventId,
                vendor_id = result.VendorId,
                vendor_name = result.VendorName,
                analysis = result.Analysis,
                recommendation = result.Recommendation
            },
            Error = result.Success ? string.Empty : result.Error ?? string.Empty
        });
    }
}
