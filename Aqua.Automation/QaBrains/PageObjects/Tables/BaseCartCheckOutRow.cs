using Aqua.Automation.Utils;
using Aqua.Framework.Components.Table;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects.Tables;

public class BaseCartCheckOutRow(ILocator locator) : BaseTableRow(locator), IComparableRow<CartRowModel>
{    
    public ILocator ItemName => Locator.Locator("h3");
    public ILocator Price => Locator.Locator("xpath=.//p[. = 'Price']/following-sibling::p");
    public ILocator Quantity => Locator.Locator("xpath=.//p[. = 'Quantity']/following-sibling::div//span");
    public ILocator Total => Locator.Locator("xpath=.//p[. = 'Total']/following-sibling::p");

    public async Task<CartRowModel> AsDataAsync() => new(
        Name: await ItemName.InnerTextAsync(),
        Price: (await Price.InnerTextAsync()).ToPrice(),
        Quantity: (await Quantity.InnerTextAsync()).ToInt(),
        Total: (await Total.InnerTextAsync()).ToPrice()
    );
}

public record CartRowModel(string Name, decimal Price, int Quantity, decimal Total);