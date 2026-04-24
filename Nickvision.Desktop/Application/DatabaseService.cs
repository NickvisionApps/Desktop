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

    public bool IsEncrypted { get; private set; }

    public DatabaseService(ILogger<DatabaseService> logger, AppInfo appInfo, ISecretService secretService)
    {
        _logger = logger;
        _secretService = secretService;
        _appInfo = appInfo;
        _connection = null;
        IsEncrypted = false;
    }

    ~DatabaseService()
    {
        Dispose(false);
    }

    public bool ClearTable(string tableName)
    {
        EnsureDatabase();
        _logger.LogDebug($"Clearing table {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName}";
        command.ExecuteNonQuery();
        _logger.LogDebug($"Cleared table {tableName}.");
        return true;
    }

    public async Task<bool> ClearTableAsync(string tableName)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Clearing table {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName}";
        await command.ExecuteNonQueryAsync();
        _logger.LogDebug($"Cleared table {tableName}.");
        return true;
    }

    public int CountInTable(string tableName)
    {
        EnsureDatabase();
        _logger.LogDebug($"Counting rows in table {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
        var count = Convert.ToInt32(command.ExecuteScalar());
        _logger.LogDebug($"Counted ({count}) rows in table {tableName}.");
        return count;
    }

    public async Task<int> CountInTableAsync(string tableName)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Counting rows in table {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        _logger.LogDebug($"Counted ({count}) rows in table {tableName}.");
        return count;
    }

    public bool ContainsInTable<T>(string tableName, string columnName, T matchingValue)
    {
        EnsureDatabase();
        _logger.LogDebug($"Checking if {tableName} contains typed value ({typeof(T).Name}) in column ({columnName})...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        var result = Convert.ToInt32(command.ExecuteScalar()) >= 1;
        if (result)
        {
            _logger.LogDebug($"Found matching typed column ({columnName}) value in {tableName}.");
        }
        else
        {
            _logger.LogDebug($"Failed to find matching typed column ({columnName}) value in {tableName}.");
        }
        return result;
    }

    public async Task<bool> ContainsInTableAsync<T>(string tableName, string columnName, T matchingValue)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Checking if {tableName} contains typed value in column ({columnName})...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        var result = Convert.ToInt32(await command.ExecuteScalarAsync()) >= 1;
        if (result)
        {
            _logger.LogDebug($"Found matching typed column ({columnName}) value in {tableName}.");
        }
        else
        {
            _logger.LogDebug($"Failed to find matching typed column ({columnName}) value in {tableName}.");
        }
        return result;
    }

    public SqliteTransaction CreateTransaction()
    {
        EnsureDatabase();
        _logger.LogDebug("Created database transaction.");
        return _connection!.BeginTransaction();
    }

    public async Task<SqliteTransaction> CreateTransactionAsync()
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug("Created database transaction.");
        return _connection!.BeginTransaction();
    }

    public bool DeleteFromTable<T>(string tableName, string columnName, T matchingValue)
    {
        EnsureDatabase();
        _logger.LogDebug($"Deleting row from {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogDebug($"Deleted row from {tableName} successfully.");

        }
        else
        {
            _logger.LogError($"Failed to delete row from {tableName}.");
        }
        return result;
    }

    public async Task<bool> DeleteFromTableAsync<T>(string tableName, string columnName, T matchingValue)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Deleting row from {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogDebug($"Deleted row from {tableName} successfully.");

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
        _logger.LogDebug($"Dropping table ({tableName})...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS {tableName}";
        command.ExecuteNonQuery();
        _logger.LogDebug($"Dropped table ({tableName}).");
        return true;
    }

    public async Task<bool> DropTableAsync(string tableName)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Dropping table ({tableName})...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS {tableName}";
        await command.ExecuteNonQueryAsync();
        _logger.LogDebug($"Dropped table ({tableName}).");
        return true;
    }

    public bool EnsureTableExists(string tableName, string layout)
    {
        EnsureDatabase();
        _logger.LogDebug($"Ensuring table ({tableName}) exists...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({layout})";
        command.ExecuteNonQuery();
        _logger.LogDebug($"Table ({tableName}) exists.");
        return true;
    }

    public async Task<bool> EnsureTableExistsAsync(string tableName, string layout)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Ensuring table ({tableName}) exists...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({layout})";
        await command.ExecuteNonQueryAsync();
        _logger.LogDebug($"Table ({tableName}) exists.");
        return true;
    }

    public int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
    {
        EnsureDatabase();
        _logger.LogDebug("Executing SQL non-query command...");
        using var command = _connection!.CreateCommand();
        command.CommandText = sql;
        if (parameters is not null)
        {
            foreach (var pair in parameters)
            {
                command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
            }
        }
        var affected = command.ExecuteNonQuery();
        _logger.LogDebug($"Executed SQL non-query command successfully. Affected rows: {affected}");
        return affected;
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug("Executing SQL non-query command...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = sql;
        if (parameters is not null)
        {
            foreach (var pair in parameters)
            {
                command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
            }
        }
        var affected = await command.ExecuteNonQueryAsync();
        _logger.LogDebug($"Executed SQL non-query command successfully. Affected rows: {affected}");
        return affected;
    }

    public bool InsertIntoTable(string tableName, Dictionary<string, object> data)
    {
        EnsureDatabase();
        _logger.LogDebug($"Inserting data into {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogDebug($"Inserted data into {tableName} successfully.");
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
        _logger.LogDebug($"Inserting data into {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogDebug($"Inserted data into {tableName} successfully.");
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
        _logger.LogDebug($"Replacing data into {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT OR REPLACE INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogDebug($"Inserted data into {tableName} successfully.");
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
        _logger.LogDebug($"Inserting data into {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"INSERT OR REPLACE INTO {tableName} ({string.Join(", ", data.Keys)}) VALUES ({string.Join(", ", data.Keys.Select(k => $"${k}"))})";
        foreach (var pair in data)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogDebug($"Inserted data into {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to insert data into {tableName}.");
        }
        return result;
    }

    public SqliteCommand SelectFromTable<T>(string tableName, string columnName, T matchingValue)
    {
        EnsureDatabase();
        _logger.LogDebug($"Selecting data from table {tableName} with matching column ({columnName})...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        _logger.LogDebug($"Selected data from table {tableName} with matching column ({columnName}).");
        return command;
    }

    public async Task<SqliteCommand> SelectFromTableAsync<T>(string tableName, string columnName, T matchingValue)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Selecting data from table {tableName} with matching column ({columnName})...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        _logger.LogDebug($"Selected data from table {tableName} with matching column ({columnName}).");
        return command;
    }

    public SqliteCommand SelectAllFromTable(string tableName)
    {
        EnsureDatabase();
        _logger.LogDebug($"Selecting all data from table {tableName}...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";
        _logger.LogDebug($"Selected all data from table {tableName}.");
        return command;
    }

    public async Task<SqliteCommand> SelectAllFromTableAsync(string tableName)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Selecting all data from table {tableName}...");
        var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";
        _logger.LogDebug($"Selected all data from table {tableName}.");
        return command;
    }

    public bool TableExists(string tableName)
    {
        EnsureDatabase();
        _logger.LogDebug($"Checking if table ({tableName}) exists...");
        using var command = _connection!.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableNameParam";
        command.Parameters.AddWithValue("$tableNameParam", tableName);
        var exists = Convert.ToInt32(command.ExecuteScalar()) >= 1;
        _logger.LogDebug(exists ? $"Table ({tableName}) exists." : $"Table ({tableName}) does not exist.");
        return exists;
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Checking if table ({tableName}) exists...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableNameParam";
        command.Parameters.AddWithValue("$tableNameParam", tableName);
        var result = await command.ExecuteScalarAsync();
        var exists = Convert.ToInt32(result) >= 1;
        _logger.LogDebug(exists ? $"Table ({tableName}) exists." : $"Table ({tableName}) does not exist.");
        return exists;
    }

    public bool UpdateInTable<T>(string tableName, string columnName, T matchingValue, Dictionary<string, object> newData)
    {
        EnsureDatabase();
        _logger.LogDebug($"Updating data in {tableName}...");
        using var command = _connection!.CreateCommand();
        command.CommandText = $"UPDATE {tableName} SET {string.Join(", ", newData.Keys.Select(k => $"{k} = ${k}"))} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        foreach (var pair in newData)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = command.ExecuteNonQuery() > 0;
        if (result)
        {
            _logger.LogDebug($"Updated data in {tableName} successfully.");
        }
        else
        {
            _logger.LogError($"Failed to update data in {tableName}.");
        }
        return result;
    }

    public async Task<bool> UpdateInTableAsync<T>(string tableName, string columnName, T matchingValue, Dictionary<string, object> newData)
    {
        await EnsureDatabaseAsync();
        _logger.LogDebug($"Updating data in {tableName}...");
        await using var command = _connection!.CreateCommand();
        command.CommandText = $"UPDATE {tableName} SET {string.Join(", ", newData.Keys.Select(k => $"{k} = ${k}"))} WHERE {columnName} = $matchingValueParam";
        command.Parameters.AddWithValue("$matchingValueParam", matchingValue);
        foreach (var pair in newData)
        {
            command.Parameters.AddWithValue($"${pair.Key}", pair.Value);
        }
        var result = await command.ExecuteNonQueryAsync() > 0;
        if (result)
        {
            _logger.LogDebug($"Updated data in {tableName} successfully.");
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
        _logger.LogDebug($"Opening application database ({path})...");
        var secret = string.Empty;
        if (!_appInfo.IsPortable && (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()))
        {
            try
            {
                secret = ((Task.Run(() => _secretService.GetAsync(_appInfo.Id)).GetAwaiter().GetResult()) ?? (Task.Run(() => _secretService.CreateAsync(_appInfo.Id)).GetAwaiter().GetResult()))?.Value;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Secret service unavailable: {e.Message}. The database will not be encrypted.");
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder($"Data Source='{path}'")
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = secret ?? string.Empty,
            Pooling = false
        }.ToString());
        try
        {
            _connection.Open();
            IsEncrypted = !string.IsNullOrEmpty(secret);
            _logger.LogDebug($"Opened application database ({path}).");
        }
        catch (SqliteException e)
        {
            _logger.LogError($"Failed to open application database: {e}");
            _connection.Dispose();
            _connection = null;
            if (string.IsNullOrEmpty(secret))
            {
                _logger.LogWarning("The database may be encrypted but the secret service is unavailable. Falling back to an in-memory database.");
                _connection = new SqliteConnection("Data Source=:memory:");
                _connection.Open();
                IsEncrypted = false;
            }
            else
            {
                throw;
            }
        }
    }

    private async Task EnsureDatabaseAsync()
    {
        if (_connection is not null)
        {
            return;
        }
        var path = Path.Combine(_appInfo.IsPortable ? System.Environment.ExecutingDirectory : Path.Combine(UserDirectories.Config, _appInfo.Name), "app.db");
        _logger.LogDebug($"Opening application database ({path})...");
        var secret = string.Empty;
        if (!_appInfo.IsPortable && (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()))
        {
            try
            {
                secret = ((await _secretService.GetAsync(_appInfo.Id)) ?? (await _secretService.CreateAsync(_appInfo.Id)))?.Value;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Secret service unavailable: {e.Message}. The database will not be encrypted.");
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder($"Data Source='{path}'")
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = secret ?? string.Empty,
            Pooling = false
        }.ToString());
        try
        {
            await _connection.OpenAsync();
            IsEncrypted = !string.IsNullOrEmpty(secret);
            _logger.LogDebug($"Opened application database ({path}).");
        }
        catch (SqliteException e)
        {
            _logger.LogError($"Failed to open application database: {e}");
            await _connection.DisposeAsync();
            _connection = null;
            if (string.IsNullOrEmpty(secret))
            {
                _logger.LogWarning("The database may be encrypted but the secret service is unavailable. Falling back to an in-memory database.");
                _connection = new SqliteConnection("Data Source=:memory:");
                await _connection.OpenAsync();
                IsEncrypted = false;
            }
            else
            {
                throw;
            }
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
