using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Nickvision.Desktop.FreeDesktop;

/// <summary>
/// Internal proxy for the org.freedesktop.secrets D-Bus interface (freedesktop Secret Service).
/// Uses "plain" (unencrypted) session transport, which is safe for local D-Bus sockets.
/// </summary>
internal sealed class SecretServiceProxy : IDisposable
{
    private const string SecretsBus = "org.freedesktop.secrets";
    private const string SecretsPath = "/org/freedesktop/secrets";
    private const string ServiceInterface = "org.freedesktop.Secret.Service";
    private const string CollectionInterface = "org.freedesktop.Secret.Collection";
    private const string ItemInterface = "org.freedesktop.Secret.Item";
    private const string ContentType = "text/plain; charset=utf8";

    private readonly DBusConnection _connection;
    private readonly string _sessionPath;
    private bool _disposed;

    private SecretServiceProxy(DBusConnection connection, string sessionPath)
    {
        _connection = connection;
        _sessionPath = sessionPath;
        _disposed = false;
    }

    /// <summary>
    /// Connects to the D-Bus session bus and opens a plain encryption session with the secrets service.
    /// </summary>
    /// <returns>A connected SecretServiceProxy, or null if the secrets service is unavailable</returns>
    internal static async Task<SecretServiceProxy?> ConnectAsync()
    {
        var sessionAddress = DBusAddress.Session;
        if (sessionAddress is null)
        {
            return null;
        }
        var connection = new DBusConnection(sessionAddress);
        await connection.ConnectAsync();
        var sessionPath = await OpenSessionAsync(connection);
        if (string.IsNullOrEmpty(sessionPath) || sessionPath == "/")
        {
            connection.Dispose();
            return null;
        }
        return new SecretServiceProxy(connection, sessionPath);
    }

    /// <summary>
    /// Gets the object path of the default collection, or null/slash if it does not exist.
    /// </summary>
    internal async Task<string?> GetDefaultCollectionPathAsync()
    {
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, SecretsPath, ServiceInterface, "ReadAlias", "s", MessageFlags.None);
            writer.WriteString("default");
            buffer = writer.CreateMessage();
        }
        return await _connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            return reader.ReadObjectPathAsString();
        }, null);
    }

    /// <summary>
    /// Creates a collection with the given label and alias.
    /// </summary>
    /// <param name="label">The human-readable label of the collection</param>
    /// <param name="alias">The alias (e.g. "default")</param>
    /// <returns>The object path of the created collection, or null on failure</returns>
    internal async Task<string?> CreateCollectionAsync(string label, string alias)
    {
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, SecretsPath, ServiceInterface, "CreateCollection", "a{sv}s", MessageFlags.None);
            writer.WriteDictionary(new Dictionary<string, VariantValue>
            {
                ["org.freedesktop.Secret.Collection.Label"] = label
            });
            writer.WriteString(alias);
            buffer = writer.CreateMessage();
        }
        return await _connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            reader.AlignStruct();
            var collection = reader.ReadObjectPathAsString();
            reader.ReadObjectPath(); // prompt (ignored)
            return collection;
        }, null);
    }

    /// <summary>
    /// Unlocks the given object (collection or item path), prompting the user if required.
    /// </summary>
    /// <param name="objectPath">The D-Bus object path to unlock</param>
    /// <returns>True if unlocked successfully, false if the user dismissed the prompt</returns>
    internal async Task<bool> UnlockAsync(string objectPath)
    {
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, SecretsPath, ServiceInterface, "Unlock", "ao", MessageFlags.None);
            writer.WriteArray(new ObjectPath[] { objectPath });
            buffer = writer.CreateMessage();
        }
        var promptPath = await _connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            reader.AlignStruct();
            reader.ReadArrayOfObjectPath(); // already-unlocked list (not reliable for items that need prompting)
            return reader.ReadObjectPathAsString(); // prompt path, or "/" if no prompt is needed
        }, null);
        if (string.IsNullOrEmpty(promptPath) || promptPath == "/")
        {
            // Object was already unlocked, no user prompt required
            return true;
        }
        return await PromptAsync(promptPath);
    }

    /// <summary>
    /// Invokes a Secret Service prompt and waits for the user to complete or dismiss it.
    /// </summary>
    /// <param name="promptPath">The D-Bus object path of the prompt</param>
    /// <returns>True if the user completed the prompt, false if dismissed</returns>
    private async Task<bool> PromptAsync(string promptPath)
    {
        var tcs = new TaskCompletionSource<bool>();
        using var subscription = await _connection.WatchSignalAsync(
            SecretsBus, promptPath, "org.freedesktop.Secret.Prompt", "Completed",
            static (Message m, object? _) =>
            {
                var reader = m.GetBodyReader();
                return reader.ReadBool(); // dismissed
            },
            (Exception? ex, bool dismissed) =>
            {
                if (ex is not null)
                {
                    tcs.TrySetException(ex);
                }
                else
                {
                    tcs.TrySetResult(!dismissed);
                }
            },
            null, /* emitOnCapturedContext */ false, ObserverFlags.None);
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, promptPath, "org.freedesktop.Secret.Prompt", "Prompt", "s", MessageFlags.None);
            writer.WriteString(""); // no parent window-id
            buffer = writer.CreateMessage();
        }
        await _connection.CallMethodAsync(buffer);
        return await tcs.Task;
    }

    /// <summary>
    /// Creates an item in the specified collection.
    /// </summary>
    /// <param name="collectionPath">The collection object path</param>
    /// <param name="label">The label for the new item</param>
    /// <param name="attributes">Lookup attributes for the item</param>
    /// <param name="value">The secret value</param>
    /// <param name="replace">Whether to replace an existing item with the same attributes</param>
    /// <returns>The object path of the created item, or null on failure</returns>
    internal async Task<string?> CreateItemAsync(string collectionPath, string label, Dictionary<string, string> attributes, string value, bool replace = false)
    {
        var attrDict = new Dict<string, string>();
        foreach (var kv in attributes)
        {
            attrDict.Add(kv.Key, kv.Value);
        }
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, collectionPath, CollectionInterface, "CreateItem", "a{sv}(oayays)b", MessageFlags.None);
            writer.WriteDictionary(new Dictionary<string, VariantValue>
            {
                ["org.freedesktop.Secret.Item.Label"] = label,
                ["org.freedesktop.Secret.Item.Attributes"] = attrDict
            });
            writer.WriteStructureStart();
            writer.WriteObjectPath(_sessionPath);
            writer.WriteArray(Array.Empty<byte>()); // empty parameters (plain encryption)
            writer.WriteArray(Encoding.UTF8.GetBytes(value));
            writer.WriteString(ContentType);
            writer.WriteBool(replace);
            buffer = writer.CreateMessage();
        }
        return await _connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            reader.AlignStruct();
            var item = reader.ReadObjectPathAsString();
            reader.ReadObjectPath(); // prompt (ignored)
            return item;
        }, null);
    }

    /// <summary>
    /// Searches for items in the specified collection that match the given attributes.
    /// </summary>
    /// <param name="collectionPath">The collection object path</param>
    /// <param name="attributes">Attributes to match</param>
    /// <returns>An array of matching item object paths</returns>
    internal async Task<string[]> SearchItemsAsync(string collectionPath, Dictionary<string, string> attributes)
    {
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, collectionPath, CollectionInterface, "SearchItems", "a{ss}", MessageFlags.None);
            var dictStart = writer.WriteDictionaryStart();
            foreach (var kv in attributes)
            {
                writer.WriteDictionaryEntryStart();
                writer.WriteString(kv.Key);
                writer.WriteString(kv.Value);
            }
            writer.WriteDictionaryEnd(dictStart);
            buffer = writer.CreateMessage();
        }
        return await _connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            var paths = reader.ReadArrayOfObjectPath();
            var result = new string[paths.Length];
            for (var i = 0; i < paths.Length; i++)
            {
                result[i] = paths[i].ToString();
            }
            return result;
        }, null);
    }

    /// <summary>
    /// Gets the secret value of the specified item.
    /// </summary>
    /// <param name="itemPath">The item object path</param>
    /// <returns>The secret value as a string, or null on failure</returns>
    internal async Task<string?> GetSecretAsync(string itemPath)
    {
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, itemPath, ItemInterface, "GetSecret", "o", MessageFlags.None);
            writer.WriteObjectPath(_sessionPath);
            buffer = writer.CreateMessage();
        }
        try
        {
            return await _connection.CallMethodAsync(buffer, static (Message m, object? _) =>
            {
                var reader = m.GetBodyReader();
                reader.AlignStruct();
                reader.ReadObjectPath(); // session (ignored)
                reader.ReadArrayOfByte(); // parameters (ignored for plain)
                var valueBytes = reader.ReadArrayOfByte();
                reader.ReadString(); // content type (ignored)
                return Encoding.UTF8.GetString(valueBytes);
            }, null);
        }
        catch (DBusErrorReplyException e) when (e.ErrorName == "org.freedesktop.Secret.Error.IsLocked")
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the secret value of the specified item.
    /// </summary>
    /// <param name="itemPath">The item object path</param>
    /// <param name="value">The new secret value</param>
    internal async Task SetSecretAsync(string itemPath, string value)
    {
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, itemPath, ItemInterface, "SetSecret", "(oayays)", MessageFlags.None);
            writer.WriteStructureStart();
            writer.WriteObjectPath(_sessionPath);
            writer.WriteArray(Array.Empty<byte>()); // empty parameters (plain encryption)
            writer.WriteArray(Encoding.UTF8.GetBytes(value));
            writer.WriteString(ContentType);
            buffer = writer.CreateMessage();
        }
        await _connection.CallMethodAsync(buffer);
    }

    /// <summary>
    /// Deletes the specified item.
    /// </summary>
    /// <param name="itemPath">The item object path</param>
    internal async Task DeleteItemAsync(string itemPath)
    {
        MessageBuffer buffer;
        {
            using var writer = _connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, itemPath, ItemInterface, "Delete", null, MessageFlags.None);
            buffer = writer.CreateMessage();
        }
        await _connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            reader.ReadObjectPath(); // prompt (ignored)
            return true;
        }, null);
    }

    /// <summary>
    /// Opens a "plain" encryption session with the secrets service.
    /// </summary>
    private static async Task<string> OpenSessionAsync(DBusConnection connection)
    {
        MessageBuffer buffer;
        {
            using var writer = connection.GetMessageWriter();
            writer.WriteMethodCallHeader(SecretsBus, SecretsPath, ServiceInterface, "OpenSession", "sv", MessageFlags.None);
            writer.WriteString("plain");
            writer.WriteVariantString(""); // input for plain = empty string
            buffer = writer.CreateMessage();
        }
        return await connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            reader.AlignStruct();
            reader.ReadVariantValue(); // output variant (ignored for plain)
            return reader.ReadObjectPathAsString(); // session path
        }, null);
    }

    /// <summary>
    /// Disposes the SecretServiceProxy and its underlying D-Bus connection.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _connection.Dispose();
            _disposed = true;
        }
    }
}
