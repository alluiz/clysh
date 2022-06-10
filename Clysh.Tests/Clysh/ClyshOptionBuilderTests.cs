using System;
using Clysh.Core.Builder;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshOptionBuilderTests
{
    [Test]
    public void InvalidShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ArgumentException>(() =>  builder.Shortcut(""));
        Assert.AreEqual("Invalid shortcut. The shortcut must be null or follow the pattern [a-zA-Z] and between 1 and 1 chars. (Parameter 'shortcut')", exception?.Message);
    }
    
    [Test]
    public void InvalidShortcutUsingHelpShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ArgumentException>(() =>  builder.Id("test").Shortcut("h"));
        Assert.AreEqual("Shortcut 'h' is reserved to help shortcut. (Parameter 'shortcut')", exception?.Message);
    }
    
    [Test]
    public void InvalidDescription()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ArgumentException>(() =>  builder.Description("test"));
        Assert.AreEqual("Option description must be not null or empty and between 10 and 50 chars. (Parameter 'description')", exception?.Message);
    }
}