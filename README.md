# VenueAssistant MCP — event operations as agent-callable tools

A minimal [MCP](https://modelcontextprotocol.io) server in **C# / .NET 10** that exposes venue and
event operations — vendor matching, budgeting, timelines, risk prediction, post-event reporting — as
MCP tools over HTTP, so an agent (Claude Desktop, or any MCP client) can plan an event through tool
calls instead of a UI. Uses `ModelContextProtocol.AspNetCore` for the MCP streamable HTTP transport.

## Why this exists

Event operations are repetitive, data-heavy and rule-shaped: rank vendors by fit, watch budget lines,
sequence a timeline, flag risks. That is exactly the work an agent should carry — but only if the
tools underneath are deterministic and auditable.

This server keeps the decisions in C# (calculators covered by unit tests) and lets the model do what
it is good at: understanding the request, choosing tools and narrating the result. The model never
invents a vendor ranking or a budget total; it calls a tool that computes one.

## Tools

| Tool | What it does |
|---|---|
| `extract_event_requirements` | Turns a free-text request into structured event requirements |
| `match_vendors_to_event` | Ranks vendors for an event by fit, availability and past performance |
| `predict_vendor_risks` | Flags likely problem vendors/areas for an event |
| `generate_event_timeline` | Produces a sequenced run-of-show for the event |
| `track_event_budget` | Budget position: planned, committed, remaining per line |
| `add_cost` | Adds a cost line against an event or vendor |
| `generate_post_event_report` | Close-out report: spend vs plan, vendor performance, exceptions |
| `add_vendor` | Registers a vendor with categories and rates |
| `list_vendors` | Lists vendors, optionally filtered by category |
| `update_vendor_performance` | Records post-event performance against a vendor |

## Architecture

```
MCP client ──HTTP JSON-RPC──▶ Program.cs  (ASP.NET Core host, DI, MapMcp("/mcp"))
                                  │
                                  ├── Tools/*      one class per tool, behind IMcpTool
                                  ├── Services/*   intake, matching, timeline, risk,
                                  │                budget, reporting, vendor + storage
                                  ├── Utils/*      VendorRankingCalculator, TimelineCalculator,
                                  │                RiskCalculator
                                  └── Data/*.json  file-based persistence + seed data
```

- **Transport:** streamable HTTP at `http://localhost:5001/mcp` — POST JSON-RPC, or GET for streaming.
- **Serialization:** snake_case JSON, enums as strings, `TimeOnly` converter, so tool payloads stay
  readable in an agent transcript.
- **Logging:** Serilog to console and to a rolling file at `logs/venue-assistant.log`.

## Quick start

```bash
dotnet restore
dotnet run --project VenueAssistant.MCP -- --seed-demo
```

`--seed-demo` copies the sample JSON into `./data` (auto-created) so the tools have venues, vendors
and an event to work with. Persistence is file-based JSON in `./data`.

List tools:

```bash
curl -s http://localhost:5001/mcp -H 'content-type: application/json' \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

Call one:

```bash
curl -s http://localhost:5001/mcp -H 'content-type: application/json' \
  -d '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"match_vendors_to_event",
      "arguments":{"event_id":"event-1","vendor_categories":["catering","AV"]}}}'
```

## Connect an MCP client

```json
{ "mcpServers": { "venue-assistant": { "url": "http://localhost:5001/mcp" } } }
```

HTTP transport means the server must be running before the client connects. `DEMO_WALKTHROUGH.md` is a
step-by-step Claude Desktop demo script ("the Grand Wedding at the Grand Pavilion") for exactly this
setup.

## Tests

```bash
dotnet test VenueAssistant.MCP.Tests/VenueAssistant.MCP.Tests.csproj
```

xUnit tests cover the three calculators — vendor ranking, timeline sequencing and risk scoring. That
is deliberate: the arithmetic an agent will rely on is the part that has to be provably right.

## Design notes

- **Deterministic core, model at the edge.** Anything a user could dispute is computed in tested C#;
  the model orchestrates and explains.
- **One class per tool** behind `IMcpTool`, so a new capability is one file plus a registration line.
- **JSON-file persistence** keeps the demo inspectable and dependency-free; `StorageService` is the
  seam to replace with a database.
- **Preview dependency:** `ModelContextProtocol.AspNetCore 0.4.1-preview.1` — expect API movement.

## Deployment

```bash
dotnet build
dotnet publish VenueAssistant.MCP -c Release -r win-x64 --self-contained true
```

The HTTP port is currently fixed at 5001 in `Program.cs`.

## Limitations and next steps

- Single-tenant and unauthenticated — a localhost demo, not a hosted service.
- No retrieval: vendor history and past-event notes are stored but not embedded or searched. A
  `search_vendor_history` tool over a vector index is the natural next capability.
- Test coverage stops at the calculators; the service layer (matching, budget) is next.
- A Postgres-backed `StorageService` with per-tenant isolation is the step from demo to product.

## Project structure

```
VenueAssistant.MCP/          server: tools, services, utils, models, seed data
VenueAssistant.MCP.Tests/    xUnit tests for the calculators
Instructions/prd.md          product spec
Instructions/TASKS.md        build tracker
DEMO_WALKTHROUGH.md          Claude Desktop demo script
venue-assistant-mcp.sln      solution
```
