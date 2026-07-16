using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class KeyboardMouseEventsPage(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await DoubleClickButton.WaitForAsync();
    public  ILocator StartBackSpaceScenarioButton => Page.Locator("#start-clear-scenario");
    public  ILocator SearchField => Page.Locator("#search-field");
    public  ILocator BackspaceClearSuccess => Page.Locator("//h4[contains(., 'Clear Pre-filled Field')]/following-sibling::span[contains(., 'Completed')]");

    public  ILocator OpenModalDialog => Page.Locator("#start-dialog-scenario");
    public  ILocator ModalSuccess => Page.Locator("//h4[contains(., 'Dialog Confirmation Flow')]/following-sibling::span[contains(., 'Completed')]");
        

    public  ILocator DoubleClickButton => Page.Locator("#editable-text");
    public  ILocator DoubleClickSuccess => Page.Locator("//h4[contains(., 'Double-click to Edit')]/following-sibling::span[contains(., 'Completed')]");
    public  ILocator HoverableElement => Page.Locator("#hover-card");
    public  ILocator HoverableSuccess => Page.Locator("//h4[contains(., 'Hover Interaction')]/following-sibling::span[contains(., 'Completed')]");
        

    public async Task<KeyboardMouseEventsPage> UseBackspaceToClearFieldAsync()
    {
        await StartBackSpaceScenarioButton.ClickAsync();
        var textToBeCleared = await SearchField.InputValueAsync();
        for (var i = 0; i < textToBeCleared.Length + 1; i++)
        {
            await SearchField.PressAsync("Backspace");
        }
        return this;
    }

    public async Task<KeyboardMouseEventsPage> DoubleClickToEditAsync()
    {
        await DoubleClickButton.DblClickAsync();
        return this;
    }

    public async Task<KeyboardMouseEventsPage> OperateModalWithKeysAsync()
    {
        var keysAlert = await OpenModalDialog.OpenPageAsync<KeysAlert>();
        await keysAlert.PressEnterToConfirmAsync();
        await keysAlert.PressEscToCloseAsync();
        return this;
    }

    public async Task<KeyboardMouseEventsPage> HoverOverCardAsync()
    {
        await HoverableElement.HoverAsync();
        return this;
    }
}