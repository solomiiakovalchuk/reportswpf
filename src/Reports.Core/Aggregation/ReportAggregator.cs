using Reports.Core.Domain;

namespace Reports.Core.Aggregation;

public sealed class ReportAggregator
{
    private readonly IReadOnlyList<IAggregationRule> _rules;

    public ReportAggregator(IEnumerable<IAggregationRule> rules)
    {
        _rules = rules.ToList();
        if (_rules.Count == 0)
            throw new ArgumentException("Набір правил агрегації порожній.", nameof(rules));
    }

    public static ReportAggregator CreateDefault() => new(new IAggregationRule[]
    {
        new DateFromRule(),
        new DateToRule(),
        new UavSummaryRule(),
        new MunitionListRule(),
        new TargetTypeListRule(),
        new SettlementListRule(),
        new RegionListRule(),
        new SubunitRule(),
        new OrderRule(),
        new SignerPositionRule(),
        new SignerRankRule(),
        new SignerNameRule()
    });

    public ReportContent Aggregate(ReportRequest request, IEnumerable<Mission> allMissions)
    {
        var selected = allMissions.Where(request.Matches)
                                  .OrderBy(m => m.Date)
                                  .ToList();

        var context = new AggregationContext { Request = request, Missions = selected };

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rule in _rules)
            values[rule.Tag] = rule.Evaluate(context) ?? string.Empty;

        return new ReportContent(values, selected.Count, context.Engagements.Count());
    }
}
