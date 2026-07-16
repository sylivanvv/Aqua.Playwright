using Aqua.AppConfig.Configuration;
using Microsoft.Playwright;

namespace Aqua.Automation.AuthHelpers;

public class LearnQaApiAuthStrategy(IConfig config) : IAuthStrategy
{
    public async Task GenerateStateAsync(IPlaywright playwright, IBrowserContext context)
    {
        await using var requestContext = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = config.Env.LearnQaApiUrl
        });
        var response = await requestContext.PostAsync("/api/auth/login", new APIRequestContextOptions
        {
            DataObject = new { email = config.AuthData.LearnQaUser.UserName,
                password = config.AuthData.LearnQaUser.Password }
        });
        
        if (!response.Ok)
            throw new Exception($"Login failed: {response.Status} {await response.TextAsync()}");
        
        var jsonBody = await response.JsonAsync();
        var token = jsonBody?.GetProperty("access_token").GetString();
        var refreshToken = jsonBody?.GetProperty("refresh_token").GetString();
        
        var page = await context.NewPageAsync(); 
        await page.GotoAsync(config.Env.LearnQaBaseUrl);
        await page.EvaluateAsync("""
                                 (data) => {
                                         localStorage.setItem('qa_platform_token', JSON.stringify(data.token));
                                         localStorage.setItem('qa_platform_refresh_token', JSON.stringify(data.refreshToken));
                                     }
                                 """, new { token, refreshToken });
        await context.StorageStateAsync(new BrowserContextStorageStateOptions 
        { 
            Path = config.LoginCookieStoragePathLearnQa 
        });
    }
}