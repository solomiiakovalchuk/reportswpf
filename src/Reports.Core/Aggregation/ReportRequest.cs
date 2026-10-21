using Reports.Core.Domain;
using Reports.Core.Periods;

namespace Reports.Core.Aggregation;

public enum ReportFormat
{
    Word = 0,
    Pdf = 1
}

public sealed class ReportRequest
{
    public required DatePeriod Period { get; init; }

    public required Person Signer { get; init; }

    public string? OperatorEmail { get; init; }

    public int? SettlementId { get; init; }

    public string Order { get; init; } = string.Empty;

    public string Subunit { get; init; } = string.Empty;

    public ReportFormat Format { get; init; } = ReportFormat.Word;

    public bool Matches(Mission mission)
    {
        if (!mission.CountsTowardsReport) return false;
        if (!Period.Contains(mission.Date)) return false;

        if (!string.IsNullOrWhiteSpace(OperatorEmail) &&
            !string.Equals(mission.OperatorEmail, OperatorEmail, StringComparison.OrdinalIgnoreCase))
            return false;

        if (SettlementId is { } id && mission.SettlementId != id)
            return false;

        return true;
    }
}
