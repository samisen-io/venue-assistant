using VenueAssistant.MCP.Models;

namespace VenueAssistant.MCP.Services;

public class BudgetTrackingService
{
    private readonly StorageService _storage;

    public BudgetTrackingService(StorageService storage)
    {
        _storage = storage;
    }

    public BudgetSummary GetBudgetSummary(string eventId)
    {
        var evt = _storage.GetEvent(eventId);
        if (evt is null)
        {
            return new BudgetSummary { Status = "NOT_FOUND" };
        }

        var actual = evt.BudgetBreakdown.Values.Sum();
        var variance = evt.BudgetTotal - actual;
        var variancePercent = evt.BudgetTotal == 0 ? 0 : (variance / evt.BudgetTotal) * 100;

        return new BudgetSummary
        {
            BudgetTotal = evt.BudgetTotal,
            ActualSpent = actual,
            Remaining = variance,
            Variance = variance,
            VariancePercent = variancePercent,
            Breakdown = evt.BudgetBreakdown,
            Status = variance >= 0 ? "ON TRACK" : "OVER BUDGET"
        };
    }

    public void AddCost(string eventId, string category, decimal amount)
    {
        _storage.UpdateEvent(eventId, evt =>
        {
            var breakdown = evt.BudgetBreakdown.ToDictionary(k => k.Key, v => v.Value);
            if (breakdown.ContainsKey(category))
            {
                breakdown[category] += amount;
            }
            else
            {
                breakdown[category] = amount;
            }
            return evt with { BudgetBreakdown = breakdown };
        });
    }

    public BudgetOverrunAlert CheckForOverrun(string eventId)
    {
        var summary = GetBudgetSummary(eventId);
        if (summary.Status == "NOT_FOUND")
        {
            return new BudgetOverrunAlert { Status = "NOT_FOUND" };
        }

        if (summary.VariancePercent >= -5)
        {
            return new BudgetOverrunAlert { Status = "ON_TRACK" };
        }

        return new BudgetOverrunAlert
        {
            Status = "OVER_BUDGET",
            Variance = summary.Variance,
            VariancePercent = summary.VariancePercent,
            Recommendations = new List<string>
            {
                "Review largest cost categories for reductions",
                "Negotiate vendor discounts or adjust scope",
                "Add contingency buffer"
            }
        };
    }

    public List<OptimizationSuggestion> GetOptimizations(string eventId)
    {
        var summary = GetBudgetSummary(eventId);
        if (summary.Status == "NOT_FOUND")
        {
            return new List<OptimizationSuggestion>();
        }

        var suggestions = new List<OptimizationSuggestion>();
        foreach (var (category, amount) in summary.Breakdown.OrderByDescending(k => k.Value))
        {
            suggestions.Add(new OptimizationSuggestion
            {
                Category = category,
                Suggestion = "Evaluate scope or vendor options to trim 5-10%",
                PotentialSavings = amount * 0.1m
            });
        }

        return suggestions;
    }
}

public record BudgetSummary
{
    public decimal BudgetTotal { get; init; }
    public decimal ActualSpent { get; init; }
    public decimal Remaining { get; init; }
    public decimal Variance { get; init; }
    public decimal VariancePercent { get; init; }
    public Dictionary<string, decimal> Breakdown { get; init; } = new();
    public string Status { get; init; } = "ON TRACK";
}

public record BudgetOverrunAlert
{
    public string Status { get; init; } = "ON_TRACK";
    public decimal Variance { get; init; }
    public decimal VariancePercent { get; init; }
    public List<string> Recommendations { get; init; } = new();
}

public record OptimizationSuggestion
{
    public string Category { get; init; } = string.Empty;
    public string Suggestion { get; init; } = string.Empty;
    public decimal PotentialSavings { get; init; }
}
