# VenueAssistant MCP (HTTP JSON-RPC)

Minimal MCP server in C# (.NET 10) with file-based persistence, HTTP JSON-RPC tools, and sample data. Uses `ModelContextProtocol.AspNetCore` for the MCP streamable HTTP transport.

## Quick Start

```bash
dotnet restore
dotnet run --project VenueAssistant.MCP -- --seed-demo
```

- Server listens on `http://localhost:5001/mcp` (MCP streamable HTTP; POST JSON-RPC or GET for streaming).
- Add `--seed-demo` to prefill sample venues, vendors, and an event.

## Tooling

- List tools:
```json
{ "jsonrpc": "2.0", "id": 1, "method": "tools/list" }
```

- Call a tool (example: `match_vendors_to_event`):
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "match_vendors_to_event",
    "arguments": {
      "event_id": "event-1",
      "vendor_categories": ["catering", "AV"]
    }
  }
}
```

## Project Structure

- `VenueAssistant.MCP/` – app code (models, services, tools, server)
- `VenueAssistant.MCP/Data/` – sample seed JSON
- `VenueAssistant.MCP.Tests/` – xUnit tests
- `Instructions/prd.md` – product spec
- `Instructions/TASKS.md` – tracker

## Services & Tools

- Event intake, vendor matching, timeline generation, risk prediction, budget tracking, reporting, vendor management.
- MCP tools: extract_event_requirements, match_vendors_to_event, generate_event_timeline, predict_vendor_risks, track_event_budget, generate_post_event_report, add_vendor, list_vendors, update_vendor_performance.

## Persistence

- File-based JSON in `./data` (auto-created). Use `--seed-demo` once to copy sample JSON into `./data`.

## Logging

- Serilog to console + rolling file `logs/venue-assistant.log`.

## Tests

```bash
dotnet test VenueAssistant.MCP.Tests/VenueAssistant.MCP.Tests.csproj
```

## Deployment

- Build: `dotnet build`
- Publish self-contained (example): `dotnet publish VenueAssistant.MCP -c Release -r win-x64 --self-contained true`
- HTTP port is currently fixed at 5001; update `Program.cs` if you need a different host/port.
