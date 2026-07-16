using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

internal class LoginPageHardPage(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await EmailInput.WaitForAsync();
        
    public ILocator EmailInput => Page.Locator("//input[@type = 'email']");
    private ILocator PasswordInput => Page.Locator("//input[@type = 'password']");
    private ILocator SignInLink => Page.Locator("//button[@type = 'submit']");

    public async Task<BaseLearnQaPage> SignInAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        return await SignInLink.OpenPageAsync<BaseLearnQaPage>();
    }
}