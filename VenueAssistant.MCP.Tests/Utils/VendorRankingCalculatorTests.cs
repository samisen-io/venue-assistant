using VenueAssistant.MCP.Models;
using VenueAssistant.MCP.Utils;
using Xunit;

namespace VenueAssistant.MCP.Tests.Utils;

public class VendorRankingCalculatorTests
{
    [Fact]
    public void CalculateMatchScore_UsesWeights()
    {
        var calc = new VendorRankingCalculator();
        var vendor = new Vendor
        {
            VenueId = "venue-1",
            Name = "Test Vendor",
            Category = "catering",
            ContactEmail = "a@b.com",
            ContactPhone = "555",
            CostPerUnit = 25,
            ReliabilityScore = 90,
            TotalEvents = 10,
            OnTimePercentage = 95
        };

        var score = calc.CalculateMatchScore(vendor, budgetPerGuest: 30);

        Assert.InRange(score, 0, 100);
        Assert.True(score > 50);
    }
}
