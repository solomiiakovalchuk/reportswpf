using Microsoft.EntityFrameworkCore;
using Reports.Core.Abstractions;
using Reports.Core.Domain;

namespace Reports.Data.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly SqliteContextFactory _factory;

    public UserRepository(SqliteContextFactory factory) => _factory = factory;

    public async Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task AddAsync(AppUser user, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdatePasswordHashAsync(int userId, string newHash, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var user = await db.Users.FindAsync([userId], ct);
        if (user == null) return;
        user.PasswordHash = newHash;
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Users.AnyAsync(ct);
    }
}
