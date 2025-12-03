using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using Serilog;
using VenueAssistant.MCP.Serialization;
using VenueAssistant.MCP.Services;
using VenueAssistant.MCP.Tools;
using VenueAssistant.MCP.Utils;

namespace VenueAssistant.MCP;

public static class Program
{
    // Shared JSON options (snake_case, enums as strings) for all serialization.
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(), new TimeOnlyJsonConverter() }
    };

    public static async Task Main(string[] args)
    {
        ConfigureLogging();

        var seedDemoData = args.Contains("--seed-demo");
        var dataDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data"));
        PersistenceBootstrapper.EnsureDataFiles(dataDirectory, JsonOptions, seedDemoData);
        Log.Information("Starting VenueAssistant MCP HTTP server (ASP.NET Core)...");

        var builder = WebApplication.CreateBuilder(args);

        // Core services
        var storage = new StorageService(dataDirectory);
        storage.Load();
        var intake = new EventIntakeService(storage);
        var ranker = new VendorRankingCalculator();
        var matcher = new VendorMatchingService(storage, ranker);
        var timelineCalc = new TimelineCalculator();
        var timeline = new TimelineGenerationService(storage, timelineCalc);
        var riskCalc = new RiskCalculator();
        var risk = new RiskPredictionService(storage, riskCalc);
        var budget = new BudgetTrackingService(storage);
        var reporting = new ReportingService(storage);
        var vendorMgmt = new VendorManagementService(storage);

        // Tool registrations
        var tools = new List<IMcpTool>
        {
            new ExtractEventRequirementsTool(intake),
            new MatchVendorsToEventTool(matcher),
            new GenerateEventTimelineTool(timeline),
            new PredictVendorRisksTool(risk),
            new TrackEventBudgetTool(budget),
            new GeneratePostEventReportTool(reporting),
            new AddVendorTool(vendorMgmt),
            new ListVendorsTool(vendorMgmt),
            new UpdateVendorPerformanceTool(vendorMgmt)
        };
        var toolMap = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithListToolsHandler((request, cancellationToken) =>
            {
                var toolList = tools.Select(tool =>
                {
                    var inputSchema = tool.InputSchema ?? new JsonObject();
                    return new Tool
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        InputSchema = JsonSerializer.SerializeToElement(inputSchema, JsonOptions)
                    };
                }).ToList();

                return ValueTask.FromResult(new ListToolsResult { Tools = toolList });
            })
            .WithCallToolHandler(async (request, cancellationToken) =>
            {
                var toolName = request.Params?.Name;
                if (string.IsNullOrWhiteSpace(toolName))
                {
                    throw new McpProtocolException("Tool name is required.", McpErrorCode.InvalidParams);
                }

                if (!toolMap.TryGetValue(toolName, out var tool))
                {
                    throw new McpProtocolException($"Tool '{toolName}' not found.", McpErrorCode.MethodNotFound);
                }

                var argsObject = request.Params?.Arguments is null
                    ? new JsonObject()
                    : JsonSerializer.SerializeToNode(request.Params.Arguments, JsonOptions) as JsonObject ?? new JsonObject();

                try
                {
                    var response = await tool.InvokeAsync(argsObject, cancellationToken);
                    var content = JsonSerializer.Serialize(response, JsonOptions);
                    return new CallToolResult
                    {
                        Content = new List<ContentBlock> { new TextContentBlock { Text = content } },
                        IsError = !response.Success
                    };
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Tool {ToolName} failed", toolName);
                    return new CallToolResult
                    {
                        Content = new List<ContentBlock> { new TextContentBlock { Text = $"Tool execution failed: {ex.Message}" } },
                        IsError = true
                    };
                }
            });

        var app = builder.Build();

        app.MapMcp("/mcp");

        try
        {
            await app.RunAsync("http://localhost:5001");
        }
        finally
        {
            Log.Information("Shutting down MCP server.");
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(logsDir, "venue-assistant.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
