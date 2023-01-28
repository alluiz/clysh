using System.Text.RegularExpressions;
using Clysh.Helper;
using NUnit.Framework;

namespace Clysh.Tests;

public static class ExtendedAssert
{
    public static void MatchMessage(string message, string messagePattern)
    {
        Assert.IsTrue(ClyshMessages.Match(message, messagePattern));
    }
    
    public static void MatchMessage(string message, string messagePattern, params string[] values)
    {
        Assert.IsTrue(ClyshMessages.Match(message, messagePattern, values));
    }
}