using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.AuthHelpers;
using Aqua.Automation.LearnQa.PageObjects.CapybarasPages;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.UiTests.Tests;

[Parallelizable(ParallelScope.All)]
[AllureFeature("Browser Errors handling")]
[WithAuth(AuthRole.LearnQaUser)]
public class BrowserErrorsTest : BaseLearnQaTest
{
    [SetUp]
    [AllureBefore("Open Capybaras page")]
    public async Task SetUp() => await Page.OpenPageAsync<CapybarasBasePage>(LearnQaUrlCreator.CapybarasBasePage);


    [Test]
    [AllureStory("Console error detection")]
    [AllureName("Test should catch 500 network error logged to console")]
    public async Task ShouldCatchHttp500ConsoleError()
    {
        const string error500 = "/api/error500";
        await AllureApi.Step("1. Arrange: Intercept API request and forcefully return a 500 status", async () =>
        {
            await Page.RouteAsync($"**{error500}", route => route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 500,
                Body = "Internal Server Error"
            }));
        });

        await AllureApi.Step("2. Act: Execute JS fetch on the page that logs to console.error upon failure", async () =>
        {
            await Page.EvaluateAsync($$"""
                                     async () => {
                                             const response = await fetch('{{error500}}');
                                             if (!response.ok) {
                                                 console.error('API Error: Status 500');
                                             }
                                         }
                                     """);
        });

        AllureApi.Step("3. Assert: Verify that the 500 error was successfully captured", () =>
        {
            var hasError = BrowserErrors.Any(error => error.Contains("API Error: Status 500"));
            ExtendedAssertions.IsTrue(hasError,
                "The expected 500 error was not found in the browser logs (BrowserErrors).");
        });
    }

    [Test]
    [AllureStory("Network error monitoring")]
    [AllureName("Browser should catch 500 network errors")]
    public async Task ShouldCaptureServerErrorResponse()
    {
        const string testUrl = "/api/mock-500";

        await AllureApi.Step("1. Arrange: Intercept request and simulate real server 500 crash", async () =>
        {
            await Page.RouteAsync($"**{testUrl}", route => route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 500,
                Body = "Server crashed"
            }));
        });

        await AllureApi.Step("2. Act: Trigger a background network request from the client side", async () =>
        {
            await Page.EvaluateAsync($$"""
                                       async () => {
                                                       try {
                                                           await fetch('{{testUrl}}');
                                                       } catch (e) {
                                                       }
                                                   }
                                       """);

        });
        AllureApi.Step("Verify that the 500 error was successfully captured", () =>
        {
            var hasError = BrowserErrors.Any(error => error.Contains("Network Error: 500"));
            ExtendedAssertions.IsTrue(hasError,
                "The expected 500 error was not found in the browser logs (BrowserErrors).");
        });
    }
}