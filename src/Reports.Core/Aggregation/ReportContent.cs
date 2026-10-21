namespace Reports.Core.Aggregation;

public sealed class ReportContent
{
    private readonly IReadOnlyDictionary<string, string> _values;

    public ReportContent(IReadOnlyDictionary<string, string> values, int missionCount, int engagementCount)
    {
        _values = values;
        MissionCount = missionCount;
        EngagementCount = engagementCount;
    }

    public int MissionCount { get; }

    public int EngagementCount { get; }

    public bool IsEmpty => MissionCount == 0;

    public IReadOnlyDictionary<string, string> Values => _values;

    public string this[string tag] => _values.TryGetValue(tag, out var v) ? v : string.Empty;

    public string Fill(string template)
    {
        var result = template;
        foreach (var (tag, value) in _values)
            result = result.Replace(tag, value, StringComparison.Ordinal);
        return result;
    }

    public string BuildSentence() => Fill(SentenceTemplate);

    public const string SentenceTemplate =
        "Дійсним доповідаю, що з " + ReportTags.DateFrom + " по " + ReportTags.DateTo +
        " було використано (витрачено) в ході бойових дій " + ReportTags.UavWithCount +
        ", з боєприпасами: " + ReportTags.Munitions +
        ", переобладнаними для виконання спеціальних завдань зі знищення повітряних цілей " +
        ReportTags.Subunit + " по знищенню " + ReportTags.TargetTypes +
        " в районі н.п. " + ReportTags.Settlements + ", " + ReportTags.Regions +
        " області, згідно бойових розпоряджень, бойового наказу " + ReportTags.Order + ".";
}
