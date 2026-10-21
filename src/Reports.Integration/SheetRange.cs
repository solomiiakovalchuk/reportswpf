namespace Reports.Integration;

public static class SheetRange
{
    public static string Of(string sheet, string a1)
    {
        var name = (sheet ?? string.Empty).Trim().Replace("'", "''");
        return $"'{name}'!{a1}";
    }

    public static string Column(int index)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

        var letters = string.Empty;
        for (int n = index; ; n = n / 26 - 1)
        {
            letters = (char)('A' + n % 26) + letters;
            if (n < 26) break;
        }

        return letters;
    }

    public static string Cell(string sheet, int columnIndex, int rowNumber) =>
        Of(sheet, $"{Column(columnIndex)}{rowNumber}");
}
