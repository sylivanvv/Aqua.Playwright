using Microsoft.Playwright;

namespace Aqua.Framework.Pages;

public static class PageFactory
{
    extension(IPage page)
    {
        public async Task<TPage> CreateAndLoadAsync<TPage>()
            where TPage : BasePage
        {
            var pageObject = (TPage)Activator.CreateInstance(typeof(TPage), page)!;
            await pageObject.WaitForLoadedAsync();
            return pageObject;
        }

        public async Task<TPage> OpenInNewTabAsync<TPage>(Func<Task> triggerAction)
            where TPage : BasePage
        {
            var newPage = await page.Context.RunAndWaitForPageAsync(triggerAction);
            return await newPage.CreateAndLoadAsync<TPage>();
        }
    }
}