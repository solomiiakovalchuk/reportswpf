namespace Reports.Core.Domain;

public sealed class Mission : Entity
{
    public string SourceRowKey { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public DateTime SubmittedAt { get; set; }

    public string OperatorEmail { get; set; } = string.Empty;

    public int? SettlementId { get; set; }
    public Settlement? Settlement { get; set; }

    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public List<TargetEngagement> Targets { get; set; } = new();

    public override string Display => $"{Date:dd.MM.yyyy} — {Settlement?.ShortName ?? "—"}";

    public int TotalUavCount => Targets.Sum(t => t.UavCount);

    public bool CountsTowardsReport => Status == RecordStatus.Active;
}

public sealed class TargetEngagement : Entity
{
    public int MissionId { get; set; }
    public Mission? Mission { get; set; }

    public int? UavTypeId { get; set; }
    public UavType? UavType { get; set; }

    public int? TargetTypeId { get; set; }
    public TargetType? TargetType { get; set; }

    public int UavCount { get; set; }

    public List<MunitionUse> Munitions { get; set; } = new();

    public override string Display =>
        $"{UavType?.ShortName ?? "?"} × {UavCount} → {TargetType?.ShortName ?? "?"}";
}

public sealed class MunitionUse : Entity
{
    public int TargetEngagementId { get; set; }
    public TargetEngagement? TargetEngagement { get; set; }

    public int? MunitionTypeId { get; set; }
    public MunitionType? MunitionType { get; set; }

    public override string Display => MunitionType?.ShortName ?? "?";
}
