using Aqua.Automation.LearnQa.PageObjects.CapybarasPages;
using Aqua.Framework.Extensions;
using Aqua.Framework.Pages;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class BaseLearnQaPage(IPage page) : BasePage(page)
{
    public override async Task WaitForLoadedAsync() => await FileOperationsLink.WaitForAsync();
    private ILocator FileOperationsLink => Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "File Operations" });
    private ILocator CapybarasPageLink => Page.Locator("//a[@href = '/self-healing-testing/']");
    private ILocator CloseCookies => Page.Locator("//button[. = 'Essential Only']");
    public ILocator WrongDataGrowl => Page.Locator("//li[@data-type='error']");
    public ILocator MainDashboardContainer => Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Main" });
    
    public async Task<FileLoadingPage> OpenFileOperationsPageAsync() => await FileOperationsLink.OpenPageAsync<FileLoadingPage>();
    public async Task<CapybarasBasePage> OpenCapybarasPageAsync() => await CapybarasPageLink.OpenPageAsync<CapybarasBasePage>();

    public async Task<BaseLearnQaPage> CloseCookiesWindowAsync()
    {
        await CloseCookies.ClickAsync();
        return this;
    }
}