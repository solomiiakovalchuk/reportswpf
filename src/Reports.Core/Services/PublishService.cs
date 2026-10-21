using Reports.Core.Abstractions;
using Reports.Core.Domain;

namespace Reports.Core.Services;

public sealed class PublishService
{
    private readonly IReferenceRepository _references;
    private readonly IPersonRepository _people;
    private readonly IMissionRepository _missions;

    public PublishService(
        IReferenceRepository references,
        IPersonRepository people,
        IMissionRepository missions)
    {
        _references = references;
        _people = people;
        _missions = missions;
    }

    public async Task<PublishPayload> CollectAsync(CancellationToken ct = default)
    {
        var uav = await _references.GetUavTypesAsync(ct);
        var targets = await _references.GetTargetTypesAsync(ct);
        var munitions = await _references.GetMunitionTypesAsync(ct);
        var settlements = await _references.GetSettlementsAsync(ct);
        var people = await _people.GetAllAsync(ct);
        var missions = await _missions.GetAllAsync(ct);

        return new PublishPayload
        {
            Uav = Pairs(uav),
            Targets = Pairs(targets),
            Munitions = Pairs(munitions),
            Settlements = settlements
                .Where(s => !string.IsNullOrWhiteSpace(s.ShortName))
                .Select(s => new DirectoryPair(s.ShortName.Trim(), s.Region.Trim()))
                .ToList(),
            People = people.Where(p => !string.IsNullOrWhiteSpace(p.Email)).ToList(),
            Statuses = missions
                .Where(m => !string.IsNullOrWhiteSpace(m.SourceRowKey))
                .Select(m => new StatusChange(m.SourceRowKey, m.Status.ToMarker()))
                .ToList()
        };
    }

    public async Task<PublishResult> PublishAsync(IResponseSink sink, CancellationToken ct = default)
    {
        var payload = await CollectAsync(ct);
        return payload.IsEmpty ? PublishResult.Nothing : await sink.PublishAsync(payload, ct);
    }

    private static IReadOnlyList<DirectoryPair> Pairs<T>(IEnumerable<T> items) where T : ReferenceItem =>
        items.Where(i => !string.IsNullOrWhiteSpace(i.ShortName))
             .Select(i => new DirectoryPair(i.ShortName.Trim(), i.FullName.Trim()))
             .ToList();
}
