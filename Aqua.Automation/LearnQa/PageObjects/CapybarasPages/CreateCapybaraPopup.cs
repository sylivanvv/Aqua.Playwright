using Aqua.Automation.LearnQa.Models.Capybara;
using Aqua.Automation.LearnQa.PageObjects.Pages;
using Aqua.Framework.Components;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.CapybarasPages;

public class CreateCapybaraPopup(IPage page) : BaseLearnQaAlert(page)
{
    public override ILocator AlertCancelBtn => Page.Locator("//button[. = 'Cancel']");
    public override ILocator AlertOkBtn => Page.Locator("//button[@type = 'submit']");
        
    private ILocator CapybaraNameInput => Page.Locator("//input[@name = 'name']");
    private ILocator CapybaraAgeInput => Page.Locator("//input[@name = 'age']");
    private ILocator CapybaraBioInput => Page.Locator("//textarea[@name = 'bio']");
        
    private ILocator CapybaraSelectorTrigger => Page.Locator("//label[@id = 'food-label']/following-sibling::div//button[@type = 'button']");
        
    public Dropdown FoodSelector => new(Page.Locator("//label[@id = 'food-label']/following-sibling::div//button[@type = 'button']"),
        Page.Locator("//ul[@aria-labelledby = 'food-label']//li"));

    public async Task<CapybarasBasePage> CreateNewCapybaraAsync(CapybaraModel capybara)
    {
        await CapybaraNameInput.FillAsync(capybara.Name);
        await CapybaraAgeInput.FillAsync(capybara.Age.ToString());
        if (!string.IsNullOrEmpty(capybara.Bio)) 
            await CapybaraBioInput.FillAsync(capybara.Bio);
        await FoodSelector.SelectOptionAsync(capybara.FavoriteFood.ToString());
        return await AlertOkBtn.OpenPageAsync<CapybarasBasePage>();
    }
}