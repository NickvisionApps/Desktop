using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public interface IDatabaseService
{
    event EventHandler<PasswordRequiredEventArgs>? PasswordRequired;

    bool ClearTable(string tableName);
    Task<bool> ClearTableAsync(string tableName);
    int CountInTable(string tableName);
    Task<int> CountInTableAsync(string tableName);
    bool ContainsInTable<T>(string tableName, string columnName, T matchingValue);
    Task<bool> ContainsInTableAsync<T>(string tableName, string columnName, T matchingValue);
    SqliteTransaction CreateTransation();
    Task<SqliteTransaction> CreateTransationAsync();
    bool DeleteFromTable<T>(string tableName, string columnName, T matchingValue);
    Task<bool> DeleteFromTableAsync<T>(string tableName, string columnName, T matchingValue);
    bool DropTable(string tableName);
    Task<bool> DropTableAsync(string tableName);
    bool EnsureTableExists(string tableName, string layout);
    Task<bool> EnsureTableExistsAsync(string tableName, string layout);
    int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null);
    Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null);
    bool InsertIntoTable(string tableName, Dictionary<string, object> data);
    Task<bool> InsertIntoTableAsync(string tableName, Dictionary<string, object> data);
    bool ReplaceIntoTable(string tableName, Dictionary<string, object> data);
    Task<bool> ReplaceIntoTableAsync(string tableName, Dictionary<string, object> data);
    SqliteCommand SelectFromTable<T>(string tableName, string columnName, T matchingValue);
    Task<SqliteCommand> SelectFromTableAsync<T>(string tableName, string columnName, T matchingValue);
    SqliteCommand SelectAllFromTable(string tableName);
    Task<SqliteCommand> SelectAllFromTableAsync(string tableName);
    bool TableExists(string tableName);
    Task<bool> TableExistsAsync(string tableName);
    bool UpdateInTable<T>(string tableName, string columnName, T matchingValue, Dictionary<string, object> newData);
    Task<bool> UpdateInTableAsync<T>(string tableName, string columnName, T matchingValue, Dictionary<string, object> newData);
}
