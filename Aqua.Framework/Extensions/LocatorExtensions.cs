using Aqua.Framework.Pages;
using Microsoft.Playwright;

namespace Aqua.Framework.Extensions;

public static class LocatorExtensions
{
        extension(ILocator locator)
        {
            public async Task<TPage> OpenPageAsync<TPage>()
                where TPage : BasePage
            {
                await locator.ClickAsync();
                return await locator.Page.CreateAndLoadAsync<TPage>();
            }

            public async Task<bool> IsElementVisibleAfterWaitAsync(int timeoutMs = 2_000)
            {
                try
                {
                    await locator.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
                    return true;
                }
                catch (TimeoutException)
                {
                    return false;
                }
            }
        }
}