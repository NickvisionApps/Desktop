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
    }

    [TestMethod]
    public void Case002_EnsureTableAndTransaction()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsTrue(_databaseService.EnsureTableExists("test_table", "id TEXT PRIMARY KEY, name TEXT, age INTEGER"));
        using var transaction = _databaseService.CreateTransation();
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
        await using (var transaction = await _databaseService.CreateTransationAsync())
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
    public async Task Case006_Cleanup()
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
