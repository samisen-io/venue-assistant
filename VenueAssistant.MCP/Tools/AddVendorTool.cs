using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class AddVendorTool : IMcpTool
{
    private readonly VendorManagementService _service;

    public AddVendorTool(VendorManagementService service)
    {
        _service = service;
    }

    public string Name => "add_vendor";
    public string Description => "Add a new vendor to a venue.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "venue_id": { "type": "string" },
        "vendor_name": { "type": "string" },
        "category": { "type": "string" },
        "contact_email": { "type": "string" },
        "contact_phone": { "type": "string" },
        "cost_per_unit": { "type": "number" }
      },
      "required": ["venue_id", "vendor_name", "category", "contact_email"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var venueId = args["venue_id"]?.GetValue<string>() ?? string.Empty;
        var vendorName = args["vendor_name"]?.GetValue<string>() ?? string.Empty;
        var category = args["category"]?.GetValue<string>() ?? string.Empty;
        var contactEmail = args["contact_email"]?.GetValue<string>() ?? string.Empty;
        var contactPhone = args["contact_phone"]?.GetValue<string>() ?? string.Empty;
        var costPerUnit = args["cost_per_unit"]?.GetValue<decimal?>() ?? 0;

        if (string.IsNullOrWhiteSpace(venueId) || string.IsNullOrWhiteSpace(vendorName) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(contactEmail))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "venue_id, vendor_name, category, and contact_email are required." });
        }

        var vendor = _service.AddVendor(venueId, new VendorCreationRequest
        {
            VendorName = vendorName,
            Category = category,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            CostPerUnit = costPerUnit
        });

        return Task.FromResult(new ToolResponse
        {
            Success = true,
            Message = $"Added {vendor.Name} to venue {venueId}.",
            Data = vendor
        });
    }
}
