using Aqua.Automation.Utils;
using Aqua.Framework.Components;
using Aqua.Framework.Components.Table;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class GoodsListPage(IPage page) : MainPageHeader(page)
{   
    public override async Task WaitForLoadedAsync()
    {
        await base.WaitForLoadedAsync();
        await GoodsTable.GetFirstRow().Locator.WaitForAsync();
    }
    public Dropdown SortingSelection => new(Page.Locator("//button[@role = 'combobox']"),
        Page.Locator("//div[@role = 'option']"));

    //Goods table
    public Table<GoodsRow> GoodsTable => new(Page.Locator("//div[contains(@class,'products')]"), 
        loc => new GoodsRow(loc), "//div[contains(@class,'flex-col')]");
    public class GoodsRow(ILocator locator) : BaseTableRow(locator), IComparableRow<GoodsRowModel>
    {
        public ILocator GoodsName => Locator.Locator("a.text-lg");
            
        public ILocator Price => Locator.Locator("span.text-lg");
        
        public ILocator AddToFavsButton => Locator.Locator("xpath=.//button[contains(@class, 'cursor-pointer')]").First;
            
        public ILocator AddedToFavsLabel => Locator.Locator("xpath=.//button[@style = 'color: rgb(255, 0, 0);']");
        
        public ILocator AddToCartButton => Locator.Locator("xpath=.//button[. = 'Add to cart']");
        
        public async Task<bool> IsGoodsAddedToFavAsync() => await AddedToFavsLabel.CountAsync() > 0;
        
        public async Task<GoodsListPage> AddGoodsInFavAsync() => await AddToFavsButton.OpenPageAsync<GoodsListPage>();

        public async Task<GoodsListPage> AddGoodsInCartAsync() => await AddToCartButton.OpenPageAsync<GoodsListPage>();
            
        public async Task<GoodsRowModel> AsDataAsync() => new(
            Name: await GoodsName.InnerTextAsync(),
            Price: (await Price.InnerTextAsync()).ToPrice());
    }
}
public record GoodsRowModel(string Name, decimal Price);