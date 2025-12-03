using System;
using VenueAssistant.MCP.Utils;
using Xunit;

namespace VenueAssistant.MCP.Tests.Utils;

public class TimelineCalculatorTests
{
    [Fact]
    public void BuildTimeline_UsesTemplate_WhenKnownType()
    {
        var calc = new TimelineCalculator();
        var timeline = calc.BuildTimeline("gala", new DateTime(2025, 5, 15, 18, 0, 0));

        Assert.NotEmpty(timeline);
        Assert.Contains(timeline, t => t.Task == "Guests Arrive");
    }

    [Fact]
    public void BuildTimeline_UsesFallback_WhenUnknownType()
    {
        var calc = new TimelineCalculator();
        var timeline = calc.BuildTimeline("unknown", new DateTime(2025, 5, 15, 18, 0, 0));

        Assert.NotEmpty(timeline);
        Assert.Contains(timeline, t => t.Task == "Setup Begins");
    }
}
