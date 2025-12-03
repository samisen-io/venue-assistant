using VenueAssistant.MCP.Models;
using VenueAssistant.MCP.Utils;

namespace VenueAssistant.MCP.Services;

public class VendorManagementService
{
    private readonly StorageService _storage;

    public VendorManagementService(StorageService storage)
    {
        _storage = storage;
    }

    public Vendor AddVendor(string venueId, VendorCreationRequest request)
    {
        var vendor = new Vendor
        {
            Id = IdGenerator.NewId(),
            VenueId = venueId,
            Name = request.VendorName,
            Category = request.Category,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CostPerUnit = request.CostPerUnit,
            ReliabilityScore = 0,
            TotalEvents = 0,
            OnTimeCount = 0,
            OnTimePercentage = 0,
            AverageQualityRating = 0
        };

        _storage.AddVendor(venueId, vendor);
        return vendor;
    }

    public IEnumerable<Vendor> ListVendors(string venueId, string? category = null)
    {
        return _storage.GetVendorsByCategory(venueId, category);
    }

    public void UpdatePerformance(string venueId, string vendorId, PerformanceUpdate update)
    {
        _storage.UpdateVendor(venueId, vendorId, v =>
        {
            var totalEvents = v.TotalEvents + 1;
            var onTimeCount = v.OnTimeCount + (update.OnTime ? 1 : 0);
            var onTimePercentage = totalEvents == 0 ? 0 : (decimal)onTimeCount / totalEvents * 100;
            var averageQuality = v.TotalEvents == 0
                ? update.QualityRating
                : ((v.AverageQualityRating * v.TotalEvents) + update.QualityRating) / totalEvents;

            var reliability = CalculateReliability(onTimePercentage, averageQuality, totalEvents);

            return v with
            {
                TotalEvents = totalEvents,
                OnTimeCount = onTimeCount,
                OnTimePercentage = Math.Round(onTimePercentage, 2),
                AverageQualityRating = Math.Round(averageQuality, 2),
                ReliabilityScore = reliability,
                Notes = update.Notes ?? v.Notes
            };
        });
    }

    private static int CalculateReliability(decimal onTimePercentage, decimal averageQuality, int totalEvents)
    {
        var reliabilityScore =
            (onTimePercentage * 0.4m) +
            ((averageQuality / 5m) * 100m * 0.4m) +
            (Math.Min(10m, totalEvents / 5m) * 2m);

        return (int)Math.Clamp(Math.Round(reliabilityScore), 0, 100);
    }
}

public record VendorCreationRequest
{
    public required string VendorName { get; init; }
    public required string Category { get; init; }
    public required string ContactEmail { get; init; }
    public required string ContactPhone { get; init; }
    public decimal CostPerUnit { get; init; }
}

public record PerformanceUpdate
{
    public bool OnTime { get; init; }
    public decimal QualityRating { get; init; }
    public bool CostAccurate { get; init; }
    public string? Notes { get; init; }
}
