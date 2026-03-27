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

public class KeyringService : IKeyringService
{
    private static readonly string TableName;

    private readonly ILogger<KeyringService> _logger;
    private readonly AppInfo _appInfo;
    private readonly IDatabaseService _databaseService;
    private readonly ISecretService _secretService;
    private readonly List<Credential> _credentials;
    private bool _tableEnsured;

    static KeyringService()
    {
        TableName = "credentials";
    }

    public KeyringService(ILogger<KeyringService> logger, AppInfo appInfo, IDatabaseService databaseService, ISecretService secretService)
    {
        _logger = logger;
        _appInfo = appInfo;
        _databaseService = databaseService;
        _secretService = secretService;
        _credentials = [];
        _tableEnsured = false;
    }

    public async Task<bool> AddCredentialAsync(Credential credential)
    {
        await EnsureTableAsync();
        _logger.LogInformation($"Adding keyring credential ({credential.Name})...");
        if (_credentials.Any(c => c.Name == credential.Name))
        {
            _logger.LogError($"Unable to add keyring credential ({credential.Name}) as it already exists.");
            return false;
        }
        var result = _databaseService.InsertIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", credential.Name },
            { "uri", credential.Url.ToString() },
            { "username", credential.Username },
            { "password", credential.Password },
        });
        if (result)
        {
            _credentials.Add(credential);
            _logger.LogInformation($"Added keyring credential ({credential.Name}) successfully.");
        }
        else
        {
            _logger.LogError($"Failed to add keyring credential ({credential.Name}) to database.");
        }
        return result;
    }

    public async Task<bool> DeleteCredentialAsync(Credential credential)
    {
        await EnsureTableAsync();
        _logger.LogInformation($"Deleting keyring credential ({credential.Name})...");
        var result = await _databaseService.DeleteFromTableAsync(TableName, "name", credential.Name);
        if (result)
        {
            _credentials.Remove(credential);
            _logger.LogInformation($"Removed keyring credential ({credential.Name}) successfully.");
        }
        else
        {
            _logger.LogError($"Failed to remove keyring credential ({credential.Name}) from database.");
        }
        return result;
    }

    public async Task<IReadOnlyList<Credential>> GetAllCredentialAsync()
    {
        await EnsureTableAsync();
        return _credentials;
    }

    public async Task<bool> UpdateCredentialAsync(Credential credential)
    {
        _logger.LogInformation($"Updating keyring credential ({credential.Name})...");
        var index = _credentials.IndexOf(credential);
        if (index == -1)
        {
            _logger.LogError($"Unable to update keyring credential ({credential.Name}) as it does not exist.");
            return false;
        }
        var result = await _databaseService.UpdateInTableAsync(TableName, "name", credential.Name, new Dictionary<string, object>()
        {
            { "uri", credential.Url.ToString() },
            { "username", credential.Username },
            { "password", credential.Password },
        });
        if (result)
        {
            _credentials[index] = credential;
            _logger.LogInformation($"Updated keyring credential ({credential.Name}) successfully.");
        }
        else
        {
            _logger.LogError($"Failed to update keyring credential ({credential.Name}) in database.");
        }
        return result;
    }

    private async Task EnsureTableAsync()
    {
        if (_tableEnsured)
        {
            return;
        }
        await _databaseService.EnsureTableExistsAsync(TableName, "name TEXT PRIMARY KEY, uri TEXT, username TEXT, password TEXT");
        _tableEnsured = true;
        await using var commandAll = await _databaseService.SelectAllFromTableAsync(TableName);
        await using var readerAll = await commandAll.ExecuteReaderAsync();
        while (await readerAll.ReadAsync())
        {
            _credentials.Add(new Credential(readerAll.GetString(0), readerAll.GetString(2), readerAll.GetString(3), new Uri(readerAll.GetString(1))));
        }
        var ring2Path = Path.Combine(UserDirectories.Config, "Nickvision", "Keyring", $"{_appInfo.Id}.ring2");
        if(File.Exists(ring2Path))
        {
            _logger.LogInformation($"Found old keyring ring2 file ({ring2Path}), migrating credentials...");
            var secret = await _secretService.GetAsync(_appInfo.Id);
            if(secret is null)
            {
                _logger.LogError($"Unable to migrate old keyring ring2 file ({ring2Path}) as no secret were found for the app.");
                File.Delete(ring2Path);
                _logger.LogInformation($"Deleted old keyring ring2 file ({ring2Path}).");
                return;
            }
            var oldCredentialDb = new SqliteConnection(new SqliteConnectionStringBuilder($"Data Source='{ring2Path}'")
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = secret.Value,
                Pooling = false
            }.ToString());
            try
            {
                await oldCredentialDb.OpenAsync();
                await using var command = oldCredentialDb.CreateCommand();
                command.CommandText = $"SELECT name, uri, username, password FROM {TableName}";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var credential = new Credential(reader.GetString(0), reader.GetString(2), reader.GetString(3), new Uri(reader.GetString(1)));
                    await AddCredentialAsync(credential);
                }
                await oldCredentialDb.CloseAsync();
                _logger.LogInformation($"Migrated keyring credentials from ring2 file ({ring2Path}) successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to migrate keyring credentials from ring2 file ({ring2Path}): {ex}");
            }
            await oldCredentialDb.DisposeAsync();
            File.Delete(ring2Path);
            _logger.LogInformation($"Deleted old keyring ring2 file ({ring2Path}).");
        }
    }
}
