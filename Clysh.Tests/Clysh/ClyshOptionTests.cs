using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Helper;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshOptionTests
{
    [Test]
    public void Shortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<EntityException>(() =>  builder.Id("test", "").Description("The test command").Build());
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateShorcut);
    }

    [Test]
    public void ShortcutUsingHelpShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<EntityException>(() =>  builder.Id("test", "h").Description("The test command").Build());
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateOptionShortcut);
    }

    [Test]
    public void Description()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<EntityException>(() =>  builder.Id("test")
            .Description("test").Build());
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateDescription);
    }
}