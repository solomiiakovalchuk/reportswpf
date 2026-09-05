using System.Globalization;
using Google.Apis.Sheets.v4;
using Reports.Core.Abstractions;
using Reports.Core.Domain;
using Reports.Core.Services;

namespace Reports.Integration;

/// <summary>
/// Читає довідники, людей і відповіді форми з Google-таблиці.
/// <para>
/// Колонки аркуша «Відповіді» шукаються <b>за заголовками</b>, а не за
/// фіксованими номерами: питання у формі можна переставити або додати,
/// і синхронізація не зламається.
/// </para>
/// </summary>
public sealed class GoogleSheetsResponseSource : IResponseSource
{
    private readonly GoogleAuthenticator _authenticator;
    private readonly AppSettings _settings;

    public GoogleSheetsResponseSource(GoogleAuthenticator authenticator, AppSettings settings)
    {
        _authenticator = authenticator;
        _settings = settings;
    }

    public string Title => "Google Таблиця";

    public async Task<RawDirectories> LoadDirectoriesAsync(CancellationToken ct = default)
    {
        using var service = await CreateServiceAsync(ct);

        var directories = await ReadRangeAsync(service, $"{_settings.DirectoriesSheet}!A1:K200", ct);
        var people = await ReadRangeAsync(service, $"{_settings.PeopleSheet}!A1:D200", ct);

        return new RawDirectories
        {
            // Чотири блоки довідників лежать поруч: A|B, D|E, G|H, J|K, дані з рядка 3.
            Uav = ReadPairs(directories, leftColumn: 0),
            Targets = ReadPairs(directories, leftColumn: 3),
            Munitions = ReadPairs(directories, leftColumn: 6),
            Settlements = ReadPairs(directories, leftColumn: 9),
            People = ReadPeople(people)
        };
    }

    public async Task<IReadOnlyList<RawResponseRow>> LoadResponsesAsync(CancellationToken ct = default)
    {
        using var service = await CreateServiceAsync(ct);

        var rows = await ReadRangeAsync(service, $"{_settings.ResponsesSheet}!A1:Z5000", ct);
        if (rows.Count < 2) return Array.Empty<RawResponseRow>();

        var map = ResponseColumnMap.FromHeader(rows[0]);
        var result = new List<RawResponseRow>(rows.Count - 1);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count == 0) continue;

            var submitted = ParseDate(map.Value(row, map.Timestamp)) ?? DateTime.Today;

            result.Add(new RawResponseRow
            {
                // Номер рядка робить ключ стабільним навіть за однакових позначок часу.
                RowKey = $"{map.Value(row, map.Timestamp)}|{i + 1}",
                SubmittedAt = submitted,
                Date = ParseDate(map.Value(row, map.Date)),
                OperatorEmail = map.Value(row, map.Email),
                SettlementName = map.Value(row, map.Settlement),
                UavTypeName = map.Value(row, map.UavType),
                TargetTypeName = map.Value(row, map.TargetType),
                UavCount = ParseCount(map.Value(row, map.UavCount)),
                MunitionNames = map.Munitions
                                   .Select(c => map.Value(row, c))
                                   .Where(v => !string.IsNullOrWhiteSpace(v))
                                   .ToList(),
                StatusMarker = map.Value(row, map.Status)
            });
        }

        return result;
    }

    private async Task<SheetsService> CreateServiceAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_settings.SpreadsheetId))
            throw new GoogleAccessException(
                "Не вказано ідентифікатор Google-таблиці. Відкрийте «Налаштування» і вставте " +
                "його з адреси таблиці: docs.google.com/spreadsheets/d/<ІДЕНТИФІКАТОР>/edit");

        return await _authenticator.CreateServiceAsync(ct);
    }

    private async Task<IList<IList<object>>> ReadRangeAsync(
        SheetsService service, string range, CancellationToken ct)
    {
        try
        {
            var response = await service.Spreadsheets.Values
                .Get(_settings.SpreadsheetId, range)
                .ExecuteAsync(ct);

            return response.Values ?? new List<IList<object>>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not GoogleAccessException)
        {
            throw new GoogleAccessException(
                $"Не вдалося прочитати діапазон «{range}». Перевірте ідентифікатор таблиці, " +
                "назви аркушів у налаштуваннях і доступ вашого акаунта до документа.", ex);
        }
    }

    /// <summary>Читає блок довідника з двох сусідніх колонок, починаючи з рядка 3.</summary>
    private static IReadOnlyList<(string, string)> ReadPairs(IList<IList<object>> rows, int leftColumn)
    {
        var pairs = new List<(string, string)>();

        for (int i = 2; i < rows.Count; i++)
        {
            var left = Cell(rows[i], leftColumn);
            if (string.IsNullOrWhiteSpace(left)) continue;

            pairs.Add((left.Trim(), Cell(rows[i], leftColumn + 1).Trim()));
        }

        return pairs;
    }

    private static IReadOnlyList<Person> ReadPeople(IList<IList<object>> rows)
    {
        var people = new List<Person>();

        for (int i = 1; i < rows.Count; i++)
        {
            var email = Cell(rows[i], 0).Trim();
            var name = Cell(rows[i], 1).Trim();
            if (email.Length == 0 && name.Length == 0) continue;

            people.Add(new Person
            {
                Email = email,
                FullName = name,
                Rank = Cell(rows[i], 2).Trim(),
                Position = Cell(rows[i], 3).Trim()
            });
        }

        return people;
    }

    private static string Cell(IList<object> row, int index) =>
        index >= 0 && index < row.Count ? row[index]?.ToString() ?? string.Empty : string.Empty;

    private static DateTime? ParseDate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        string[] formats =
        {
            "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm", "dd.MM.yyyy",
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "MM/dd/yyyy HH:mm:ss", "M/d/yyyy"
        };

        if (DateTime.TryParseExact(raw.Trim(), formats, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out var exact))
            return exact;

        if (DateTime.TryParse(raw.Trim(), CultureInfo.CurrentCulture,
                              DateTimeStyles.None, out var current))
            return current;

        // Google іноді віддає дату як серійне число Excel.
        if (double.TryParse(raw.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var serial)
            && serial > 20000 && serial < 80000)
            return new DateTime(1899, 12, 30).AddDays(serial);

        return null;
    }

    private static int ParseCount(string raw)
    {
        if (int.TryParse(raw?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var n))
            return Math.Max(0, n);
        return 0;
    }
}
