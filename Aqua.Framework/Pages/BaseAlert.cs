using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Framework.Pages;

public class BaseAlert(IPage page) : BasePage(page)
{
    public override async Task WaitForLoadedAsync() => await AlertCancelBtn.WaitForAsync();
    public virtual ILocator AlertCancelBtn => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions {Name = "Cancel"});
    public virtual ILocator AlertOkBtn => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions {Name = "Confirm"});
    public virtual ILocator AlertText => Page.Locator("//p");

    public async Task<bool> IsAlertPresentAsync() => await AlertCancelBtn.IsVisibleAsync();

    public async Task AcceptAlertAsync() => await AlertOkBtn.ClickAsync();

    public virtual async Task<TPageType> AcceptAndOpenPageAsync<TPageType>() where TPageType : BasePage => 
        await AlertOkBtn.OpenPageAsync<TPageType>();

    public async Task DeclineAlertAsync() => await AlertCancelBtn.ClickAsync();

    public async Task<TPageType> DeclineAndOpenPageAsync<TPageType>() where TPageType : BasePage => 
        await AlertCancelBtn.OpenPageAsync<TPageType>();
}