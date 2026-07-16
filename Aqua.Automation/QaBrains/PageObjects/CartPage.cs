using Aqua.Automation.QaBrains.PageObjects.Tables;
using Aqua.Framework.Components.Table;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class CartPage(IPage page) : MainPageHeader(page)
{
    public override async Task WaitForLoadedAsync() => await YourCartLabel.WaitForAsync();

    public ILocator YourCartLabel => Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions {Name = "Your Cart", Exact = true });
    public ILocator CheckoutLink => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions {Name = "Checkout" });
        
    public async Task<CheckoutForm> OpenCheckoutFormAsync() => await CheckoutLink.OpenPageAsync<CheckoutForm>();

    //Goods table
    public Table<CartItemsRow> GoodsTableInCart => new(Page.Locator("#cart"), 
        loc => new CartItemsRow(loc), "xpath=.//div[contains(@class,'cart-list')]/div");
}