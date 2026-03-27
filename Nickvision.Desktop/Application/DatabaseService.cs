using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public class DatabaseService : IAsyncDisposable, IDisposable, IDatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly ISecretService _secretService;
    private readonly AppInfo _appInfo;
    private SqliteConnection? _connection;

    public event EventHandler<PasswordRequiredEventArgs>? PasswordRequired;

    public DatabaseService(ILogger<DatabaseService> logger, AppInfo appInfo, ISecretService secretService)
    {
        _logger = logger;
        _secretService = secretService;
        _appInfo = appInfo;
        _connection = null;
    }

    ~DatabaseService()
    {
        Dispose(false);
    }

    public bool ContainsInTable(string tableName, string columnName, string matchingValue)
    {
        EnsureDatabase();
        _logger.LogInformation($"Checking if {tableName} contains value in column ({columnName})...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = $param";
        command.Parameters.AddWithValue("$param", matchingValue);
        using var reader = command.ExecuteReader();
        var result = false;
        while (reader.Read())
        {
            if (reader.GetInt32(0) >= 1)
            {
                result = true;
                break;
            }
        }
        if (result)
        {
            _logger.LogInformation($"Found matching column ({columnName}) value in {tableName}.");
        }
        else
        {
            _logger.LogInformation($"Failed to find matching column ({columnName}) value in {tableName}.");
        }
        return result;
    }

    public async Task<bool> ContainsInTableAsync(string tableName, string columnName, string matchingValue)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Checking if {tableName} contains value in column ({columnName})...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = $param";
        command.Parameters.AddWithValue("$param", matchingValue);
        await using var reader = await command.ExecuteReaderAsync();
        var result = false;
        while (await reader.ReadAsync())
        {
            if (reader.GetInt32(0) >= 1)
            {
                result = true;
                break;
            }
        }
        if (result)
        {
            _logger.LogInformation($"Found matching column ({columnName}) value in {tableName}.");
        }
        else
        {
            _logger.LogInformation($"Failed to find matching column ({columnName}) value in {tableName}.");
        }
        return result;
    }

    public SqliteTransaction CreateTransation()
    {
        EnsureDatabase();
        _logger.LogInformation("Created database transaction.");
        return _connection!.BeginTransaction();
    }

    public async Task<SqliteTransaction> CreateTransationAsync()
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation("Created database transaction.");
        return _connection!.BeginTransaction();
    }

    public bool DeleteFromTable(string tableName, string columnName, string matchingValue)
    {
        EnsureDatabase();
        _logger.LogInformation($"Deleting row from {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName} WHERE {columnName} = $param";
        command.Parameters.AddWithValue("$param", matchingValue);
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogInformation($"Deleted row from {tableName} successfully.");

        }
        else
        {
            _logger.LogError($"Failed to delete row from {tableName}.");
        }
        return result;
    }

    public async Task<bool> DeleteFromTableAsync(string tableName, string columnName, string matchingValue)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Deleting row from {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName} WHERE {columnName} = $param";
        command.Parameters.AddWithValue("$param", matchingValue);
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Deleted row from {tableName} successfully.");

        }
        else
        {
            _logger.LogError($"Failed to delete row from {tableName}.");
        }
        return result;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    public bool DropTable(string tableName)
    {
        EnsureDatabase();
        _logger.LogInformation($"Dropping table ({tableName})...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS {tableName}";
        command.ExecuteNonQuery();
        _logger.LogInformation($"Dropped table ({tableName}).");
        return true;
    }

    public async Task<bool> DropTableAsync(string tableName)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Dropping table ({tableName})...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS {tableName}";
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation($"Dropped table ({tableName}).");
        return true;
    }

    public bool EnsureTableExists(string tableName, string layout)
    {
        EnsureDatabase();
        _logger.LogInformation($"Ensuring table ({tableName}) exists...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({layout})";
        command.ExecuteNonQuery();
        _logger.LogInformation($"Table ({tableName}) exists.");
        return true;
    }

    public async Task<bool> EnsureTableExistsAsync(string tableName, string layout)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Ensuring table ({tableName}) exists...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({layout})";
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation($"Table ({tableName}) exists.");
        return true;
    }

    public bool InsertIntoTable(string tableName, Dictionary<string, object> data)
    {
        EnsureDatabase();
        _logger.LogInformation($"Insering data into {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogInformation($"Inserted data into {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to insert data into {tableName}.");
        }
        return result;
    }

    public async Task<bool> InsertIntoTableAsync(string tableName, Dictionary<string, object> data)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Insering data into {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Inserted data into {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to insert data into {tableName}.");
        }
        return result;
    }

    public bool ReplaceIntoTable(string tableName, Dictionary<string, object> data)
    {
        EnsureDatabase();
        _logger.LogInformation($"Replacing data into {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT OR REPLACE INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogInformation($"Inserted data into {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to insert data into {tableName}.");
        }
        return result;
    }

    public async Task<bool> ReplaceIntoTableAsync(string tableName, Dictionary<string, object> data)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Insering data into {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT OR REPLACE INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Inserted data into {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to insert data into {tableName}.");
        }
        return result;
    }

    public SqliteCommand SelectFromTable(string tableName, string columnName, string matchingValue)
    {
        EnsureDatabase();
        _logger.LogInformation($"Selecting data from table {tableName} with matching column ({columnName})...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} WHERE {columnName} = $param";
        command.Parameters.AddWithValue("$param", matchingValue);
        _logger.LogInformation($"Selected data from table {tableName} with matching column ({columnName}).");
        return command;
    }

    public async Task<SqliteCommand> SelectFromTableAsync(string tableName, string columnName, string matchingValue)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Selecting data from table {tableName} with matching column ({columnName})...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} WHERE {columnName} = $param";
        command.Parameters.AddWithValue("$param", matchingValue);
        _logger.LogInformation($"Selected data from table {tableName} with matching column ({columnName}).");
        return command;
    }

    public SqliteCommand SelectAllFromTable(string tableName)
    {
        EnsureDatabase();
        _logger.LogInformation($"Selecting all data from table {tableName}...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";
        _logger.LogInformation($"Selected all data from table {tableName}.");
        return command;
    }

    public async Task<SqliteCommand> SelectAllFromTableAsync(string tableName)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Selecting all data from table {tableName}...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";
        _logger.LogInformation($"Selected all data from table {tableName}.");
        return command;
    }

    public bool UpdateInTable(string tableName, string columnName, string matchingValue, Dictionary<string, object> newData)
    {
        EnsureDatabase();
        _logger.LogInformation($"Updating data in {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"UPDATE {tableName} SET {string.Join(", ", newData.Keys.Select(k => $"{k} = ${k}"))}";
        foreach (var pair in newData)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogInformation($"Updated data in {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to update data in {tableName}.");
        }
        return result;
    }

    public async Task<bool> UpdateInTableAsync(string tableName, string columnName, string matchingValue, Dictionary<string, object> newData)
    {
        await EnsureDatabaseAsync();
        _logger.LogInformation($"Updating data in {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"UPDATE {tableName} SET {string.Join(", ", newData.Keys.Select(k => $"{k} = ${k}"))}";
        foreach (var pair in newData)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogInformation($"Updated data in {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to update data in {tableName}.");
        }
        return result;
    }

    private void EnsureDatabase()
    {
        if (_connection is not null)
        {
            return;
        }
        var path = Path.Combine(_appInfo.IsPortable ? System.Environment.ExecutingDirectory : Path.Combine(UserDirectories.Config, _appInfo.Name), "app.db");
        _logger.LogInformation($"Opening application database ({path})...");
        var secret = string.Empty;
        if (!_appInfo.IsPortable && (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()))
        {
            secret = ((Task.Run(() => _secretService.GetAsync(_appInfo.Id)).GetAwaiter().GetResult()) ?? (Task.Run(() => _secretService.CreateAsync(_appInfo.Id)).GetAwaiter().GetResult()))?.Value;
        }
        while (string.IsNullOrEmpty(secret))
        {
            _logger.LogInformation("Empty secret value. Sending password required event...");
            var args = new PasswordRequiredEventArgs();
            PasswordRequired?.Invoke(this, args);
            secret = args.Password;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder($"Data Source='{path}'")
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = secret,
            Pooling = false
        }.ToString());
        try
        {
            _connection.Open();
            _logger.LogInformation($"Opened application database ({path}).");
        }
        catch (SqliteException e)
        {
            _logger.LogError($"Failed to open application database: {e}");
            _connection.Dispose();
            _connection = null;
        }
    }

    private async Task EnsureDatabaseAsync()
    {
        if (_connection is not null)
        {
            return;
        }
        var path = Path.Combine(_appInfo.IsPortable ? System.Environment.ExecutingDirectory : Path.Combine(UserDirectories.Config, _appInfo.Name), "app.db");
        _logger.LogInformation($"Opening application database ({path})...");
        var secret = string.Empty;
        if (!_appInfo.IsPortable && (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()))
        {
            secret = ((await _secretService.GetAsync(_appInfo.Id)) ?? (await _secretService.CreateAsync(_appInfo.Id)))?.Value;
        }
        while (string.IsNullOrEmpty(secret))
        {
            _logger.LogInformation("Empty secret value. Sending password required event...");
            var args = new PasswordRequiredEventArgs();
            PasswordRequired?.Invoke(this, args);
            secret = args.Password;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder($"Data Source='{path}'")
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = secret,
            Pooling = false
        }.ToString());
        try
        {
            await _connection.OpenAsync();
            _logger.LogInformation($"Opened application database ({path}).");
        }
        catch (SqliteException e)
        {
            _logger.LogError($"Failed to open application database: {e}");
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
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
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
    }
}
