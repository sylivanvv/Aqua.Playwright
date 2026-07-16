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
[AllureFeature("Windows, tabs and IFrames Handling")]
public class WindowsAndIFramesTest : BaseLearnQaTest
{
    private WindowsAndIFramesPage _windowsAndIFramesPage;

    [SetUp]
    [AllureBefore("Open Windows and IFrame page")]
    public async Task SetUp() => _windowsAndIFramesPage = await Page
        .OpenPageAsync<WindowsAndIFramesPage>(LearnQaUrlCreator.IFramesWindowsPage);

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Working with IFrames")]
    [AllureName("Interact with elements inside an IFrame")]
    public async Task VerifyAbilityToWorkWithIFrames()
    {
        await AllureApi.Step("1. Act: Open IFrame, click button inside and return to main context", async () =>
        {
            var iFrame = _windowsAndIFramesPage.OpenIFrame();
            await iFrame.ClickIFrameButtonAndReturn();
        });
        await AllureApi.Step("2. Assert: Verify success label is displayed on the main page",
            async () => await Assertions.Expect(_windowsAndIFramesPage.SuccessfulIFrameLabel).ToBeVisibleAsync());
    }

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Working with Second FullScreen Window")]
    [AllureName("Fill form in fullscreen window and return to the parent")]
    public async Task SwitchToBasicAnotherWindowTest()
    {
        var randomText = RandomGenerator.RandomString(7);
        await AllureApi.Step("1. Act: Open basic popup in another basic window, fill and submit form", async () =>
        {
            var basicPopup = await _windowsAndIFramesPage.OpenBasicPopupInAnotherWindowAsync();
            await basicPopup.FillFormAndReturn(randomText);
        });
        await AllureApi.Step("2. Assert: Verify context returned to the main page",
            async () =>
                await Assertions.Expect(_windowsAndIFramesPage.SuccessfullyClosedBasicPopup).ToBeVisibleAsync());
    }

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Working with Second small Window")]
    [AllureName("Fill form in small window and return to the parent")]
    public async Task SwitchToAnotherSmallWindowTest()
    {
        var randomText = RandomGenerator.RandomString(7);
        await AllureApi.Step("1. Act: Open small fill and submit form", async () =>
        {
            var basicPopup = await _windowsAndIFramesPage.OpenSmallPopupInAnotherWindowAsync();
            await basicPopup.FillFormAndReturn(randomText);
        });
        await AllureApi.Step("2. Assert: Verify context returned to the main page",
            async () =>
                await Assertions.Expect(_windowsAndIFramesPage.SuccessfullyClosedSmallPopup).ToBeVisibleAsync());
    }

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Working with second Tab")]
    [AllureName("Fill form in another tab and return to the parent")]
    public async Task SwitchToAnotherTabTest()
    {
        var randomText = RandomGenerator.RandomString(7);
        var randomMessage = RandomGenerator.RandomString(7);
        await AllureApi.Step("1. Act: Open new tab fill and submit form", async () =>
        {
            var popup = await _windowsAndIFramesPage.OpenPopupInAnotherTabAsync();
            await popup.FillFormAndReturn(randomText, randomMessage);
        });
        await AllureApi.Step("2. Assert: Verify context returned to the main page",
            async () => await Assertions.Expect(_windowsAndIFramesPage.SuccessfullyClosedNewTab).ToBeVisibleAsync());
    }

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Native Browser Dialogs")]
    [AllureName("Handle native browser Alert, Confirm, and Prompt dialogs")]
    public async Task HandleDefaultBrowserAlerts()
    {
        const string expectedAlertMessage = "This is a browser alert dialog!";
        const string expectedConfirmMessage = "Do you want to proceed?";
        const string expectedPromptMessage = "Please enter your name:";

        await AllureApi.Step("1. Handle default Browser Alert", async () =>
        {
            var alertMessage = await Page.RunAndHandleDialogAsync(async () =>
                await _windowsAndIFramesPage.OpenBrowserAlertDialog());
            ExtendedAssertions.AreEqual(expectedAlertMessage, alertMessage);
        });
        await AllureApi.Step("2. Handle default Browser Confirm",
            async () =>
            {
                var alertMessage = await Page.RunAndHandleDialogAsync(async () =>
                    await _windowsAndIFramesPage.OpenBrowserConfirmDialog(), false);
                ExtendedAssertions.AreEqual(expectedConfirmMessage, alertMessage);
                await Page.RunAndHandleDialogAsync(async () => await _windowsAndIFramesPage.OpenBrowserConfirmDialog());
            });
        await AllureApi.Step("3. Handle default Browser Prompt", async () =>
        {
            var alertMessage =
                await Page.RunAndHandleDialogAsync(async () =>
                    await _windowsAndIFramesPage.OpenBrowserPromptDialog(), false);

            ExtendedAssertions.AreEqual(expectedPromptMessage, alertMessage, "Prompt text should match");

            alertMessage =
                await Page.RunAndHandleDialogAsync(async () => await _windowsAndIFramesPage.OpenBrowserPromptDialog(),
                    promptText: RandomGenerator.RandomString(5));

            ExtendedAssertions.AreEqual(expectedPromptMessage, alertMessage, "Prompt text should match");
        });
    }
    
    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Custom Alerts")]
    [AllureName("Handle custom alerts")]
    public async Task CustomAlertsTest()
    {
        await AllureApi.Step("1. Act & Assert: Open custom modal and decline", async () =>
        {
            var alert = await _windowsAndIFramesPage.OpenCustomModal();
            await Assertions.Expect(alert.AlertCancelBtn, "Alert should be present").ToBeVisibleAsync();
            await alert.DeclineAlertAsync();
            await Assertions.Expect(alert.AlertCancelBtn, "Alert shouldn't be present").Not.ToBeVisibleAsync();
        });
        await AllureApi.Step("2. Act & Assert: Open custom modal and accept", async () =>
        {
            var alert = await _windowsAndIFramesPage.OpenCustomModal();
            await alert.AcceptAlertAsync();
            await Assertions.Expect(alert.AlertCancelBtn, "Alert shouldn't be present").Not.ToBeVisibleAsync();
        });
    }
}