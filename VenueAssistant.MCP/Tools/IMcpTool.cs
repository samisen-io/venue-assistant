using System.Text.Json.Nodes;

namespace VenueAssistant.MCP.Tools;

public interface IMcpTool
{
    string Name { get; }
    string Description { get; }
    JsonNode InputSchema { get; }
    Task<ToolResponse> InvokeAsync(JsonObject args, CancellationToken cancellationToken = default);
}

public record ToolResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; }
    public string Error { get; init; } = string.Empty;
}
