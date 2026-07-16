using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.LearnQa.PageObjects.Pages;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.UiTests.Tests;

[Parallelizable(ParallelScope.All)]
[AllureFeature("Keyboard and Mouse Interactions")]
public class KeyboardAndMouseEventsTest : BaseLearnQaTest
{
    private KeyboardMouseEventsPage _keyboardMouseEventsPage;

    [SetUp]
    [AllureBefore("Open Keyboard and mouse events page")]
    public async Task SetUp() => _keyboardMouseEventsPage =
        await Page.OpenPageAsync<KeyboardMouseEventsPage>(LearnQaUrlCreator.KeyboardMouseEventsPage);

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Keyboard Events: Clear field using Backspace key")]
    [AllureName("Use Backspace key to clear an input field")]
    public async Task UseBackSpaceToClearField()
    {
        await AllureApi.Step("1. Act: Press Backspace key until field is empty",
            async () => { await _keyboardMouseEventsPage.UseBackspaceToClearFieldAsync(); });
        await AllureApi.Step("2. Assert: Assert success notification is shown",
            async () =>
            {
                await Assertions.Expect(_keyboardMouseEventsPage.BackspaceClearSuccess, "Field wasn't cleared")
                    .ToBeVisibleAsync();
            });
    }

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Keyboard Events: use enter to open modal and esc to close")]
    [AllureName("Operate a modal dialog using keyboard keys")]
    public async Task OperateModalWithKeyboard()
    {
        await AllureApi.Step("1. Act: Press Enter and ESC keys to open and close modal",
            async () => { await _keyboardMouseEventsPage.OperateModalWithKeysAsync(); });
        await AllureApi.Step("2. Assert: Assert success notification is shown",
            async () =>
            {
                await Assertions.Expect(_keyboardMouseEventsPage.ModalSuccess, "Modal wasn't opened or closed")
                    .ToBeVisibleAsync();
            });
    }


    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Mouse Events: Double click to edit")]
    [AllureName("Double click an element to enable editing mode")]
    public async Task DoubleClickToEditElement()
    {
        await AllureApi.Step("1. Act: Double click on element to enable editing mode",
            async () => { await _keyboardMouseEventsPage.DoubleClickToEditAsync(); });
        await AllureApi.Step("2. Assert: Verify success notification is shown",
            async () =>
            {
                await Assertions.Expect(_keyboardMouseEventsPage.DoubleClickSuccess, "Field wasn't edited after double click")
                   .ToBeVisibleAsync();
            });
    }


    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Mouse Events: Hover to show content")]
    [AllureName("Hover over a card to show hidden content")]
    public async Task HoverToShowHiddenContent()
    {
        await AllureApi.Step("1. Act: Hover over element to show hidden content",
           async () =>
           {
               await _keyboardMouseEventsPage.HoverOverCardAsync();
           });
        await AllureApi.Step("2. Assert: Verify success notification is shown",
           async () =>
           {
               await Assertions.Expect(_keyboardMouseEventsPage.HoverableSuccess, "Hover failed")
                   .ToBeVisibleAsync();
           });
    }
}