using Aqua.Framework.Pages;
using Microsoft.Playwright;

namespace Aqua.Framework.Extensions;

public static class PageExtensions
{
    extension(IPage page)
    {
        public async Task<TPage> OpenPageAsync<TPage>(string url) where TPage : BasePage
        {
            await page.GotoAsync(url);
            return await page.CreateAndLoadAsync<TPage>();
        }

        public async Task<TPage> GoBackAsync<TPage>() where TPage : BasePage
        {
            await page.GoBackAsync();
            return await page.CreateAndLoadAsync<TPage>();
        }

        public async Task<TPage> ReloadAsync<TPage>() where TPage : BasePage
        {
            await page.ReloadAsync();
            return await page.CreateAndLoadAsync<TPage>();
        }
    }
}