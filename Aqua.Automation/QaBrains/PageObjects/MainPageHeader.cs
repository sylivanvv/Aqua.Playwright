using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class MainPageHeader(IPage page) : BaseQaBrainsPage(page)
{
    public override async Task WaitForLoadedAsync()
    {
        await base.WaitForLoadedAsync();
        await UserProfile.WaitForAsync();
    }
        
    private ILocator CartLink => Page.Locator("//span[@role = 'button']").First;
    internal ILocator UserProfile => Page.Locator("//button[contains(@id,'radix')]//span[contains(@class, 'user-name')]");
    private ILocator FavoritePageLink => Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions {Name = "Favorites"});
        
    public ILocator LogoutLink => Page.Locator("//div[@role = 'menu']//button[@data-slot='dialog-trigger']");
    public ILocator ItemsInCartCountLabel => Page.Locator("//span[contains(@class, 'bg-qa-clr')]");
        
    public async Task<int> GetItemsInCartCountAsync() => Convert.ToInt32(await ItemsInCartCountLabel.InnerTextAsync());

    public async Task<GoodsListPage> OpenUserFavoriteGoodsAsync()
    {
        await UserProfile.ClickAsync();
        return await FavoritePageLink.OpenPageAsync<GoodsListPage>();
    }
    public async Task<CartPage> OpenUserCartAsync() => await CartLink.OpenPageAsync<CartPage>();
        
    public async Task<bool> IsPageDisplayedAsync() => await CartLink.IsElementVisibleAfterWaitAsync() 
                                                      && await UserProfile.IsElementVisibleAfterWaitAsync();
}