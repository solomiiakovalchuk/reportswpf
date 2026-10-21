using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Reports.Documents;

public static class TagReplacer
{
    public static void ReplaceInBody(Body body, IReadOnlyDictionary<string, string> values)
    {
        foreach (var paragraph in body.Descendants<Paragraph>())
            ReplaceInParagraph(paragraph, values);
    }

    public static void ReplaceInParagraph(Paragraph paragraph, IReadOnlyDictionary<string, string> values)
    {
        var runs = paragraph.Elements<Run>().ToList();
        if (runs.Count == 0) return;

        var texts = runs.SelectMany(r => r.Elements<Text>()).ToList();
        if (texts.Count == 0) return;

        var original = string.Concat(texts.Select(t => t.Text));
        if (!ContainsAnyTag(original, values)) return;

        var replaced = original;
        foreach (var (tag, value) in values)
            replaced = replaced.Replace(tag, value ?? string.Empty, StringComparison.Ordinal);

        var lines = replaced.Replace("\r\n", "\n").Split('\n');

        var first = texts[0];
        first.Text = lines[0];
        first.Space = SpaceProcessingModeValues.Preserve;

        for (int i = 1; i < texts.Count; i++)
        {
            texts[i].Text = string.Empty;
            texts[i].Space = SpaceProcessingModeValues.Preserve;
        }

        var anchorRun = first.Parent as Run;
        if (anchorRun is null || lines.Length == 1) return;

        var insertAfter = (OpenXmlElement)anchorRun;
        for (int i = 1; i < lines.Length; i++)
        {
            var run = (Run)anchorRun.CloneNode(true);
            run.RemoveAllChildren<Text>();
            run.AppendChild(new Break());
            run.AppendChild(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });

            insertAfter.InsertAfterSelf(run);
            insertAfter = run;
        }
    }

    private static bool ContainsAnyTag(string text, IReadOnlyDictionary<string, string> values)
    {
        foreach (var tag in values.Keys)
            if (text.Contains(tag, StringComparison.Ordinal))
                return true;
        return false;
    }
}
