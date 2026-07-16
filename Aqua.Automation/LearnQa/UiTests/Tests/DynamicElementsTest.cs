using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.LearnQa.PageObjects.Pages;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Extensions;
using Aqua.Framework.Utils;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.UiTests.Tests;

[Parallelizable(ParallelScope.All)]
[AllureEpic("UI Tests")]
[AllureFeature("Dynamic Elements Handling")]
public class DynamicElementsTest : BaseLearnQaTest
{
    private DynamicElementsPage _dynamicPage;

    [SetUp]
    [AllureBefore("Open Dynamic Elements page")]
    public async Task SetUp() =>
        _dynamicPage = await Page.OpenPageAsync<DynamicElementsPage>(LearnQaUrlCreator.DynamicElementsPage);

    [Test]
    [AllureStory("User can see elements loaded with delay")]
    [AllureName("Wait for Lazy Elements")]
    [Category(TestCategories.LearnQa)]
    public async Task LazyElements()
    {
        await AllureApi.Step("1. Act: Click lazy element trigger",
            async () => await _dynamicPage.ClickDelayedButton());
        await AllureApi.Step("2. Assert: Lazy element is shown", async () =>
            await Assertions.Expect(_dynamicPage.DelayedItem)
                .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 6000 }));
    }

    [Test]
    [AllureStory("User can see elements loaded via AJAX")]
    [AllureName("Wait for AJAX Elements")]
    [Category(TestCategories.LearnQa)]
    public async Task AjaxElements()
    {
        await AllureApi.Step("1. Act: Click ajax element trigger",
            async () =>
                await _dynamicPage.ClickButtonAndCheckIfAjaxItemsAppearsAsync());
        await AllureApi.Step("2. Assert: Ajax element is shown", async () =>
            await Assertions.Expect(_dynamicPage.AjaxTable.GetFirstRow().Locator)
                .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 7000 }));
    }

    [Test]
    [AllureStory("User can scroll infinitely to load more rows")]
    [AllureName("Infinite Scroll Validation")]
    [Category(TestCategories.LearnQa)]
    public async Task InfiniteScroll()
    {
        var randomElementIndex = RandomGenerator.GetRandomInt(1, 50);
        ExtendedAssertions.IsTrue(await _dynamicPage.IsElementWithIndexShownAfterScroll(randomElementIndex),
            $"Requested row with index - {randomElementIndex} wasn't displayed after scrolling");
    }

    [Test]
    [AllureStory("User can scroll list of images")]
    [AllureName("Lazy loading images")]
    [Category(TestCategories.LearnQa)]
    public async Task ImagesScroll()
    {
        var randomImage = await AllureApi.Step("1. Act: Click ajax element trigger",
            async () =>
            {
                var images = await _dynamicPage.GetListOfImagesAsync();
                var randomImage = images.RandomElement();
                await randomImage.ScrollIntoViewIfNeededAsync();
                return randomImage;
            });
        await AllureApi.Step("2. Assert: Check if image was loaded",
            async () =>
                await Assertions.Expect(randomImage.Locator("img"), "Images wasn't found").ToBeVisibleAsync());
    }

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("User can reveal hidden elements")]
    [AllureName("Reveal Hidden Elements")]
    public async Task RevealHidden()
    {
        await AllureApi.Step("1. Act & Assert: Click element trigger and check is dynamic element is displayed",
            async () =>
            {
                await Assertions.Expect(_dynamicPage.HiddenItem).Not.ToBeVisibleAsync();
                await _dynamicPage.ClickToRevealHiddenElementAsync();
                await Assertions.Expect(_dynamicPage.HiddenItem).ToBeVisibleAsync();
            });
        await AllureApi.Step("2. Act & Assert: Click dynamic element and check that it generates random text",
            async () =>
            {
                for (var i = 0; i <= 3; i++)
                    ExtendedAssertions.IsFalse(
                        string.IsNullOrEmpty(await _dynamicPage.GenerateAndGetDynamicContentAsync()));
            });
    }
}