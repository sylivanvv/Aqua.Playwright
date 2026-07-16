using Aqua.Framework.Extensions;
using Aqua.Framework.Pages;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class WindowsAndIFramesPage(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await OpenCustomModalLink.WaitForAsync();
        
    public  IFrameLocator IFrameContainer => Page.FrameLocator("#basic-iframe");
    public  ILocator OpenCustomModalLink => Page.Locator("#open-modal");
    public  ILocator OpenBrowserAlertDialogButton => Page.Locator("#trigger-alert");
    public  ILocator OpenBrowserConfirmDialogButton => Page.Locator("#trigger-confirm");
    public  ILocator OpenBrowserPromptDialogButton => Page.Locator("#trigger-prompt");
    public  ILocator OpenBasicPopupButton => Page.Locator("#open-basic-popup");
    public  ILocator OpenSmallPopupButton => Page.Locator("#open-small-popup");
    public  ILocator OpenInNewTabButton => Page.Locator("#open-new-tab");
    public  ILocator SuccessfulIFrameLabel => Page.Locator("#iframe-message-display");
    public  ILocator SuccessfullyClosedBasicPopup => Page.Locator("//h4[. = 'Open Windows/Tabs:' ]/following-sibling::div//*[text() = 'Basic Popup Window']");
    public  ILocator SuccessfullyClosedSmallPopup => Page.Locator("//h4[. = 'Open Windows/Tabs:' ]/following-sibling::div//*[text() = 'Small Popup']");
    public  ILocator SuccessfullyClosedNewTab => Page.Locator("//h4[. = 'Open Windows/Tabs:' ]/following-sibling::div//*[text() = 'New Tab Content']");
    public async Task<BaseLearnQaAlert> OpenCustomModal() => await OpenCustomModalLink.OpenPageAsync<BaseLearnQaAlert>();
    public async Task<WindowsAndIFramesPage> OpenBrowserAlertDialog()
    {
        await OpenBrowserAlertDialogButton.ClickAsync();
        return this;
    }
       
    public async Task<WindowsAndIFramesPage> OpenBrowserConfirmDialog()
    {
        await OpenBrowserConfirmDialogButton.ClickAsync();
        return this;
    }
        
    public async Task<WindowsAndIFramesPage> OpenBrowserPromptDialog()
    {
        await OpenBrowserPromptDialogButton.ClickAsync();
        return this;
    }

    public async Task<WindowPopup> OpenBasicPopupInAnotherWindowAsync() =>
        await Page.OpenInNewTabAsync<WindowPopup>(async () =>
            await OpenBasicPopupButton.ClickAsync());

    public async Task<WindowPopup> OpenSmallPopupInAnotherWindowAsync() =>
        await Page.OpenInNewTabAsync<WindowPopup>(async () =>
            await OpenSmallPopupButton.ClickAsync());

    public async Task<FormInAnotherTab> OpenPopupInAnotherTabAsync() =>
        await Page.OpenInNewTabAsync<FormInAnotherTab>(async () =>
            await OpenInNewTabButton.ClickAsync());

    public IFrameOnIFramesAndWindowsPage OpenIFrame() => new(IFrameContainer);
}