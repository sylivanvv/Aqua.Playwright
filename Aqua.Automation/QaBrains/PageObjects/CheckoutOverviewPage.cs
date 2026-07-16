using Aqua.Automation.QaBrains.PageObjects.Tables;
using Aqua.Automation.Utils;
using Aqua.Framework.Components.Table;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class CheckoutOverviewPage(IPage page) : MainPageHeader(page)
{
    public override async Task WaitForLoadedAsync() => await CheckOutOverviewLabel.WaitForAsync();
    public ILocator CheckOutOverviewLabel => Page.GetByRole(AriaRole.Heading,  new PageGetByRoleOptions {Name = "Checkout: Overview" });
    public ILocator FinishLink => Page.GetByRole(AriaRole.Button,  new PageGetByRoleOptions {Name = "Finish" });
    public ILocator AllItemsTotalLabel => Page.Locator("//p[contains(., 'Item Total')]");
    public ILocator TaxLabel => Page.Locator("//p[contains(., 'Tax')]");
    public ILocator TotalLabel => Page.Locator("//div[contains(@class, 'summery')]//p[contains(., 'Total')][last()]");
        
    public async Task<decimal> GetAllItemsTotalAsync() => (await AllItemsTotalLabel.InnerTextAsync()).ToPrice();
    public async Task<decimal> GetTaxAsync() => (await TaxLabel.InnerTextAsync()).ToPrice();
    public async Task<decimal> GetGrandTotalAsync() => (await TotalLabel.InnerTextAsync()).ToPrice();
        
    //Goods table
    public Table<CheckoutItemRow> GoodsTableInCheckout => new(Page.Locator("//div[@id = 'cart' or (@id = 'checkout-overview')]"), 
        loc => new CheckoutItemRow(loc), "xpath=.//div[contains(@class,'cart-list')]/div");
}