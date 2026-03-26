using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Application;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Keyring;

public class KeyringService : IKeyringService
{
    private static readonly string TableName;

    private readonly ILogger<KeyringService> _logger;
    private readonly IDatabaseService _databaseService;
    private bool _tableEnsured;

    static KeyringService()
    {
        TableName = "credentials";
    }

    public KeyringService(ILogger<KeyringService> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _tableEnsured = false;
    }

    public async Task<bool> AddCredentialAsync(Credential credential)
    {
        await EnsureTableAsync();
        _logger.LogInformation($"Adding keyring credential ({credential.Name}).");
        if (await _databaseService.ContainsInTableAsync(TableName, "name", credential.Name))
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
        _logger.LogInformation($"Deleting keyring credential ({credential.Name}).");
        var result = await _databaseService.DeleteFromTableAsync(TableName, "name", credential.Name);
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

    public async Task<IEnumerable<Credential>> GetAllCredentialAsync()
    {
        await EnsureTableAsync();
        var credentials = new List<Credential>();
        await using var command = await _databaseService.SelectAllFromTableAsync(TableName);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            credentials.Add(new Credential(reader.GetString(0), reader.GetString(2), reader.GetString(3), new Uri(reader.GetString(1))));
        }
        return credentials;
    }

    public async Task<bool> UpdateCredentialAsync(Credential credential)
    {
        _logger.LogInformation($"Updating keyring credential ({credential.Name}).");
        if (!await _databaseService.ContainsInTableAsync(TableName, "name", credential.Name))
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
    }
}
