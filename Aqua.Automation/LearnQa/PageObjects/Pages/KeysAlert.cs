using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class KeysAlert(IPage page) : BaseLearnQaAlert(page)
{
    public override async Task WaitForLoadedAsync() => await ConfirmButton.WaitForAsync();
        
    public  ILocator ConfirmButton => Page.Locator("#dialog-confirm");
        
    public async Task<KeysAlert> PressEnterToConfirmAsync()
    {
        await ConfirmButton.PressAsync("Enter");
        return this;
    }

    public async Task PressEscToCloseAsync() => await Page.Keyboard.PressAsync("Escape");
}