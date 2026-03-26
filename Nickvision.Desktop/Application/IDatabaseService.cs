using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public interface IDatabaseService
{
    event EventHandler<PasswordRequiredEventArgs>? PasswordRequired;

    bool ContainsInTable(string tableName, string columnName, string matchingValue);
    Task<bool> ContainsInTableAsync(string tableName, string columnName, string matchingValue);
    SqliteTransaction CreateTransation();
    Task<SqliteTransaction> CreateTransationAsync();
    bool DeleteFromTable(string tableName, string columnName, string matchingValue);
    Task<bool> DeleteFromTableAsync(string tableName, string columnName, string matchingValue);
    bool EnsureTableExists(string tableName, string layout);
    Task<bool> EnsureTableExistsAsync(string tableName, string layout);
    bool InsertIntoTable(string tableName, Dictionary<string, object> data);
    Task<bool> InsertIntoTableAsync(string tableName, Dictionary<string, object> data);
    bool ReplaceIntoTable(string tableName, Dictionary<string, object> data);
    Task<bool> ReplaceIntoTableAsync(string tableName, Dictionary<string, object> data);
    SqliteCommand SelectFromTable(string tableName, string columnName, string matchingValue);
    Task<SqliteCommand> SelectFromTableAsync(string tableName, string columnName, string matchingValue);
    SqliteCommand SelectAllFromTable(string tableName);
    Task<SqliteCommand> SelectAllFromTableAsync(string tableName);
    bool UpdateInTable(string tableName, string columnName, string matchingValue, Dictionary<string, object> newData);
    Task<bool> UpdateInTableAsync(string tableName, string columnName, string matchingValue, Dictionary<string, object> newData);
}
