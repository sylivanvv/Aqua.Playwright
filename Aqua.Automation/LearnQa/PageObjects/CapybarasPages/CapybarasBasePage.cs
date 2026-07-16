using Aqua.Automation.LearnQa.CapybaraApi;
using Aqua.Automation.LearnQa.Models.Capybara;
using Aqua.Automation.LearnQa.PageObjects.Pages;
using Aqua.Framework.Components;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.CapybarasPages;

public class CapybarasBasePage(IPage page) : BaseLearnQaPage(page)
{  
    public override async Task WaitForLoadedAsync() => 
        await AddCapybaraIfEmptyLink.Or(CapybaraSelectorButton).WaitForAsync();

    private ILocator AddCapybaraIfEmptyLink => Page.Locator("//button[@aria-label = 'Create your first capybara']");
    private ILocator PageLabel => Page.Locator("//h1[. = 'Self-Healing Testing Practice']");
    private ILocator CapybaraSelectorButton => Page.Locator("//button[@aria-labelledby = 'capybara-selector-label']");
     

    public async Task<CreateCapybaraPopup> OpenCreateNewCapybaraPopupAsync()
    {
        if (!await CapybaraSelectorButton.IsVisibleAsync())
            return await AddCapybaraIfEmptyLink.OpenPageAsync<CreateCapybaraPopup>();
        await CapybaraSelectorButton.ClickAsync();
        return await CreateNewCapybaraInSelectorLink.OpenPageAsync<CreateCapybaraPopup>();
    }

    public Dropdown CapybaraSelector => new(Page.Locator("//button[@aria-labelledby = 'capybara-selector-label']"),
        Page.Locator("//div[contains(@id, 'capybara-dropdown')]//li//div"));

    public async Task<CapybarasBasePage> SelectCapybaraViewAsync(string name)
    {
        await CapybaraSelector.SelectOptionAsync(name);
        return this;
    }
        
    public ILocator CreateNewCapybaraInSelectorLink => Page.Locator("//button[. = 'Create New Capybara']");
    private ILocator ScheduleTableLink => Page.Locator("//button[contains(@id, 'tab-schedule')]");
    private ILocator PoolsListLink => Page.Locator("//button[contains(@id, 'tab-pools')]");
    private ILocator FoodSectionLink => Page.Locator("//button[contains(@id, 'tab-food')]");
    public ILocator FriendsPageLink => Page.Locator("//button[contains(@id, 'tab-friends')]");

    //Capybara info
        
    public ILocator CapybaraName => Page.Locator("//div[contains(@id, 'capybara-info-name')]//span[last()]");
    private ILocator CapybaraAge => Page.Locator("//div[contains(@id, 'capybara-info-age')]//span[last()]");
    private ILocator CapybaraMood => Page.Locator("//div[contains(@id, 'capybara-info-mood')]//span[last()]");
    private ILocator CapybaraHappiness => Page.Locator("//div[contains(@id, 'capybara-info-happiness')]//span[last()]");
    public ILocator CapybaraFavoriteFood => Page.Locator("//div[contains(@id, 'capybara-info-favorite')]//span[last()]").First;
    public ILocator CapybaraBio => Page.Locator("//div[contains(@id, 'capybara-info-bio')]//span[last()]").First;
    private ILocator CapybaraFriends => Page.Locator("//div[contains(@id, 'capybara-info-friends')]//span[last()]");
    private ILocator CapybaraLastFed => Page.Locator("//div[contains(@id, 'capybara-info-lastfed')]//span[last()]");

    public async Task<CapybaraModel> GetCapybaraModelFromPageAsync() => new()
    {
        Name = await CapybaraName.InnerTextAsync(),
        Age = Convert.ToInt32((await CapybaraAge.InnerTextAsync()).Split(" ")[0]),
        Bio = await CapybaraBio.InnerTextAsync(),
        FavoriteFood = Enum.Parse<FoodEnum>((await CapybaraFavoriteFood.InnerTextAsync()).Split(" ")[1])
    };

}