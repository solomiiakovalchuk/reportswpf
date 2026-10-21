using Reports.Core.Abstractions;
using Reports.Core.Domain;

namespace Reports.Core.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private AppUser? _current;

    public AuthService(IUserRepository users) => _users = users;

    public AppUser? CurrentUser => _current;
    public bool IsAuthenticated => _current != null;
    public bool IsAdmin => _current?.Role == UserRole.Admin;

    public async Task<bool> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return false;

        var user = await _users.FindByUsernameAsync(username.Trim(), ct);
        if (user == null) return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)
            ? (_current = user) != null
            : false;
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken ct = default)
    {
        if (_current == null || string.IsNullOrEmpty(newPassword)) return false;
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, _current.PasswordHash)) return false;

        var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _users.UpdatePasswordHashAsync(_current.Id, hash, ct);
        _current.PasswordHash = hash;
        return true;
    }

    public async Task ResetPasswordAsync(string username, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(newPassword)) return;
        var user = await _users.FindByUsernameAsync(username.Trim(), ct);
        if (user == null) return;

        var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _users.UpdatePasswordHashAsync(user.Id, hash, ct);
        if (_current?.Id == user.Id) _current.PasswordHash = hash;
    }

    public void Logout() => _current = null;
}
