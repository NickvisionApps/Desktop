using Microsoft.Data.Sqlite;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Keyring;

public class KeyringService : IAsyncDisposable, IDisposable, IKeyringService
{
    private readonly List<Credential> _credentials;
    private readonly string _path;
    private SqliteConnection? _connection;

    public KeyringService(AppInfo info, ISecretService secretService)
    {
        var keyringDir = Path.Combine(UserDirectories.Config, "Nickvision", "Keyring");
        Directory.CreateDirectory(keyringDir);
        _credentials = [];
        _path = Path.Combine(keyringDir, $"{info.Id}.ring2");
        _connection = null;
#if OS_WINDOWS || OS_MAC || OS_LINUX
        var secret = secretService.Get(info.Id) ?? secretService.Create(info.Id);
        if (secret is not null)
        {
            _connection = new SqliteConnection(
                new SqliteConnectionStringBuilder($"Data Source='{_path}'")
                {
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = secret.Value,
                    Pooling = false
                }.ToString());
            try
            {
                _connection.Open();
            }
            catch (SqliteException)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
#endif
        if (_connection is null)
        {
            return;
        }
        using var createTableCommand = _connection.CreateCommand();
        createTableCommand.CommandText =
            "CREATE TABLE IF NOT EXISTS credentials (name TEXT, uri TEXT, username TEXT, password TEXT)";
        createTableCommand.ExecuteNonQuery();
        using var selectAllCommand = _connection.CreateCommand();
        selectAllCommand.CommandText = "SELECT * FROM credentials";
        using var reader = selectAllCommand.ExecuteReader();
        while (reader.Read())
        {
            _credentials.Add(
                new Credential(
                    reader.GetString(0),
                    reader.GetString(2),
                    reader.GetString(3),
                    new Uri(reader.GetString(1))));
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool IsSavingToDisk => _connection is not null;
    public IEnumerable<Credential> Credentials => _credentials;

    public async Task<bool> AddCredentialAsync(Credential credential)
    {
        if (_credentials.Any(c => c.Name == credential.Name))
        {
            return false;
        }
        _credentials.Add(credential);
        if (_connection is null)
        {
            return false;
        }
        await using var insertCommand = _connection.CreateCommand();
        insertCommand.CommandText =
            "INSERT INTO credentials (name, uri, username, password) VALUES ($name, $uri, $username, $password)";
        insertCommand.Parameters.AddWithValue("$name", credential.Name);
        insertCommand.Parameters.AddWithValue("$uri", credential.Url.ToString());
        insertCommand.Parameters.AddWithValue("$username", credential.Username);
        insertCommand.Parameters.AddWithValue("$password", credential.Password);
        return await insertCommand.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DestroyAsync()
    {
        await DisposeAsync();
        _credentials.Clear();
        File.Delete(_path);
        return !File.Exists(_path);
    }

    public async Task<bool> RemoveCredentialAsync(Credential credential)
    {
        var credentialIndex = _credentials.FindIndex(c => c.Name == credential.Name);
        if (credentialIndex == -1)
        {
            return false;
        }
        _credentials.RemoveAt(credentialIndex);
        if (_connection is null)
        {
            return false;
        }
        await using var deleteCommand = _connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM credentials WHERE name = $name";
        deleteCommand.Parameters.AddWithValue("$name", credential.Name);
        return await deleteCommand.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateCredentialAsync(Credential credential)
    {
        var credentialIndex = _credentials.FindIndex(c => c.Name == credential.Name);
        if (credentialIndex == -1)
        {
            return false;
        }
        _credentials[credentialIndex] = credential;
        if (_connection is null)
        {
            return false;
        }
        await using var updateCommand = _connection.CreateCommand();
        updateCommand.CommandText =
            "UPDATE credentials SET uri = $uri, username = $username, password = $password WHERE name = $name";
        updateCommand.Parameters.AddWithValue("$name", credential.Name);
        updateCommand.Parameters.AddWithValue("$uri", credential.Url.ToString());
        updateCommand.Parameters.AddWithValue("$username", credential.Username);
        updateCommand.Parameters.AddWithValue("$password", credential.Password);
        return await updateCommand.ExecuteNonQueryAsync() > 0;
    }

    ~KeyringService()
    {
        Dispose(false);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
        _connection = null;
    }

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
