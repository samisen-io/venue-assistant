using VenueAssistant.MCP.Models;
using VenueAssistant.MCP.Utils;

namespace VenueAssistant.MCP.Services;

public class RiskPredictionService
{
    private readonly StorageService _storage;
    private readonly RiskCalculator _calculator;

    public RiskPredictionService(StorageService storage, RiskCalculator calculator)
    {
        _storage = storage;
        _calculator = calculator;
    }

    public RiskAnalysisResult PredictRisks(string eventId, string vendorId)
    {
        var evt = _storage.GetEvent(eventId);
        if (evt is null)
        {
            return new RiskAnalysisResult { Success = false, Error = "Event not found." };
        }

        var vendor = _storage.GetVendor(evt.VenueId, vendorId);
        if (vendor is null)
        {
            return new RiskAnalysisResult { Success = false, Error = "Vendor not found." };
        }

        var daysUntilEvent = (int)Math.Max(0, (evt.EventDate - DateTime.UtcNow).TotalDays);
        var overall = _calculator.CalculateOverallRisk(vendor.ReliabilityScore, daysUntilEvent, vendor.TotalEvents);
        var level = _calculator.CategorizeRisk(overall);

        return new RiskAnalysisResult
        {
            Success = true,
            EventId = eventId,
            VendorId = vendorId,
            VendorName = vendor.Name,
            Analysis = new RiskAnalysis
            {
                OverallRiskScore = overall,
                RiskLevel = level,
                Factors = new Dictionary<string, RiskFactor>
                {
                    ["reliability"] = new RiskFactor
                    {
                        Score = vendor.ReliabilityScore,
                        Assessment = vendor.ReliabilityScore > 85 ? "Excellent track record" : "Mixed history"
                    },
                    ["timing"] = new RiskFactor
                    {
                        Score = daysUntilEvent,
                        Assessment = daysUntilEvent < 7 ? "Last-minute risk" : "Adequate lead time"
                    },
                    ["experience"] = new RiskFactor
                    {
                        Score = vendor.TotalEvents,
                        Assessment = vendor.TotalEvents < 5 ? "Limited experience" : "Experienced vendor"
                    }
                }
            },
            Recommendation = new RiskRecommendation
            {
                Action = level switch
                {
                    "HIGH" => "Add backup vendor and increase check-ins",
                    "MEDIUM" => "Confirm timelines and set milestones",
                    _ => "Proceed normally"
                },
                Reasoning = $"Risk level {level} with score {overall}."
            }
        };
    }
}

public record RiskAnalysisResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? EventId { get; init; }
    public string? VendorId { get; init; }
    public string? VendorName { get; init; }
    public RiskAnalysis? Analysis { get; init; }
    public RiskRecommendation? Recommendation { get; init; }
}

public record RiskAnalysis
{
    public int OverallRiskScore { get; init; }
    public string RiskLevel { get; init; } = "LOW";
    public Dictionary<string, RiskFactor> Factors { get; init; } = new();
}

public record RiskFactor
{
    public int Score { get; init; }
    public string Assessment { get; init; } = string.Empty;
}

public record RiskRecommendation
{
    public string Action { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
}
