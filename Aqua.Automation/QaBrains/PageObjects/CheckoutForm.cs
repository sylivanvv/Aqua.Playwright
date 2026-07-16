using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class CheckoutForm(IPage page) : MainPageHeader(page)
{
    public override async Task WaitForLoadedAsync() => await CheckoutLabel.WaitForAsync();
    public ILocator CheckoutLabel => Page.Locator("//h3[. = 'Checkout: Your Information']");
    public ILocator Email => Page.Locator("//label[. = 'Email']/following-sibling::input");
    public ILocator FirstNameTextBox => Page.Locator("//label[. = 'First Name']/following-sibling::input");
    public ILocator LastNameTextBox => Page.Locator("//label[. = 'Last Name']/following-sibling::input");
    public ILocator ZipCodeTextBox => Page.Locator("//label[. = 'Zip Code']/following-sibling::input");
    public ILocator ContinueLink => Page.GetByRole(AriaRole.Button,  new PageGetByRoleOptions {Name = "Continue" });
        
    public async Task<CheckoutOverviewPage> OpenCheckoutOverviewAsync() => await ContinueLink.OpenPageAsync<CheckoutOverviewPage>();
}