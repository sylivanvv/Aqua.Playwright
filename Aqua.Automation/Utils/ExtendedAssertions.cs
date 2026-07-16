using System.Runtime.CompilerServices;
using Aqua.Framework.Core;
using Aqua.Framework.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Aqua.Automation.Utils;

//[E] - expected ; [A] - actual
public static class ExtendedAssertions
{
    private static ILogger Log => AquaServices.LoggerFactory.CreateLogger(nameof(ExtendedAssertions));

    public static void Multiple(Action assertions) 
    {
        using (Assert.EnterMultipleScope())
            assertions();
    } 

    public static void AreEqual<T>(T expected, T actual, string verifyMessage = "Objects should be equal",
        bool softVerify = false, [CallerArgumentExpression("expected")] string expectedExpression = "",
        [CallerArgumentExpression("actual")] string actualExpression = "")
    {
        var testLog = $"[E] {expectedExpression} - '{expected}' == [A] {actualExpression} - '{actual}'";
        Log.Info(testLog);
        try
        {
            Assert.That(actual, Is.EqualTo(expected), testLog + $", but {verifyMessage}");
        }
        catch
        {
            if (!softVerify) throw;
        }
    }

    public static void AreCollectionsEqual<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual,
        string verifyMessage = "Collections should be equal",
        [CallerArgumentExpression(nameof(expected))] string expectedExpression = "",
        [CallerArgumentExpression(nameof(actual))] string actualExpression = "")
    {
        var expString = string.Join(", ", expected);
        var actString = string.Join(", ", actual);
        
        var testLog = $"[E] {expectedExpression} - [{expString}] == [A] {actualExpression} - [{actString}]";
        Log.Info(testLog);
        actual.Should().BeEquivalentTo(expected, testLog + verifyMessage);
    }
    
    public static void IsTrue(bool actual, string verifyMessage = "Should be true", bool softVerify = false,
        [CallerArgumentExpression("actual")] string actualExpression = "")
    {
        var testLog = $"'{actualExpression}' - {actual}";
        Log.Info(testLog);
        try
        {
            Assert.That(actual, Is.True, testLog + $", But {verifyMessage}");
        }
        catch
        {
            if(!softVerify) throw;
        }
    }
    
    public static void IsFalse(bool actual, string verifyMessage = "Should be false", bool softVerify = false,
        [CallerArgumentExpression(nameof(actual))] string actualExpression = "")
    {
        var testLog = $"'{actualExpression}' - {actual}";
        Log.Info(testLog);
        try
        {
            Assert.That(actual, Is.False, testLog + $", But {verifyMessage}");
        }
        catch
        {
            if(!softVerify) throw;
        }
    }
    
    public static void AreDecimalsEqual(decimal expected, decimal actual, decimal precision = 0.01m, string verifyMessage = "should be equal", 
        bool softVerify = false, [CallerArgumentExpression(nameof(expected))] string expectedExpression = "",
        [CallerArgumentExpression(nameof(actual))] string actualExpression = "")
    {
        var testLog = $"[E] {expectedExpression} - '{expected}' == [A] {actualExpression} - '{actual}'";
        Log.Info(testLog);
        try
        {
            Assert.That(actual, Is.EqualTo(expected).Within(precision), testLog + $", but {verifyMessage}");
        }
        catch
        {
            if (!softVerify) throw;
        }
    }
}