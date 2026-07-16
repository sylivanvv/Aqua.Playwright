using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class DragAndDropPage(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await DropZone.WaitForAsync();

    public ILocator DropZone => Page.Locator("#drop-zone");
    public async Task<IReadOnlyList<string>> GetListOfDraggableElementsAsync() => 
        await Page.Locator("//div[contains(@id, 'item')]//span[@class='font-medium']").AllInnerTextsAsync();
    public async Task<IReadOnlyList<string>> GetListOfDraggableElementsInZoneAsync() => 
        await Page.Locator("//*[@id = 'drop-zone']//div[contains(@id, 'item')]//span[@class='font-medium']").AllInnerTextsAsync();
    
    private ILocator GetDraggableItem(string itemName) => 
        Page.Locator($"//div[contains(@id, 'item')]//span[.='{itemName}']");
    
    public async Task<DragAndDropPage> DragItemInDragZoneAsync(ILocator item)
    {
        await item.DragToAsync(DropZone);
        return this;
    }

    public async Task<List<string>> DragRandomItemsToDragZoneAsync()
    {
        var itemsToDrag = await GetListOfDraggableElementsAsync();
        var randomItems = itemsToDrag.TakeRandomElements();
        foreach (var itemName in randomItems)
            await DragItemInDragZoneAsync(GetDraggableItem(itemName));
        return randomItems;
    }
}