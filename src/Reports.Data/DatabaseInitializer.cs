using Microsoft.EntityFrameworkCore;
using Reports.Core.Domain;

namespace Reports.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureCreatedAsync(
        IDbContextFactory<AppDbContext> factory,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        await db.Database.EnsureCreatedAsync(ct);

        // Add Users table to existing databases that predate auth feature
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "Users" (
                "Id"           INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
                "Username"     TEXT    NOT NULL,
                "PasswordHash" TEXT    NOT NULL,
                "Role"         INTEGER NOT NULL DEFAULT 1
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users" ("Username");
            """, ct);

        if (!await db.Users.AnyAsync(ct))
        {
            db.Users.Add(new AppUser
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Role = UserRole.Admin
            });
            await db.SaveChangesAsync(ct);
        }

        if (await db.UavTypes.AnyAsync(ct)) return;

        db.UavTypes.AddRange(
            new UavType { ShortName = "FPV", FullName = "FPV-дрон" },
            new UavType { ShortName = "Кажан", FullName = "дрон «Кажан»" },
            new UavType { ShortName = "Мавік", FullName = "БпЛА типу Mavic" });

        db.TargetTypes.AddRange(
            new TargetType { ShortName = "жива сила", FullName = "живої сили противника" },
            new TargetType { ShortName = "укріплення", FullName = "укріплення противника" },
            new TargetType { ShortName = "автомобіль", FullName = "автомобільної техніки противника" },
            new TargetType { ShortName = "розвід. БпЛА", FullName = "розвідувальних безпілотних літальних апаратів противника" });

        db.MunitionTypes.AddRange(
            new MunitionType { ShortName = "ТШ-200", FullName = "тротилова шашка 200 г" },
            new MunitionType { ShortName = "ВОГ-17", FullName = "ВОГ-17" },
            new MunitionType { ShortName = "ЕД-8", FullName = "електродетонатор ЕД-8" });

        db.Settlements.AddRange(
            new Settlement { ShortName = "Ліски", Region = "Харківська" },
            new Settlement { ShortName = "Вербове", Region = "Запорізька" },
            new Settlement { ShortName = "Ямпіль", Region = "Донецька" });

        db.People.AddRange(
            new Person
            {
                Email = "commander@example.com",
                FullName = "О. КОВАЛЕНКО",
                Rank = "майор",
                Position = "Командир зенітної батареї військової частини А0000"
            },
            new Person
            {
                Email = "operator1@example.com",
                FullName = "П. ШЕВЧЕНКО",
                Rank = "сержант",
                Position = "Оператор БпЛА"
            });

        await db.SaveChangesAsync(ct);
    }
}
