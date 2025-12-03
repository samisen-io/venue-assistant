using VenueAssistant.MCP.Utils;
using Xunit;

namespace VenueAssistant.MCP.Tests.Utils;

public class RiskCalculatorTests
{
    [Theory]
    [InlineData(95, 14, 20, "LOW")]
    [InlineData(60, 3, 2, "LOW")]
    [InlineData(40, 1, 0, "MEDIUM")]
    public void CategorizeRisk_ReturnsExpected(int reliability, int daysUntilEvent, int totalEvents, string expectedLevel)
    {
        var calc = new RiskCalculator();
        var score = calc.CalculateOverallRisk(reliability, daysUntilEvent, totalEvents);
        var level = calc.CategorizeRisk(score);

        Assert.Equal(expectedLevel, level);
    }
}
