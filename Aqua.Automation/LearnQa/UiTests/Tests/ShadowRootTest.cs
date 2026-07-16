using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.LearnQa.PageObjects.Pages;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.UiTests.Tests;

[Parallelizable(ParallelScope.All)]
[AllureFeature("Shadow DOM Interaction")]
public class ShadowRootTest : BaseLearnQaTest
{
    private ShadowRootPage _shadowRootPage;

    [SetUp]
    [AllureBefore("Open ShadowRoot page")]
    public async Task SetUp() => _shadowRootPage = await Page.OpenPageAsync<ShadowRootPage>(LearnQaUrlCreator.ShadowRootPage);

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Interact with Element inside Shadow Root")]
    [AllureName("Interact with input field and button inside Shadow DOM")]
    public async Task InteractWithElementInsideShadowRoot()
    {
        const string shadowDomText = "If you see this, shadow DOM works!";
        await AllureApi.Step("1. Act: Interact with Element inside Shadow Root",
            async () => await _shadowRootPage.ClickButtonInShadowDomAsync());
        await AllureApi.Step("2. Assert: Verify the text inside Shadow DOM is updated",
            async () =>
            {
                await Assertions.Expect(_shadowRootPage.ShadowTextElement).ToHaveTextAsync(shadowDomText);
                await Assertions.Expect(_shadowRootPage.ShadowRootSuccess, "Shadow Dom button wasn't clicked")
                    .ToBeVisibleAsync();
            });
    }
}