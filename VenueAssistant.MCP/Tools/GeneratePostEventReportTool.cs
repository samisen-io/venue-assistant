using System.Text.Json;
using System.Text.Json.Nodes;
using VenueAssistant.MCP.Services;

namespace VenueAssistant.MCP.Tools;

public class GeneratePostEventReportTool : IMcpTool
{
    private readonly ReportingService _service;

    public GeneratePostEventReportTool(ReportingService service)
    {
        _service = service;
    }

    public string Name => "generate_post_event_report";
    public string Description => "Create comprehensive report after event completion.";

    public JsonNode InputSchema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "event_id": { "type": "string" },
        "completed_at": { "type": "string", "description": "ISO format timestamp" }
      },
      "required": ["event_id"]
    }
    """)!;

    public Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default)
    {
        var eventId = args["event_id"]?.GetValue<string>() ?? string.Empty;
        var completedAtStr = args["completed_at"]?.GetValue<string>();
        var completedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(completedAtStr) && DateTime.TryParse(completedAtStr, out var parsed))
        {
            completedAt = parsed;
        }

        if (string.IsNullOrWhiteSpace(eventId))
        {
            return Task.FromResult(new ToolResponse { Success = false, Error = "event_id is required." });
        }

        var result = _service.GenerateReport(eventId, completedAt);

        return Task.FromResult(new ToolResponse
        {
            Success = result.Success,
            Message = result.Summary,
            Data = result.Report,
            Error = result.Success ? string.Empty : result.Error ?? string.Empty
        });
    }
}
