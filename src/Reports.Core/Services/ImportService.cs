using Reports.Core.Abstractions;
using Reports.Core.Domain;

namespace Reports.Core.Services;

public sealed record ImportResult(
    int TotalRows,
    int Imported,
    int Skipped,
    int NeedsAttention,
    IReadOnlyList<string> UnknownValues)
{
    public string Describe()
    {
        if (TotalRows == 0)
            return "Джерело не повернуло жодного рядка.";

        var text = $"Прочитано рядків: {TotalRows}. Імпортовано: {Imported}. " +
                   $"Уже були в базі: {Skipped}.";

        if (NeedsAttention > 0)
            text += $" Потребують уваги: {NeedsAttention}.";

        if (UnknownValues.Count > 0)
            text += Environment.NewLine +
                    "Немає в довідниках: " + string.Join(", ", UnknownValues);

        return text;
    }
}

public sealed class ImportService
{
    private readonly IMissionRepository _missions;
    private readonly IReferenceRepository _references;

    public ImportService(IMissionRepository missions, IReferenceRepository references)
    {
        _missions = missions;
        _references = references;
    }

    public async Task<ImportResult> ImportAsync(IResponseSource source, CancellationToken ct = default)
    {
        var rows = await source.LoadResponsesAsync(ct);
        var known = await _missions.GetImportedKeysAsync(ct);

        var uav = Index(await _references.GetUavTypesAsync(ct));
        var targets = Index(await _references.GetTargetTypesAsync(ct));
        var munitions = Index(await _references.GetMunitionTypesAsync(ct));
        var settlements = Index(await _references.GetSettlementsAsync(ct));

        var fresh = new List<Mission>();
        var unknown = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
        int skipped = 0, needsAttention = 0;

        foreach (var row in rows)
        {
            if (known.Contains(row.RowKey))
            {
                skipped++;
                continue;
            }

            var uavType = Lookup(uav, row.UavTypeName, unknown);
            var targetType = Lookup(targets, row.TargetTypeName, unknown);
            var settlement = Lookup(settlements, row.SettlementName, unknown);

            var status = RecordStatusExtensions.Parse(row.StatusMarker);
            if (status == RecordStatus.Active && (uavType is null || targetType is null))
            {
                status = RecordStatus.NeedsAttention;
                needsAttention++;
            }

            var engagement = new TargetEngagement
            {
                UavTypeId = uavType?.Id,
                TargetTypeId = targetType?.Id,
                UavCount = Math.Max(0, row.UavCount)
            };

            foreach (var name in row.MunitionNames)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                var munition = Lookup(munitions, name, unknown);
                if (munition is not null)
                    engagement.Munitions.Add(new MunitionUse { MunitionTypeId = munition.Id });
            }

            fresh.Add(new Mission
            {
                SourceRowKey = row.RowKey,
                Date = (row.Date ?? row.SubmittedAt).Date,
                SubmittedAt = row.SubmittedAt,
                OperatorEmail = row.OperatorEmail.Trim(),
                SettlementId = settlement?.Id,
                Status = status,
                Targets = { engagement }
            });
        }

        if (fresh.Count > 0)
            await _missions.AddRangeAsync(fresh, ct);

        return new ImportResult(rows.Count, fresh.Count, skipped, needsAttention, unknown.ToList());
    }

    private static Dictionary<string, T> Index<T>(IEnumerable<T> items) where T : ReferenceItem =>
        items.GroupBy(i => i.ShortName.Trim(), StringComparer.CurrentCultureIgnoreCase)
             .ToDictionary(g => g.Key, g => g.First(), StringComparer.CurrentCultureIgnoreCase);

    private static T? Lookup<T>(Dictionary<string, T> index, string name, ISet<string> unknown)
        where T : ReferenceItem
    {
        var key = name?.Trim() ?? string.Empty;
        if (key.Length == 0) return null;

        if (index.TryGetValue(key, out var found)) return found;

        unknown.Add(key);
        return null;
    }
}
