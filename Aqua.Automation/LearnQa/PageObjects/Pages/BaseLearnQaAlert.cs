using Aqua.Framework.Pages;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class BaseLearnQaAlert(IPage page) : BaseAlert(page)
{
    public override async Task WaitForLoadedAsync() => await AlertCancelBtn.WaitForAsync();
    public override ILocator AlertCancelBtn => Page.Locator("//button[. = 'Cancel']").First;
    public override ILocator AlertOkBtn => Page.Locator("//button[@id = 'modal-action']").First;
    public override ILocator AlertText => Page.Locator("//p[@ng-bind-html = 'mCtrl.message']");
}