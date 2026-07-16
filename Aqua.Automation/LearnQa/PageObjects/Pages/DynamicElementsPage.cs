using Aqua.Framework.Components.Table;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class DynamicElementsPage(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await DelayedButton.WaitForAsync();

    public  ILocator DelayedButton => Page.Locator("#trigger-delayed");
    public  ILocator DelayedItem => Page.Locator("#delayed-element");
    public  ILocator LoadAjaxItemsButton => Page.Locator("#load-ajax-data");
    public  ILocator LoadHiddenItemButton => Page.Locator("#reveal-hidden");
    public  ILocator HiddenItem => Page.Locator("#hidden-element");
    public  ILocator GenerateContentButton => Page.Locator("#generate-content");
    public  ILocator DynamicContent => Page.Locator("//div[@id = 'dynamic-content']//p");
    
    public async Task<IReadOnlyList<ILocator>> GetListOfImagesAsync() => 
        await Page.Locator("//div[@data-id]").AllAsync();

    public async Task ClickDelayedButton()
    {
        await DelayedButton.ClickAsync();
    }
    
    public async Task ClickButtonAndCheckIfAjaxItemsAppearsAsync()
    {
        await LoadAjaxItemsButton.ClickAsync();
    }

    public async Task<bool> IsElementWithIndexShownAfterScroll(int targetIndex = 50)
    {
        const int maxAttempts = 20;
        for (var i = 0; i < maxAttempts; i++)
        {
            var currentCount = await InfiniteTable.GetRowsCountAsync();
            if (currentCount >= targetIndex)
            {
                await InfiniteTable.GetRow(targetIndex - 1).Locator.ScrollIntoViewIfNeededAsync();
                return true;
            }
            await InfiniteTable.GetLastRow().Locator.ScrollIntoViewIfNeededAsync();
            await InfiniteTable.WaitUntilRowsCountChangesAsync(currentCount);
        }
        return false;
    }

    public async Task<DynamicElementsPage> ClickToRevealHiddenElementAsync()
    {
        await LoadHiddenItemButton.ClickAsync();
        return this;
    }

    public async Task<string> GenerateAndGetDynamicContentAsync()
    {
        await GenerateContentButton.ClickAsync();
        return await DynamicContent.InnerTextAsync();
    }
    
    public Table<AjaxRow> AjaxTable => new(Page.Locator("//div[contains(@class, 'overflow-y-auto')]"), 
        loc => new AjaxRow(loc), "xpath=.//div[contains(@class, 'ajax-item')]");
    public Table<AjaxRow> InfiniteTable => new(Page.Locator("(//div[contains(@class, 'overflow-y-auto')])[3]"), 
        loc => new AjaxRow(loc), "xpath=.//div[contains(@class, 'scroll-item')]");
    public class AjaxRow(ILocator locator) : BaseTableRow(locator)
    {
        public ILocator RowName => Locator.Locator("h4");
            
        public ILocator Description => Locator.Locator("p");
        
        public ILocator Status => Locator.Locator("span");
    }
}