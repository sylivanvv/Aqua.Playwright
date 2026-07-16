using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class IFrameOnIFramesAndWindowsPage(IFrameLocator frameLocator)
    {
        public  ILocator IFrameButton => frameLocator.Locator("#iframe-button");
        
        public async Task ClickIFrameButtonAndReturn()
        {
            await IFrameButton.ClickAsync();
        }
}