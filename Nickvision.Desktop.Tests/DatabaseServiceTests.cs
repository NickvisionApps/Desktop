using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.System;
using Nickvision.Desktop.Tests.Mocks;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class DatabaseServiceTests
{
    private static DatabaseService? _databaseService;

    [TestMethod]
    public void Case001_Init()
    {
        _databaseService = new DatabaseService(new MockLogger<DatabaseService>(), new AppInfo("org.nickvision.desktop.test.database", "Nickvision.Desktop.Test.Database", "Test Database"), new SecretService(new MockLogger<SecretService>()));
        Assert.IsNotNull(_databaseService);
        Assert.IsFalse(_databaseService.IsEncrypted);
    }

    [TestMethod]
    public void Case002_EnsureTableAndTransaction()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(_databaseService.EnsureTableExists("test_table", "id TEXT PRIMARY KEY, name TEXT, age INTEGER"));
        Assert.IsTrue(_databaseService.IsEncrypted);
        Assert.IsTrue(_databaseService.TableExists("test_table"));
        Assert.IsFalse(_databaseService.TableExists("missing_table"));
        using var transaction = _databaseService.CreateTransaction();
        Assert.IsNotNull(transaction);
        transaction.Commit();
    }

    [TestMethod]
    public void Case003_InsertContainsAndSelect()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(_databaseService.InsertIntoTable("test_table", new Dictionary<string, object>()
        {
            { "id", "row1" },
            { "name", "Alice" },
            { "age", 30 }
        }));
        Assert.IsTrue(_databaseService.ContainsInTable("test_table", "id", "row1"));
        Assert.IsFalse(_databaseService.ContainsInTable("test_table", "id", "row_does_not_exist"));
        using var command = _databaseService.SelectFromTable("test_table", "id", "row1");
        using var reader = command.ExecuteReader();
        Assert.IsTrue(reader.Read());
        Assert.AreEqual("row1", reader.GetString(0));
        Assert.AreEqual("Alice", reader.GetString(1));
        Assert.AreEqual("30", reader.GetString(2));
        Assert.IsFalse(reader.Read());
    }

    [TestMethod]
    public void Case004_UpdateReplaceDeleteAndDrop()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(_databaseService.UpdateInTable("test_table", "id", "row1", new Dictionary<string, object>()
        {
            { "name", "Alice Updated" },
            { "age", 31 }
        }));
        using (var command = _databaseService.SelectFromTable("test_table", "id", "row1"))
        using (var reader = command.ExecuteReader())
        {
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("Alice Updated", reader.GetString(1));
            Assert.AreEqual("31", reader.GetString(2));
        }
        Assert.IsTrue(_databaseService.ReplaceIntoTable("test_table", new Dictionary<string, object>()
        {
            { "id", "row1" },
            { "name", "Alice Replaced" },
            { "age", 40 }
        }));
        using (var command = _databaseService.SelectFromTable("test_table", "id", "row1"))
        using (var reader = command.ExecuteReader())
        {
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("Alice Replaced", reader.GetString(1));
            Assert.AreEqual("40", reader.GetString(2));
        }
        Assert.IsTrue(_databaseService.DeleteFromTable("test_table", "id", "row1"));
        Assert.IsFalse(_databaseService.DeleteFromTable("test_table", "id", "row1"));
        Assert.IsFalse(_databaseService.ContainsInTable("test_table", "id", "row1"));
        Assert.IsTrue(_databaseService.DropTable("test_table"));
    }

    [TestMethod]
    public async Task Case005_AsyncCrud()
    {
        Assert.IsNotNull(_databaseService);
        const string asyncTable = "test_table_async";
        Assert.IsTrue(await _databaseService.EnsureTableExistsAsync(asyncTable, "id TEXT PRIMARY KEY, name TEXT, age INTEGER"));
        Assert.IsTrue(await _databaseService.TableExistsAsync(asyncTable));
        Assert.IsFalse(await _databaseService.TableExistsAsync("missing_table_async"));
        await using (var transaction = await _databaseService.CreateTransactionAsync())
        {
            Assert.IsNotNull(transaction);
            await transaction.CommitAsync();
        }
        Assert.IsTrue(await _databaseService.InsertIntoTableAsync(asyncTable, new Dictionary<string, object>()
        {
            { "id", "rowA" },
            { "name", "Bob" },
            { "age", 20 }
        }));
        Assert.IsTrue(await _databaseService.ContainsInTableAsync(asyncTable, "id", "rowA"));
        Assert.IsFalse(await _databaseService.ContainsInTableAsync(asyncTable, "id", "row_missing"));
        await using (var command = await _databaseService.SelectAllFromTableAsync(asyncTable))
        await using (var reader = await command.ExecuteReaderAsync())
        {
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("rowA", reader.GetString(0));
            Assert.AreEqual("Bob", reader.GetString(1));
            Assert.AreEqual("20", reader.GetString(2));
            Assert.IsFalse(await reader.ReadAsync());
        }
        Assert.IsTrue(await _databaseService.UpdateInTableAsync(asyncTable, "id", "rowA", new Dictionary<string, object>()
        {
            { "name", "Bob Updated" },
            { "age", 21 }
        }));
        await using (var command = await _databaseService.SelectFromTableAsync(asyncTable, "id", "rowA"))
        await using (var reader = await command.ExecuteReaderAsync())
        {
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("Bob Updated", reader.GetString(1));
            Assert.AreEqual("21", reader.GetString(2));
            Assert.IsFalse(await reader.ReadAsync());
        }
        Assert.IsTrue(await _databaseService.ReplaceIntoTableAsync(asyncTable, new Dictionary<string, object>()
        {
            { "id", "rowA" },
            { "name", "Bob Replaced" },
            { "age", 22 }
        }));
        await using (var command = await _databaseService.SelectFromTableAsync(asyncTable, "id", "rowA"))
        await using (var reader = await command.ExecuteReaderAsync())
        {
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("Bob Replaced", reader.GetString(1));
            Assert.AreEqual("22", reader.GetString(2));
            Assert.IsFalse(await reader.ReadAsync());
        }
        Assert.IsTrue(await _databaseService.DeleteFromTableAsync(asyncTable, "id", "rowA"));
        Assert.IsFalse(await _databaseService.DeleteFromTableAsync(asyncTable, "id", "rowA"));
        Assert.IsTrue(await _databaseService.DropTableAsync(asyncTable));
    }

    [TestMethod]
    public void Case006_CountAndExecuteNonQuery()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(_databaseService.EnsureTableExists("test_count", "id TEXT PRIMARY KEY, val TEXT"));
        Assert.AreEqual(1, _databaseService.ExecuteNonQuery("INSERT INTO test_count (id, val) VALUES ($id, $val)", new Dictionary<string, object>()
        {
            { "id", "c1" },
            { "val", "v1" }
        }));
        Assert.AreEqual(1, _databaseService.CountInTable("test_count"));
        Assert.IsTrue(_databaseService.DropTable("test_count"));
    }

    [TestMethod]
    public void Case007_TypedContainsInTable()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(_databaseService.EnsureTableExists("test_typed", "id TEXT PRIMARY KEY, age INTEGER, enabled INTEGER, name TEXT"));
        Assert.IsTrue(_databaseService.InsertIntoTable("test_typed", new Dictionary<string, object>()
        {
            { "id", "typed1" },
            { "age", 55 },
            { "enabled", true },
            { "name", "typed-name" }
        }));
        Assert.IsTrue(_databaseService.ContainsInTable("test_typed", "age", 55));
        Assert.IsFalse(_databaseService.ContainsInTable("test_typed", "age", 56));
        Assert.IsTrue(_databaseService.ContainsInTable("test_typed", "enabled", true));
        Assert.IsFalse(_databaseService.ContainsInTable("test_typed", "enabled", false));
        Assert.IsTrue(_databaseService.ContainsInTable("test_typed", "name", "typed-name"));
        Assert.IsFalse(_databaseService.ContainsInTable("test_typed", "name", "typed-missing"));
        Assert.IsTrue(_databaseService.DropTable("test_typed"));
    }

    [TestMethod]
    public async Task Case008_CountAndExecuteNonQueryAsync()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(await _databaseService.EnsureTableExistsAsync("test_count_async", "id TEXT PRIMARY KEY, val TEXT"));
        Assert.AreEqual(1, await _databaseService.ExecuteNonQueryAsync("INSERT INTO test_count_async (id, val) VALUES ($id, $val)", new Dictionary<string, object>()
        {
            { "id", "ac1" },
            { "val", "av1" }
        }));
        Assert.AreEqual(1, await _databaseService.CountInTableAsync("test_count_async"));
        Assert.IsTrue(await _databaseService.DropTableAsync("test_count_async"));
    }

    [TestMethod]
    public async Task Case009_TypedContainsInTableAsync()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(await _databaseService.EnsureTableExistsAsync("test_typed_async", "id TEXT PRIMARY KEY, age INTEGER, enabled INTEGER, name TEXT"));
        Assert.IsTrue(await _databaseService.InsertIntoTableAsync("test_typed_async", new Dictionary<string, object>()
        {
            { "id", "typedA" },
            { "age", 77 },
            { "enabled", false },
            { "name", "typed-async-name" }
        }));
        Assert.IsTrue(await _databaseService.ContainsInTableAsync("test_typed_async", "age", 77));
        Assert.IsFalse(await _databaseService.ContainsInTableAsync("test_typed_async", "age", 78));
        Assert.IsTrue(await _databaseService.ContainsInTableAsync("test_typed_async", "enabled", false));
        Assert.IsFalse(await _databaseService.ContainsInTableAsync("test_typed_async", "enabled", true));
        Assert.IsTrue(await _databaseService.ContainsInTableAsync("test_typed_async", "name", "typed-async-name"));
        Assert.IsFalse(await _databaseService.ContainsInTableAsync("test_typed_async", "name", "typed-async-missing"));
        Assert.IsTrue(await _databaseService.DropTableAsync("test_typed_async"));
    }

    [TestMethod]
    public async Task Case010_Cleanup()
    {
        var path = Path.Combine(UserDirectories.Config, "Nickvision.Desktop.Test.Database", "app.db");
        Assert.IsNotNull(_databaseService);
        await _databaseService.DisposeAsync();
        File.Delete(path);
        Directory.Delete(Path.GetDirectoryName(path)!);
        Assert.IsFalse(File.Exists(path));
        _databaseService = null;
    }
}
