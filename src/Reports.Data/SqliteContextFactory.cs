using Microsoft.EntityFrameworkCore;

namespace Reports.Data;

public sealed class SqliteContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public SqliteContextFactory(string databasePath)
    {
        DatabasePath = databasePath;

        var folder = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
    }

    public string DatabasePath { get; }

    public AppDbContext CreateDbContext() => new(_options);
}
