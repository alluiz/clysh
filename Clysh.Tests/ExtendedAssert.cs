using System;
using Clysh.Helper;
using NUnit.Framework;

namespace Clysh.Tests;

public static class ExtendedAssert
{
    public static void MatchMessage(string message, string messagePattern)
    {
        try
        {
            Assert.IsTrue(ClyshMessages.Match(message, messagePattern));
        }
        catch (Exception)
        {
            Console.WriteLine("Message match error:");
            Console.WriteLine($"Message to be compared: '{message}'");
            Console.WriteLine($"Message pattern: '{messagePattern}'");
            throw;
        }
    }
    
    public static void MatchMessage(string message, string messagePattern, params string[] values)
    {
        Assert.IsTrue(ClyshMessages.Match(message, messagePattern, values));
    }
}