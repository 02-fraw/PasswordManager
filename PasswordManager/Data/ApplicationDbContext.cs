using Microsoft.EntityFrameworkCore;
using PasswordManager.Models;

namespace PasswordManager.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AccountCredential> AccountCredentials => Set<AccountCredential>();
    public DbSet<RecoveryCodeEntry> RecoveryCodeEntries => Set<RecoveryCodeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountCredential>(entity =>
        {
            entity.ToTable("AccountCredentials");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AccountName)
                .IsRequired()
                .HasMaxLength(160);

            entity.Property(x => x.LoginType)
                .HasConversion<int>()
                .HasDefaultValue(LoginIdentifierType.None);

            entity.Property(x => x.LoginIdentifier)
                .IsRequired()
                .HasMaxLength(200)
                .HasDefaultValue(string.Empty);

            entity.Property(x => x.Password)
                .IsRequired()
                .HasMaxLength(300)
                .HasDefaultValue(string.Empty);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.AccountName);
        });

        modelBuilder.Entity<RecoveryCodeEntry>(entity =>
        {
            entity.ToTable("RecoveryCodeEntries");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceName)
                .IsRequired()
                .HasMaxLength(160);

            entity.Property(x => x.Codes)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(x => x.Notes)
                .IsRequired()
                .HasMaxLength(400)
                .HasDefaultValue(string.Empty);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.ServiceName);
        });
    }
}
