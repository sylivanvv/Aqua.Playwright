using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class ShadowRootPage(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await CreateBasicShadowRootButton.WaitForAsync();
    public  ILocator CreateBasicShadowRootButton => Page.Locator("#create-basic-shadow");
    public  ILocator ShadowRootSuccess => Page.Locator("//h4[contains(., 'Basic Shadow DOM')]/following-sibling::span[contains(., 'Completed')]");
    protected ILocator ShadowHostElement => Page.Locator("#shadow-host-element");

    protected ILocator ShadowButtonElement => ShadowHostElement.Locator("#shadow-btn");

    public ILocator ShadowTextElement => ShadowHostElement.Locator("p");

    public async Task ClickButtonInShadowDomAsync()
    {
        await CreateBasicShadowRootButton.ClickAsync();
        await ShadowButtonElement.ClickAsync();
    }
}