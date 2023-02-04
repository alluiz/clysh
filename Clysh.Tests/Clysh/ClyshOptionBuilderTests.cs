using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Helper;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshOptionBuilderTests
{
    [Test]
    public void Shortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Id("test", ""));
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message!, ClyshMessages.ErrorOnValidateShorcut);
    }

    [Test]
    public void ShortcutUsingHelpShortcut()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Id("test", "h"));
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message!, ClyshMessages.ErrorOnValidateOptionShortcut);
    }

    [Test]
    public void Description()
    {
        var builder = new ClyshOptionBuilder();
        var exception = Assert.Throws<ClyshException>(() =>  builder.Description("test"));
        ExtendedAssert.MatchMessage(exception?.InnerException?.Message!, ClyshMessages.ErrorOnValidateDescription);
    }
}