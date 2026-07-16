using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.QaBrains.PageObjects;

public class LoginPage(IPage page) : BaseQaBrainsPage(page)
{
    private ILocator EmailInput => Page.Locator("#email");
    private ILocator PasswordInput => Page.Locator("#password");
    private ILocator LoginButton => Page.Locator("//button[@type='submit']");
    public ILocator WrongDataGrowl => Page.Locator("//li[@data-type='error']");

    public override async Task WaitForLoadedAsync() =>
        await Task.WhenAll(
            EmailInput.WaitForAsync(),
            PasswordInput.WaitForAsync(),
            LoginButton.WaitForAsync()
        );

    public async Task FillLoginDataAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
    }

    public async Task<GoodsListPage> LoginAsync(string email, string password)
    {
        await FillLoginDataAsync(email, password);
        return await LoginButton.OpenPageAsync<GoodsListPage>();
    }
    
    public async Task LoginInvalidDataAsync(string email, string password)
    {
        await FillLoginDataAsync(email, password);
        await LoginButton.ClickAsync();
    }
}