using Aqua.Framework.Pages;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class BaseQaBrainsPage(IPage page) : BasePage(page)
{
    private ILocator HomeLink => Page.Locator("//a[@href='/ecommerce']");
    public override async Task WaitForLoadedAsync() => await HomeLink.WaitForAsync();
}