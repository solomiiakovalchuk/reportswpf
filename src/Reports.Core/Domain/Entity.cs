namespace Reports.Core.Domain;

public abstract class Entity
{
    public int Id { get; set; }

    public abstract string Display { get; }

    public override string ToString() => Display;

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id && Id != 0;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
