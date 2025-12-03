# Deployment Guide

## Build

```bash
dotnet build
```

## Publish (self-contained examples)

- Windows x64:
```bash
dotnet publish VenueAssistant.MCP -c Release -r win-x64 --self-contained true
```

- Linux x64:
```bash
dotnet publish VenueAssistant.MCP -c Release -r linux-x64 --self-contained true
```

- macOS x64:
```bash
dotnet publish VenueAssistant.MCP -c Release -r osx-x64 --self-contained true
```

Artifacts land under `VenueAssistant.MCP/bin/Release/<tfm>/<rid>/publish/`.

## Runtime

- The server listens on `http://localhost:5001/mcp` using the MCP streamable HTTP transport. Adjust the URL in `Program.cs` if needed.
- Data directory: `./data` relative to the executable; ensure it is writable.
- Seed demo data once with `--seed-demo` (optional).

## Logging

- Console + rolling file at `logs/venue-assistant.log` next to the executable. Ensure the process can write to `logs/`.

## Health / Troubleshooting

- If the port is in use, change the URL in `Program.cs`.
- Ensure .NET 10 runtime is present unless you published self-contained.
- File permissions: data and logs directories must be writable.
