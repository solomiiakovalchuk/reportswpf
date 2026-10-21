namespace Reports.Core.Domain;

public abstract class ReferenceItem : Entity
{
    public string ShortName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public abstract string DirectoryTitle { get; }

    public override string Display => ShortName;

    public virtual string ForDocument() =>
        string.IsNullOrWhiteSpace(FullName) ? ShortName : FullName;
}

public sealed class UavType : ReferenceItem
{
    public override string DirectoryTitle => "БпЛА";
}

public sealed class TargetType : ReferenceItem
{
    public override string DirectoryTitle => "ЦІЛІ";
}

public sealed class MunitionType : ReferenceItem
{
    public override string DirectoryTitle => "БОЄПРИПАСИ";
}

public sealed class Settlement : ReferenceItem
{
    public override string DirectoryTitle => "МІСЦЯ";

    public string Region { get; set; } = string.Empty;

    public override string ForDocument() => ShortName;
}
