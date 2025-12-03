using System;

namespace VenueAssistant.MCP.Utils;

public static class IdGenerator
{
    public static string NewId() => Guid.NewGuid().ToString();
}
