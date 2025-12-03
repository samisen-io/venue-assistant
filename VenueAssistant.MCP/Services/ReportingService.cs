using VenueAssistant.MCP.Models;

namespace VenueAssistant.MCP.Services;

public class ReportingService
{
    private readonly StorageService _storage;

    public ReportingService(StorageService storage)
    {
        _storage = storage;
    }

    public PostEventReportResult GenerateReport(string eventId, DateTime completedAt)
    {
        var evt = _storage.GetEvent(eventId);
        if (evt is null)
        {
            return new PostEventReportResult { Success = false, Error = "Event not found." };
        }

        var summary = new EventSummary
        {
            Name = evt.EventName,
            Type = evt.EventType,
            Date = evt.EventDate,
            Guests = evt.GuestCount,
            CompletedAt = completedAt
        };

        var financial = new FinancialSummary
        {
            Budgeted = evt.BudgetTotal,
            Actual = evt.BudgetBreakdown.Values.Sum(),
            Variance = evt.BudgetTotal - evt.BudgetBreakdown.Values.Sum()
        };
        financial.VariancePercent = financial.Budgeted == 0 ? 0 : (financial.Variance / financial.Budgeted) * 100;

        var lessons = ExtractLessonsLearned(evt);
        var recommendations = GenerateRecommendations(evt);

        _storage.UpdateEvent(eventId, e => e with { Status = "completed", CompletedAt = completedAt });

        var report = new PostEventReport
        {
            EventSummary = summary,
            FinancialSummary = financial,
            LessonsLearned = lessons,
            Recommendations = recommendations
        };

        return new PostEventReportResult
        {
            Success = true,
            EventId = eventId,
            Report = report,
            Summary = GenerateReportSummary(report)
        };
    }

    private List<string> ExtractLessonsLearned(Event evt)
    {
        return new List<string>
        {
            $"For future {evt.EventType} events with {evt.GuestCount} guests:",
            "- Book primary vendors early",
            "- Hold timeline walkthrough prior to event",
            "- Maintain 10% budget buffer"
        };
    }

    private Dictionary<string, string> GenerateRecommendations(Event evt)
    {
        return new Dictionary<string, string>
        {
            ["vendor_selection"] = "Review performance and keep top vendors preferred",
            ["timeline"] = "Adjust timeline offsets based on actuals",
            ["budget"] = "Analyze category variances to refine estimates"
        };
    }

    private static string GenerateReportSummary(PostEventReport report)
    {
        return $"Post-event report for {report.EventSummary.Name} ({report.EventSummary.Type}) completed.";
    }
}

public record PostEventReportResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? EventId { get; init; }
    public PostEventReport? Report { get; init; }
    public string Summary { get; init; } = string.Empty;
}

public record PostEventReport
{
    public required EventSummary EventSummary { get; init; }
    public required FinancialSummary FinancialSummary { get; init; }
    public List<string> LessonsLearned { get; init; } = new();
    public Dictionary<string, string> Recommendations { get; init; } = new();
}

public record EventSummary
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public int Guests { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record FinancialSummary
{
    public decimal Budgeted { get; init; }
    public decimal Actual { get; init; }
    public decimal Variance { get; init; }
    public decimal VariancePercent { get; set; }
}
