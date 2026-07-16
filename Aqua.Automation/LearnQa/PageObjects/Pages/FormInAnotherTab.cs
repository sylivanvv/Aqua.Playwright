using Aqua.Framework.Components;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class FormInAnotherTab(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await PopupNameInputField.WaitForAsync();
    public  ILocator CloseTabLink => Page.Locator("//button[. = 'Close Tab']");
    public  ILocator PopupNameInputField => Page.Locator("#tab-name-input");
    public  ILocator PopupMessageInputField => Page.Locator("#tab-message-input");
    public  ILocator SubmitButton => Page.Locator("#tab-submit");

    public Dropdown SelectInPopup => new(Page.Locator("#tab-select"),
        Page.Locator("//option[contains(@value, 'option')]"));
        
    public async Task FillFormAndReturn(string name, string message, string option = "")
    {
        await PopupNameInputField.FillAsync(name);
        await PopupMessageInputField.FillAsync(message);
        if (string.IsNullOrEmpty(option)) await SelectInPopup.SelectRandomOptionInSelectAsync();
        else await SelectInPopup.SelectOptionAsync(option);
        await SubmitButton.ClickAsync();
        await Page.CloseAsync();
    }
}