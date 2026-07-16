using Aqua.Framework.Pages;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class BaseQaBrainsAlert(IPage page) : BaseAlert(page)
{   
    public override async Task WaitForLoadedAsync() => await AlertCancelBtn.WaitForAsync();
    public override ILocator AlertCancelBtn => Page.Locator("//button[contains(@class, 'bg-gray-200') and(@data-slot = 'dialog-close')]").First;
    public override ILocator AlertOkBtn => Page.Locator("//button[contains(@class, 'bg-red-500') and (@data-slot = 'dialog-close')]").First;
    public override ILocator AlertText => Page.Locator("//p[@ng-bind-html = 'mCtrl.message']");
}