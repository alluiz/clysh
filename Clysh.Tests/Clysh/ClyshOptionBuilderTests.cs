using System;
using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Helper;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshOptionBuilderTests
{
    [Test]
    public void InvalidShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Id("test", ""));
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message, ClyshMessages.ErrorOnValidateShorcut);
    }

    [Test]
    public void InvalidShortcutUsingHelpShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Id("test", "h"));
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message, ClyshMessages.ErrorOnValidateOptionShortcut);
    }

    [Test]
    public void InvalidDescription()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Description("test"));
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message, ClyshMessages.ErrorOnValidateDescription);
    }
}