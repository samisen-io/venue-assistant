using System;
using VenueAssistant.MCP.Models;

namespace VenueAssistant.MCP.Utils;

/// <summary>
/// Scores vendors against an event budget and history per PRD weights.
/// </summary>
public class VendorRankingCalculator
{
    public double CalculateMatchScore(Vendor vendor, decimal budgetPerGuest)
    {
        var costFit = CalculateCostFitPercentage(vendor.CostPerUnit, budgetPerGuest);
        var reliabilityComponent = vendor.ReliabilityScore * 0.4;
        var costFitComponent = costFit * 0.3;
        var experienceComponent = Math.Min(vendor.TotalEvents, 20) * 0.2;
        var costAccuracyComponent = CalculateCostAccuracy(vendor) * 0.1;

        return reliabilityComponent + costFitComponent + experienceComponent + costAccuracyComponent;
    }

    private static double CalculateCostFitPercentage(decimal vendorCostPerUnit, decimal budgetPerGuest)
    {
        if (vendorCostPerUnit <= 0 || budgetPerGuest <= 0)
        {
            return 0;
        }

        var fit = (double)(budgetPerGuest / vendorCostPerUnit) * 100;
        return Math.Clamp(fit, 0, 100);
    }

    private static double CalculateCostAccuracy(Vendor vendor)
    {
        // Proxy for cost accuracy; lacking explicit metric, use on-time percentage as stability indicator.
        return vendor.OnTimePercentage > 0 ? (double)vendor.OnTimePercentage : 100;
    }
}
