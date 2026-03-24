using Nickvision.Desktop.Helpers;
using System.Text.Json.Serialization;

namespace Nickvision.Desktop.Tests;

public class CopyModel
{
    public string Name { get; set; }
    public int Value { get; set; }

    public CopyModel()
    {
        Name = string.Empty;
        Value = 0;
    }
}

[JsonSerializable(typeof(CopyModel))]
internal partial class CopyModelContext : JsonSerializerContext
{

}

[TestClass]
public sealed class ObjectExtensionsTests
{
    [TestMethod]
    public void Case001_DeepCopy_PropertiesAreEqual()
    {
        var original = new CopyModel()
        {
            Name = "test",
            Value = 42
        };
        var copy = original.DeepCopy(CopyModelContext.Default.CopyModel);
        Assert.AreEqual(original.Name, copy.Name);
        Assert.AreEqual(original.Value, copy.Value);
    }

    [TestMethod]
    public void Case002_DeepCopy_IsNotSameReference()
    {
        var original = new CopyModel()
        {
            Name = "hello",
            Value = 7
        };
        var copy = original.DeepCopy(CopyModelContext.Default.CopyModel);
        Assert.IsFalse(ReferenceEquals(original, copy));
    }

    [TestMethod]
    public void Case003_DeepCopy_MutatingCopyDoesNotAffectOriginal()
    {
        var original = new CopyModel()
        {
            Name = "original",
            Value = 1
        };
        var copy = original.DeepCopy(CopyModelContext.Default.CopyModel);
        copy.Name = "mutated";
        copy.Value = 99;
        Assert.AreEqual("original", original.Name);
        Assert.AreEqual(1, original.Value);
    }
}
