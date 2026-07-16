using Aqua.Framework.Components;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class WindowPopup(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await PopupInputField.WaitForAsync();
        
    public  ILocator ClosePopupLink => Page.Locator("#popup-close");
    public  ILocator PopupInputField => Page.Locator("#popup-input");
        
    public Dropdown SelectInPopup => new(Page.Locator("#popup-select"),
        Page.Locator("//option[contains(@value, 'option')]"));
    public  ILocator OpenBrowserConfirmDialogButton => Page.Locator("#trigger-confirm");

    public async Task FillFormAndReturn(string text, string option = "")
    {
        await PopupInputField.FillAsync(text);
        if (string.IsNullOrEmpty(option)) await SelectInPopup.SelectRandomOptionInSelectAsync();
        else await SelectInPopup.SelectOptionAsync(option);
        await ClosePopupLink.ClickAsync();
    }
}