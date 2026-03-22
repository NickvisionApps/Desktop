using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nickvision.Desktop.Keyring;

/// <summary>
/// A design-time factory for KeyringDbContext, used by EF Core tooling (e.g., dotnet-ef dbcontext optimize).
/// </summary>
internal sealed class KeyringDbContextDesignTimeFactory : IDesignTimeDbContextFactory<KeyringDbContext>
{
    /// <summary>
    /// Creates a new instance of KeyringDbContext for use by EF Core tooling.
    /// </summary>
    /// <param name="args">Command-line arguments (unused)</param>
    /// <returns>A new KeyringDbContext configured with an in-memory SQLite database</returns>
    public KeyringDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<KeyringDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        return new KeyringDbContext(options);
    }
}
