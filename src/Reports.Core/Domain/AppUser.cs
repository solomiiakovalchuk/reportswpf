namespace Reports.Core.Domain;

public enum UserRole { Admin, Viewer }

public sealed class AppUser : Entity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;

    public override string Display => Username;
}
