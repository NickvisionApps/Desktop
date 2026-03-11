using Microsoft.Data.Sqlite;
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
    private SqliteConnection? _connection;

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
        _connection = null;
        _logger.LogInformation($"Opening keyring database at {_path}...");
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            var secret = secretService.Get(info.Id) ?? secretService.Create(info.Id);
            if (secret is not null)
            {
                _connection = new SqliteConnection(new SqliteConnectionStringBuilder($"Data Source='{_path}'")
                {
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = secret.Value,
                    Pooling = false
                }.ToString());
                try
                {
                    _connection.Open();
                    _logger.LogInformation($"Opened keyring database at {_path} successfully.");
                }
                catch (SqliteException e)
                {
                    _logger.LogError($"Failed to open keyring database at {_path}: {e}");
                    _connection.Dispose();
                    _connection = null;
                }
            }
            else
            {
                _logger.LogWarning($"Unable to retrieve or create secret for {info.Id}. Keyring will not be saved to disk.");
            }
        }
        if (_connection is null)
        {
            _logger.LogWarning($"Keyring database connection is unavailable. Changes will not be saved to disk.");
            return;
        }
        using var createTableCommand = _connection.CreateCommand();
        createTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS credentials (name TEXT, uri TEXT, username TEXT, password TEXT)";
        createTableCommand.ExecuteNonQuery();
        using var selectAllCommand = _connection.CreateCommand();
        selectAllCommand.CommandText = "SELECT * FROM credentials";
        using var reader = selectAllCommand.ExecuteReader();
        while (reader.Read())
        {
            _credentials.Add(new Credential(reader.GetString(0), reader.GetString(2), reader.GetString(3), new Uri(reader.GetString(1))));
        }
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
    public bool IsSavingToDisk => _connection is not null;

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
        _logger.LogInformation($"Adding credential {credential.Name} to keyring...");
        if (_credentials.Any(c => c.Name == credential.Name))
        {
            _logger.LogWarning($"Credential {credential.Name} already exists in keyring.");
            return false;
        }
        _credentials.Add(credential);
        if (_connection is null)
        {
            _logger.LogWarning($"Keyring database connection is unavailable. Credential {credential.Name} will not be persisted to disk.");
            return false;
        }
        await using var insertCommand = _connection.CreateCommand();
        insertCommand.CommandText = "INSERT INTO credentials (name, uri, username, password) VALUES ($name, $uri, $username, $password)";
        insertCommand.Parameters.AddWithValue("$name", credential.Name);
        insertCommand.Parameters.AddWithValue("$uri", credential.Url.ToString());
        insertCommand.Parameters.AddWithValue("$username", credential.Username);
        insertCommand.Parameters.AddWithValue("$password", credential.Password);
        var result = await insertCommand.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Added credential {credential.Name} to keyring successfully.");
        }
        else
        {
            _logger.LogError($"Failed to persist credential {credential.Name} to keyring database.");
        }
        return result;
    }

    /// <summary>
    /// Destroys the keyring and all its credentials.
    /// </summary>
    /// <returns>True if the keyring was successfully added, else false</returns>
    public async Task<bool> DestroyAsync()
    {
        _logger.LogInformation($"Destroying keyring database at {_path}...");
        await DisposeAsync();
        _credentials.Clear();
        File.Delete(_path);
        var result = !File.Exists(_path);
        if (result)
        {
            _logger.LogInformation($"Destroyed keyring database at {_path} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to destroy keyring database at {_path}.");
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
        _logger.LogInformation($"Removing credential {credential.Name} from keyring...");
        var credentialIndex = _credentials.FindIndex(c => c.Name == credential.Name);
        if (credentialIndex == -1)
        {
            _logger.LogWarning($"Credential {credential.Name} not found in keyring.");
            return false;
        }
        _credentials.RemoveAt(credentialIndex);
        if (_connection is null)
        {
            _logger.LogWarning($"Keyring database connection is unavailable. Credential {credential.Name} will not be removed from disk.");
            return false;
        }
        await using var deleteCommand = _connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM credentials WHERE name = $name";
        deleteCommand.Parameters.AddWithValue("$name", credential.Name);
        var result = await deleteCommand.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Removed credential {credential.Name} from keyring successfully.");
        }
        else
        {
            _logger.LogError($"Failed to remove credential {credential.Name} from keyring database.");
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
        _logger.LogInformation($"Updating credential {credential.Name} in keyring...");
        var credentialIndex = _credentials.FindIndex(c => c.Name == credential.Name);
        if (credentialIndex == -1)
        {
            _logger.LogWarning($"Credential {credential.Name} not found in keyring.");
            return false;
        }
        _credentials[credentialIndex] = credential;
        if (_connection is null)
        {
            _logger.LogWarning($"Keyring database connection is unavailable. Credential {credential.Name} will not be updated on disk.");
            return false;
        }
        await using var updateCommand = _connection.CreateCommand();
        updateCommand.CommandText = "UPDATE credentials SET uri = $uri, username = $username, password = $password WHERE name = $name";
        updateCommand.Parameters.AddWithValue("$name", credential.Name);
        updateCommand.Parameters.AddWithValue("$uri", credential.Url.ToString());
        updateCommand.Parameters.AddWithValue("$username", credential.Username);
        updateCommand.Parameters.AddWithValue("$password", credential.Password);
        var result = await updateCommand.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Updated credential {credential.Name} in keyring successfully.");
        }
        else
        {
            _logger.LogError($"Failed to update credential {credential.Name} in keyring database.");
        }
        return result;
    }

    /// <summary>
    /// Disposes a KeyringService asynchronously.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
        _connection = null;
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
        _connection?.Dispose();
        _connection = null;
    }
}
