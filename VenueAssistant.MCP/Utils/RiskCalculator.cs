using System;

namespace VenueAssistant.MCP.Utils;

/// <summary>
/// Computes vendor risk scores and qualitative levels based on reliability, timing, and experience.
/// </summary>
public class RiskCalculator
{
    public int CalculateOverallRisk(int reliabilityScore, int daysUntilEvent, int totalEvents)
    {
        var reliabilityRisk = Math.Clamp(100 - reliabilityScore, 0, 100);
        var lastMinuteRisk = daysUntilEvent < 7 ? Math.Max(0, 50 - (daysUntilEvent * 5)) : 0;
        var experienceRisk = totalEvents < 5 ? 20 : 0;

        var overall = (reliabilityRisk * 0.4) + (lastMinuteRisk * 0.3) + (experienceRisk * 0.3);
        return (int)Math.Round(Math.Clamp(overall, 0, 100));
    }

    public string CategorizeRisk(int overallRiskScore) =>
        overallRiskScore > 70 ? "HIGH" :
        overallRiskScore >= 40 ? "MEDIUM" :
        "LOW";
}
