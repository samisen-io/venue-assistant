using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class TrackEventBudgetTool : IMcpTool
{
    private readonly BudgetTrackingService _service;

    public TrackEventBudgetTool(BudgetTrackingService service)
    {
        _service = service;
    }

    public string Name => "track_event_budget";
    public string Description => "Track budget, flag overruns, and suggest optimizations.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "event_id": { "type": "string" },
        "action": { "type": "string", "enum": ["get_summary", "add_cost", "flag_overrun", "get_optimization_suggestions"] },
        "category": { "type": "string" },
        "amount": { "type": "number" }
      },
      "required": ["event_id", "action"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var eventId = args["event_id"]?.GetValue<string>() ?? string.Empty;
        var action = args["action"]?.GetValue<string>() ?? string.Empty;
        var category = args["category"]?.GetValue<string>();
        var amount = args["amount"]?.GetValue<decimal?>() ?? 0;

        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(action))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "event_id and action are required." });
        }

        switch (action)
        {
            case "get_summary":
                {
                    var summary = _service.GetBudgetSummary(eventId);
                    return Task.FromResult(new ToolResponse { Success = summary.Status != "NOT_FOUND", Data = summary, Message = "Budget summary retrieved." });
                }
            case "add_cost":
                {
                    if (string.IsNullOrWhiteSpace(category))
                    {
                        return Task.FromResult(new ToolResponse { Success = false, Error = "category is required for add_cost." });
                    }
                    _service.AddCost(eventId, category!, amount);
                    var summary = _service.GetBudgetSummary(eventId);
                    return Task.FromResult(new ToolResponse { Success = true, Data = summary, Message = "Cost added." });
                }
            case "flag_overrun":
                {
                    var alert = _service.CheckForOverrun(eventId);
                    return Task.FromResult(new ToolResponse { Success = alert.Status != "NOT_FOUND", Data = alert, Message = "Overrun check complete." });
                }
            case "get_optimization_suggestions":
                {
                    var suggestions = _service.GetOptimizations(eventId);
                    return Task.FromResult(new ToolResponse { Success = true, Data = suggestions, Message = "Optimization suggestions generated." });
                }
            default:
                return Task.FromResult(new ToolResponse { Success = false, Error = "Unsupported action." });
        }
    }
}
