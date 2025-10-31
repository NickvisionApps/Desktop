using Nickvision.Desktop.Keyring;
using System;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class PasswordGeneratorTests
{
    [TestMethod]
    public void Case001_Generate()
    {
        var generator = new PasswordGenerator();
        var password = generator.Next();
        Console.WriteLine(password);
        Assert.IsTrue(password.Length == 16);
        foreach (var c in password)
        {
            Assert.IsTrue(
                char.IsDigit(c) ||
                char.IsLower(c) ||
                char.IsUpper(c) ||
                char.IsPunctuation(c) ||
                char.IsSymbol(c) ||
                char.IsWhiteSpace(c));
        }
    }

    [TestMethod]
    public void Case002_Generate()
    {
        var generator = new PasswordGenerator(PasswordContent.Numeric);
        var password = generator.Next(23);
        Console.WriteLine(password);
        Assert.IsTrue(password.Length == 23);
        foreach (var c in password)
        {
            Assert.IsTrue(char.IsDigit(c));
            Assert.IsFalse(char.IsLetter(c));
            Assert.IsFalse(char.IsWhiteSpace(c));
        }
    }

    [TestMethod]
    public void Case003_Generate()
    {
        var generator = new PasswordGenerator(PasswordContent.Numeric | PasswordContent.Lowercase);
        var password = generator.Next(64);
        Console.WriteLine(password);
        Assert.IsTrue(password.Length == 64);
        foreach (var c in password)
        {
            Assert.IsTrue(char.IsDigit(c) || char.IsLower(c));
            Assert.IsFalse(char.IsUpper(c));
            Assert.IsFalse(char.IsWhiteSpace(c));
        }
    }

    [TestMethod]
    public void Case004_Generate()
    {
        var generator = new PasswordGenerator(PasswordContent.AllNoSpace);
        var password = generator.Next(64);
        Console.WriteLine(password);
        Assert.IsTrue(password.Length == 64);
        foreach (var c in password)
        {
            Assert.IsTrue(
                char.IsDigit(c) || char.IsLower(c) || char.IsUpper(c) || char.IsPunctuation(c) || char.IsSymbol(c));
            Assert.IsFalse(char.IsWhiteSpace(c));
        }
    }
}
