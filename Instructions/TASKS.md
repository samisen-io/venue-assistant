# VenueAssistant MCP - Build Tracker

- [x] Project scaffold (`.NET 10` console app, csproj deps, snake_case JSON options)
- [x] Domain models (`Venue`, `Vendor`, `Event`, `EventVendor`, `TimelineItem`, `Communication`)
- [x] StorageService (file-based JSON persistence, thread-safe CRUD, ID generation)
- [x] Persistence bootstrap (`./data` creation, default empty JSON stubs)
- [x] Utility calculators (`VendorRankingCalculator`, `RiskCalculator`, `TimelineCalculator`, `IdGenerator`)
- [x] Services layer (`EventIntakeService`, `VendorMatchingService`, `TimelineGenerationService`, `RiskPredictionService`, `BudgetTrackingService`, `ReportingService`, `VendorManagementService`)
- [x] MCP tool interfaces (`IMcpTool` + nine tool classes; summaries/recommendations per logic)
- [x] JSON-RPC layer (tool listing/call handling over HTTP, async request handling, `ToolResponse` envelope)
- [x] MCP server host (`Program.cs` DI/logging setup, register tools, start HTTP MCP server)
- [x] Logging wiring (`ILogger`/Serilog console + optional file sink)
- [x] Demo/sample data for venues, vendors, and events to exercise tools
- [x] Testing (xUnit unit tests for calculators/services; integration flows end-to-end)
- [x] Documentation (`README`, tool descriptions, sample payloads, system prompt, demo data notes)
- [x] Deployment guide (`dotnet publish` targets, optional Dockerfile)

Tool implementation checklist:
- [x] Tool: extract_event_requirements
- [x] Tool: match_vendors_to_event
- [x] Tool: generate_event_timeline
- [x] Tool: predict_vendor_risks
- [x] Tool: track_event_budget
- [x] Tool: generate_post_event_report
- [x] Tool: add_vendor
- [x] Tool: list_vendors
- [x] Tool: update_vendor_performance
