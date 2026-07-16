using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects.Tables;

public class CartItemsRow(ILocator locator) : BaseCartCheckOutRow(locator)
{
    public ILocator RemoveItemButton => Locator.Locator("button").First;
    public ILocator QuantityMinusButton => Locator.Locator("xpath=.//button[. = '-']");
    public ILocator QuantityPlusButton => Locator.Locator("xpath=.//button[. = '+']");

    public async Task<CartPage> RemoveItemFromCartAsync()
    {
        var alert = await RemoveItemButton.OpenPageAsync<BaseQaBrainsAlert>();
        return await alert.AcceptAndOpenPageAsync<CartPage>();
    } 

    public async Task<CartItemsRow> AddQuantityAsync(int quantity = 1)
    {
        for (var i = 0; i < quantity; i++)
            await QuantityPlusButton.ClickAsync();
        return this;
    }

    public async Task<CartItemsRow> ReduceQuantityAsync(int quantity = 1)
    {
        for (var i = 0; i < quantity; i++)
        {
            var currentQty = Convert.ToInt32(await Quantity.InnerTextAsync());
            if (currentQty <= 1) break;
            await QuantityMinusButton.ClickAsync();
        }
        return this;
    }

    public async Task<CartPage> ReduceQuantityAndDeleteRowAsync(int quantity = 1)
    {
        for (var i = 1; i < quantity; i++)
        {
            await QuantityMinusButton.ClickAsync();
            if (Convert.ToInt32(await Quantity.InnerTextAsync()) == 2) break;
        }

        var alert = await QuantityMinusButton.OpenPageAsync<BaseQaBrainsAlert>(); 
        return await alert.AcceptAndOpenPageAsync<CartPage>();
    }
}