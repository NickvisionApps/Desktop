using Microsoft.EntityFrameworkCore;
using System;

namespace Nickvision.Desktop.Keyring;

/// <summary>
/// An Entity Framework Core DbContext for the keyring credentials database.
/// </summary>
public class KeyringDbContext : DbContext
{
    /// <summary>
    /// The set of credentials stored in the database.
    /// </summary>
    public DbSet<Credential> Credentials => Set<Credential>();

    /// <summary>
    /// Constructs a KeyringDbContext.
    /// </summary>
    /// <param name="options">The options for this context</param>
    public KeyringDbContext(DbContextOptions<KeyringDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model for the keyring database.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credential>(entity =>
        {
            entity.ToTable("credentials");
            entity.HasKey(c => c.Name);
            entity.Property(c => c.Name).HasColumnName("name").IsRequired();
            entity.Property(c => c.Username).HasColumnName("username").IsRequired();
            entity.Property(c => c.Password).HasColumnName("password").IsRequired();
            entity.Property(c => c.Url)
                .HasColumnName("uri")
                .HasConversion(
                    v => v.ToString(),
                    v => new Uri(v))
                .IsRequired();
        });
    }
}
