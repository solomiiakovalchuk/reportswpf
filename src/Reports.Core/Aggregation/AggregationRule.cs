using Reports.Core.Domain;

namespace Reports.Core.Aggregation;

public interface IAggregationRule
{
    string Tag { get; }
    string Evaluate(AggregationContext context);
}

public sealed class AggregationContext
{
    public required ReportRequest Request { get; init; }
    public required IReadOnlyList<Mission> Missions { get; init; }

    public IEnumerable<TargetEngagement> Engagements =>
        Missions.SelectMany(m => m.Targets);

    public bool IsEmpty => Missions.Count == 0;
}

public abstract class AggregationRule : IAggregationRule
{
    public abstract string Tag { get; }

    public abstract string Evaluate(AggregationContext context);

    protected static string JoinList(IEnumerable<string> parts) =>
        string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    protected static IEnumerable<string> DistinctNames<T>(IEnumerable<T?> items)
        where T : ReferenceItem =>
        items.Where(i => i is not null)
             .Select(i => i!.ForDocument())
             .Where(n => !string.IsNullOrWhiteSpace(n))
             .Distinct(StringComparer.CurrentCultureIgnoreCase);
}
