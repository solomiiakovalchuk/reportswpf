namespace Reports.Core.Domain;

public sealed class Person : Entity
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public override string Display =>
        string.IsNullOrWhiteSpace(FullName) ? Email : $"{FullName} ({Rank})";

    public bool CanSign =>
        !string.IsNullOrWhiteSpace(FullName)
        && !string.IsNullOrWhiteSpace(Rank)
        && !string.IsNullOrWhiteSpace(Position);
}
