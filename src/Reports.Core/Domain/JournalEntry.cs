namespace Reports.Core.Domain;

public sealed class JournalEntry : Entity
{
    public DateTime GeneratedAt { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }

    public string SignerName { get; set; } = string.Empty;
    public string OperatorFilter { get; set; } = string.Empty;
    public string SettlementFilter { get; set; } = string.Empty;
    public string Order { get; set; } = string.Empty;

    public int MissionCount { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public override string Display =>
        $"{GeneratedAt:dd.MM.yyyy HH:mm} — {PeriodFrom:dd.MM} … {PeriodTo:dd.MM} ({SignerName})";
}
