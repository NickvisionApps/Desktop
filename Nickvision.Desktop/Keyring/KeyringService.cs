using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Keyring;

/// <summary>
/// A service for managing credentials in a database keyring.
/// </summary>
public class KeyringService : IAsyncDisposable, IDisposable, IKeyringService
{
    private readonly ILogger<KeyringService> _logger;
    private readonly List<Credential> _credentials;
    private readonly string _path;
    private KeyringDbContext? _context;

    /// <summary>
    /// Constructs a KeyringService.
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="info">The AppInfo object for the app</param>
    /// <param name="secretService">The service for managing secrets</param>
    /// <remarks>This will create a new encrypted database store if it doesn't already exist.</remarks>
    /// <remarks> If the database is unable to be created or unlocked, changes will not be saved to disk.</remarks>
    public KeyringService(ILogger<KeyringService> logger, AppInfo info, ISecretService secretService)
    {
        _logger = logger;
        var keyringDir = Path.Combine(UserDirectories.Config, "Nickvision", "Keyring");
        Directory.CreateDirectory(keyringDir);
        _credentials = [];
        _path = Path.Combine(keyringDir, $"{info.Id}.ring2");
        _context = null;
        _logger.LogInformation($"Opening keyring database ({_path}).");
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            var secret = secretService.Get(info.Id) ?? secretService.Create(info.Id);
            if (secret is not null)
            {
                var connectionString = new SqliteConnectionStringBuilder($"Data Source='{_path}'")
                {
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = secret.Value,
                    Pooling = false
                }.ToString();
                var options = new DbContextOptionsBuilder<KeyringDbContext>()
                    .UseSqlite(connectionString)
                    .Options;
                _context = new KeyringDbContext(options);
                try
                {
                    _context.Database.OpenConnection();
                    _context.Database.EnsureCreated();
                    _logger.LogInformation($"Opened keyring database ({_path}) successfully.");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to open keyring database ({_path}): {e}");
                    _context.Dispose();
                    _context = null;
                }
            }
            else
            {
                _logger.LogError($"Unable to open keyring database ({_path}). The system secret ({info.Id}) could not be retrieved or created.");
            }
        }
        if (_context is null)
        {
            _logger.LogError($"Keyring database ({_path}) connection is unavailable. Changes will not be saved to disk.");
            return;
        }
        _credentials.AddRange(_context.Credentials.AsNoTracking().ToList());
        _logger.LogInformation($"Loaded {_credentials.Count} credentials from keyring.");
    }

    /// <summary>
    /// Finalizes a KeyringService.
    /// </summary>
    ~KeyringService()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes a KeyringService asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes a KeyringService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Whether the keyring is currently saving to disk.
    /// </summary>
    public bool IsSavingToDisk => _context is not null;

    /// <summary>
    /// The list of credentials in the keyring.
    /// </summary>
    public IEnumerable<Credential> Credentials => _credentials;

    /// <summary>
    /// Adds a credential to the keyring.
    /// </summary>
    /// <param name="credential">The credential to add</param>
    /// <returns>True if the keyring was successfully added, else false</returns>
    public async Task<bool> AddCredentialAsync(Credential credential)
    {
        _logger.LogInformation($"Adding keyring credential ({credential.Name}).");
        if (_credentials.Any(c => c.Name == credential.Name))
        {
            _logger.LogError($"Unable to add keyring credential ({credential.Name}) as it already exists.");
            return false;
        }
        _credentials.Add(credential);
        if (_context is null)
        {
            _logger.LogError($"Unable to persist keyring credential ({credential.Name}) to disk as the database connection is unavailable.");
            return false;
        }
        _context.Credentials.Add(credential);
        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Added keyring credential ({credential.Name}) successfully.");
        }
        else
        {
            _logger.LogError($"Failed to add keyring credential ({credential.Name}) to database.");
        }
        // Detach so the in-memory list and the EF change tracker don't diverge
        _context.Entry(credential).State = EntityState.Detached;
        return result;
    }

    /// <summary>
    /// Destroys the keyring and all its credentials.
    /// </summary>
    /// <returns>True if the keyring was successfully destroyed, else false</returns>
    public async Task<bool> DestroyAsync()
    {
        _logger.LogInformation($"Destroying keyring database ({_path}).");
        await DisposeAsync();
        _credentials.Clear();
        File.Delete(_path);
        var result = !File.Exists(_path);
        if (result)
        {
            _logger.LogInformation($"Destroyed keyring database ({_path}) successfully.");
        }
        else
        {
            _logger.LogError($"Failed to destroy keyring database ({_path}).");
        }
        return result;
    }

    /// <summary>
    /// Removes a credential from the keyring.
    /// </summary>
    /// <param name="credential">The credential to remove</param>
    /// <returns>True if the keyring was successfully removed, else false</returns>
    public async Task<bool> RemoveCredentialAsync(Credential credential)
    {
        _logger.LogInformation($"Removing keyring credential ({credential.Name}).");
        var credentialIndex = _credentials.FindIndex(c => c.Name == credential.Name);
        if (credentialIndex == -1)
        {
            _logger.LogError($"Unable to remove keyring credential ({credential.Name}) as it does not exist.");
            return false;
        }
        _credentials.RemoveAt(credentialIndex);
        if (_context is null)
        {
            _logger.LogError($"Unable to remove keyring credential ({credential.Name}) from disk as the database connection is unavailable.");
            return false;
        }
        _context.Credentials.Remove(credential);
        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Removed keyring credential ({credential.Name}) successfully.");
        }
        else
        {
            _logger.LogError($"Failed to remove keyring credential ({credential.Name}) from database.");
        }
        return result;
    }

    /// <summary>
    /// Updates a credential in the keyring.
    /// </summary>
    /// <param name="credential">The credential to update</param>
    /// <returns>True if the keyring was successfully updated, else false</returns>
    public async Task<bool> UpdateCredentialAsync(Credential credential)
    {
        _logger.LogInformation($"Updating keyring credential ({credential.Name}).");
        var credentialIndex = _credentials.FindIndex(c => c.Name == credential.Name);
        if (credentialIndex == -1)
        {
            _logger.LogError($"Unable to update keyring credential ({credential.Name}) as it does not exist.");
            return false;
        }
        _credentials[credentialIndex] = credential;
        if (_context is null)
        {
            _logger.LogError($"Unable to update keyring credential ({credential.Name}) on disk as the database connection is unavailable.");
            return false;
        }
        _context.Credentials.Update(credential);
        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Updated keyring credential ({credential.Name}) successfully.");
        }
        else
        {
            _logger.LogError($"Failed to update keyring credential ({credential.Name}) in database.");
        }
        // Detach so the in-memory list and the EF change tracker don't diverge
        _context.Entry(credential).State = EntityState.Detached;
        return result;
    }

    /// <summary>
    /// Disposes a KeyringService asynchronously.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_context is not null)
        {
            await _context.DisposeAsync().ConfigureAwait(false);
        }
        _context = null;
    }

    /// <summary>
    /// Disposes a KeyringService.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        _context?.Dispose();
        _context = null;
    }
}