using Aqua.Framework.Core;
using Aqua.TestRailIntegration.TestRailIntegration;
using NUnit.Framework.Interfaces;

namespace Aqua.Automation.Utils;

[AttributeUsage(AttributeTargets.Method)]
public class TestRailCaseAttribute(string testCaseId) : Attribute, ITestAction
{
    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest test)
    {
    }

    public void AfterTest(ITest test)
    {
        var config = AquaServices.Config.TestRailConfig;
        if (!config.Enabled) return;

        var result = TestContext.CurrentContext.Result;

        var statusId = result.Outcome.Status switch
        {
            TestStatus.Passed => 1,
            TestStatus.Skipped => 4,
            TestStatus.Failed => 5,
            _ => 5
        };

        var comment = $"""
                       TestRun.
                       Status: {result.Outcome.Status}
                       Message: {result.Message}
                       {result.StackTrace}
                       """;

        var trClient = AquaServices.Get<TestRailApiClient>();

        try
        {
            trClient.AddResultAsync(config.RunId, testCaseId, statusId, comment).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"[TestRail Error]: Error during sending results to TestRail{testCaseId}. Error: {ex.Message}");
        }
    }
}