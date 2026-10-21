namespace Reports.Core.Domain;

public enum RecordStatus
{
    Active = 0,

    Cancelled = 1,

    NeedsAttention = 2
}

public static class RecordStatusExtensions
{
    public const string CancelledMarker = "СКАСОВАНО";
    public const string NeedsAttentionMarker = "ПОТРЕБУЄ УВАГИ";

    public static RecordStatus Parse(string? raw) => raw?.Trim().ToUpperInvariant() switch
    {
        null or "" => RecordStatus.Active,
        CancelledMarker => RecordStatus.Cancelled,
        NeedsAttentionMarker => RecordStatus.NeedsAttention,
        _ => RecordStatus.Active
    };

    public static string ToMarker(this RecordStatus status) => status switch
    {
        RecordStatus.Cancelled => CancelledMarker,
        RecordStatus.NeedsAttention => NeedsAttentionMarker,
        _ => string.Empty
    };

    public static string ToDisplay(this RecordStatus status) => status switch
    {
        RecordStatus.Cancelled => "Скасовано",
        RecordStatus.NeedsAttention => "Потребує уваги",
        _ => "Активний"
    };
}
