using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class ListVendorsTool : IMcpTool
{
    private readonly VendorManagementService _service;

    public ListVendorsTool(VendorManagementService service)
    {
        _service = service;
    }

    public string Name => "list_vendors";
    public string Description => "List vendors for a venue, optionally filtered by category.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "venue_id": { "type": "string" },
        "category": { "type": "string" }
      },
      "required": ["venue_id"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var venueId = args["venue_id"]?.GetValue<string>() ?? string.Empty;
        var category = args["category"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(venueId))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "venue_id is required." });
        }

        var vendors = _service.ListVendors(venueId, category);

        return Task.FromResult(new ToolResponse
        {
            Success = true,
            Message = $"Found {vendors.Count()} vendors.",
            Data = vendors
        });
    }
}
