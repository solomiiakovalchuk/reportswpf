using Microsoft.EntityFrameworkCore;
using Reports.Core.Domain;

namespace Reports.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UavType> UavTypes => Set<UavType>();
    public DbSet<TargetType> TargetTypes => Set<TargetType>();
    public DbSet<MunitionType> MunitionTypes => Set<MunitionType>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<TargetEngagement> TargetEngagements => Set<TargetEngagement>();
    public DbSet<MunitionUse> MunitionUses => Set<MunitionUse>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        ConfigureReference<UavType>(b, "UavTypes");
        ConfigureReference<TargetType>(b, "TargetTypes");
        ConfigureReference<MunitionType>(b, "MunitionTypes");

        b.Entity<Settlement>(e =>
        {
            e.ToTable("Settlements");
            e.HasKey(x => x.Id);
            e.Property(x => x.ShortName).IsRequired().HasMaxLength(200);
            e.Property(x => x.FullName).HasMaxLength(400);
            e.Property(x => x.Region).HasMaxLength(200);
            e.HasIndex(x => x.ShortName).IsUnique();
            e.Ignore(x => x.Display);
            e.Ignore(x => x.DirectoryTitle);
        });

        b.Entity<Person>(e =>
        {
            e.ToTable("People");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(320);
            e.Property(x => x.FullName).HasMaxLength(300);
            e.Property(x => x.Rank).HasMaxLength(150);
            e.Property(x => x.Position).HasMaxLength(400);
            e.HasIndex(x => x.Email).IsUnique();
            e.Ignore(x => x.Display);
            e.Ignore(x => x.CanSign);
        });

        b.Entity<Mission>(e =>
        {
            e.ToTable("Missions");
            e.HasKey(x => x.Id);
            e.Property(x => x.SourceRowKey).IsRequired().HasMaxLength(200);
            e.Property(x => x.OperatorEmail).HasMaxLength(320);
            e.Property(x => x.Status).HasConversion<int>();

            e.HasIndex(x => x.SourceRowKey).IsUnique();
            e.HasIndex(x => x.Date);

            e.HasOne(x => x.Settlement)
             .WithMany()
             .HasForeignKey(x => x.SettlementId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(x => x.Targets)
             .WithOne(x => x.Mission!)
             .HasForeignKey(x => x.MissionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Ignore(x => x.Display);
            e.Ignore(x => x.TotalUavCount);
            e.Ignore(x => x.CountsTowardsReport);
        });

        b.Entity<TargetEngagement>(e =>
        {
            e.ToTable("TargetEngagements");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.UavType)
             .WithMany()
             .HasForeignKey(x => x.UavTypeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.TargetType)
             .WithMany()
             .HasForeignKey(x => x.TargetTypeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(x => x.Munitions)
             .WithOne(x => x.TargetEngagement!)
             .HasForeignKey(x => x.TargetEngagementId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Ignore(x => x.Display);
        });

        b.Entity<MunitionUse>(e =>
        {
            e.ToTable("MunitionUses");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.MunitionType)
             .WithMany()
             .HasForeignKey(x => x.MunitionTypeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Ignore(x => x.Display);
        });

        b.Entity<JournalEntry>(e =>
        {
            e.ToTable("JournalEntries");
            e.HasKey(x => x.Id);
            e.Property(x => x.SignerName).HasMaxLength(300);
            e.Property(x => x.FilePath).HasMaxLength(1000);
            e.Ignore(x => x.Display);
        });

        b.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(100);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(100);
            e.Property(x => x.Role).HasConversion<int>();
            e.HasIndex(x => x.Username).IsUnique();
            e.Ignore(x => x.Display);
        });
    }

    private static void ConfigureReference<T>(ModelBuilder b, string table) where T : ReferenceItem
    {
        b.Entity<T>(e =>
        {
            e.ToTable(table);
            e.HasKey(x => x.Id);
            e.Property(x => x.ShortName).IsRequired().HasMaxLength(200);
            e.Property(x => x.FullName).HasMaxLength(400);
            e.HasIndex(x => x.ShortName).IsUnique();
            e.Ignore(x => x.Display);
            e.Ignore(x => x.DirectoryTitle);
        });
    }
}
