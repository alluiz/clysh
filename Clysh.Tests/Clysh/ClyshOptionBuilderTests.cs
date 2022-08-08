using System;
using Clysh.Core;
using Clysh.Core.Builder;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshOptionBuilderTests
{
    [Test]
    public void InvalidShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Id("test", ""));
        Assert.AreEqual("Invalid shortcut. The shortcut must be null or follow the pattern [a-zA-Z]{1} and between 1 and 1 chars. Shortcut: '' (Parameter 'shortcutId')", exception?.InnerException?.Message);
    }

    [Test]
    public void InvalidShortcutUsingHelpShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Id("test", "h"));
        Assert.AreEqual("Shortcut 'h' is reserved to help shortcut. Option: test (Parameter 'shortcut')", exception?.InnerException?.Message);
    }

    [Test]
    public void InvalidDescription()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Description("test"));
        Assert.AreEqual("Option description must be not null or empty and between 10 and 500 chars. Description: 'test' (Parameter 'descriptionValue')", exception?.InnerException?.Message);
    }
}