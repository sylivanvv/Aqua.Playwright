using Aqua.AppConfig.Configuration;
using Aqua.Automation.QaBrains.PageObjects;
using Aqua.Framework.Extensions;
using Microsoft.Playwright;

namespace Aqua.Automation.AuthHelpers;

public class QaBrainsUiAuthStrategy(IConfig config) : IAuthStrategy
{
    public async Task GenerateStateAsync(IPlaywright playwright, IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        var loginPage = await page.OpenPageAsync<LoginPage>(config.Env.QaBrainsBaseUrl);
        await loginPage.LoginAsync(config.AuthData.QaBrainsUser.UserName, config.AuthData.QaBrainsUser.Password);
        await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = config.LoginCookieStoragePathQaBrains });
    }  
}