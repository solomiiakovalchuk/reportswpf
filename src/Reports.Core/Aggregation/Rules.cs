using System.Globalization;
using Reports.Core.Domain;

namespace Reports.Core.Aggregation;

public sealed class DateFromRule : AggregationRule
{
    public override string Tag => ReportTags.DateFrom;
    public override string Evaluate(AggregationContext c) =>
        c.Request.Period.From.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}

public sealed class DateToRule : AggregationRule
{
    public override string Tag => ReportTags.DateTo;
    public override string Evaluate(AggregationContext c) =>
        c.Request.Period.To.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}

public sealed class UavSummaryRule : AggregationRule
{
    public override string Tag => ReportTags.UavWithCount;

    public override string Evaluate(AggregationContext c)
    {
        var groups = c.Engagements
            .Where(e => e.UavType is not null)
            .GroupBy(e => e.UavType!.ForDocument(), StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new { Name = g.Key, Count = g.Sum(e => e.UavCount) })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name, StringComparer.CurrentCulture);

        return JoinList(groups.Select(g => $"{g.Name} — {g.Count} шт."));
    }
}

public sealed class MunitionListRule : AggregationRule
{
    public override string Tag => ReportTags.Munitions;

    public override string Evaluate(AggregationContext c) =>
        JoinList(DistinctNames(c.Engagements
            .SelectMany(e => e.Munitions)
            .Select(m => m.MunitionType)));
}

public sealed class TargetTypeListRule : AggregationRule
{
    public override string Tag => ReportTags.TargetTypes;

    public override string Evaluate(AggregationContext c) =>
        JoinList(DistinctNames(c.Engagements.Select(e => e.TargetType)));
}

public sealed class SettlementListRule : AggregationRule
{
    public override string Tag => ReportTags.Settlements;

    public override string Evaluate(AggregationContext c) =>
        JoinList(DistinctNames(c.Missions.Select(m => m.Settlement)));
}

public sealed class RegionListRule : AggregationRule
{
    public override string Tag => ReportTags.Regions;

    public override string Evaluate(AggregationContext c) =>
        JoinList(c.Missions
            .Select(m => m.Settlement?.Region)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r!.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase));
}

public sealed class SubunitRule : AggregationRule
{
    public override string Tag => ReportTags.Subunit;
    public override string Evaluate(AggregationContext c) => c.Request.Subunit;
}

public sealed class OrderRule : AggregationRule
{
    public override string Tag => ReportTags.Order;
    public override string Evaluate(AggregationContext c) => c.Request.Order;
}

public sealed class SignerPositionRule : AggregationRule
{
    public override string Tag => ReportTags.Position;
    public override string Evaluate(AggregationContext c) => c.Request.Signer.Position;
}

public sealed class SignerRankRule : AggregationRule
{
    public override string Tag => ReportTags.Rank;
    public override string Evaluate(AggregationContext c) => c.Request.Signer.Rank;
}

public sealed class SignerNameRule : AggregationRule
{
    public override string Tag => ReportTags.SignerName;
    public override string Evaluate(AggregationContext c) => c.Request.Signer.FullName;
}
