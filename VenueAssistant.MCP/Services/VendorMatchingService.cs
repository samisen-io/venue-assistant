using VenueAssistant.MCP.Models;
using VenueAssistant.MCP.Utils;

namespace VenueAssistant.MCP.Services;

public class VendorMatchingService
{
    private readonly StorageService _storage;
    private readonly VendorRankingCalculator _ranker;

    public VendorMatchingService(StorageService storage, VendorRankingCalculator ranker)
    {
        _storage = storage;
        _ranker = ranker;
    }

    public VendorMatches MatchVendors(string eventId, List<string> vendorCategories)
    {
        var evt = _storage.GetEvent(eventId);
        if (evt is null)
        {
            return new VendorMatches { Success = false, EventId = eventId, Matches = new() };
        }

        var matches = new Dictionary<string, List<VendorMatch>>(StringComparer.OrdinalIgnoreCase);
        var budgetPerGuest = evt.GuestCount > 0 ? evt.BudgetTotal / evt.GuestCount : 0;

        foreach (var category in vendorCategories)
        {
            var vendors = _storage.GetVendorsByCategory(evt.VenueId, category).ToList();
            var ranked = vendors
                .Select(v => new VendorMatch
                {
                    Id = v.Id,
                    Name = v.Name,
                    ReliabilityScore = v.ReliabilityScore,
                    EstimatedCost = v.CostPerUnit * evt.GuestCount,
                    OnTimeRate = v.OnTimePercentage,
                    MatchScore = _ranker.CalculateMatchScore(v, budgetPerGuest),
                    Recommendation = "BACKUP"
                })
                .OrderByDescending(v => v.MatchScore)
                .ToList();

            if (ranked.Count > 0)
            {
                ranked[0] = ranked[0] with { Recommendation = "PRIMARY" };
            }

            matches[category] = ranked;
        }

        return new VendorMatches
        {
            Success = true,
            EventId = eventId,
            Matches = matches,
            Summary = GenerateMatchSummary(matches, evt)
        };
    }

    private static string GenerateMatchSummary(Dictionary<string, List<VendorMatch>> matches, Event evt)
    {
        var categories = string.Join(", ", matches.Select(kvp => $"{kvp.Key}: {kvp.Value.Count}"));
        return $"Vendor matches for {evt.EventName} ({evt.EventType}) — {categories}.";
    }
}

public record VendorMatch
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public int ReliabilityScore { get; init; }
    public decimal EstimatedCost { get; init; }
    public decimal OnTimeRate { get; init; }
    public double MatchScore { get; init; }
    public string Recommendation { get; init; } = "BACKUP";
}

public record VendorMatches
{
    public bool Success { get; init; }
    public required string EventId { get; init; }
    public Dictionary<string, List<VendorMatch>> Matches { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public string Summary { get; init; } = string.Empty;
}
