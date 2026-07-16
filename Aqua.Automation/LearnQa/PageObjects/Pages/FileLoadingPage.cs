using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.PageObjects.Pages;

public class FileLoadingPage(IPage page) : BaseLearnQaPage(page)
{
    public override async Task WaitForLoadedAsync() => await DownloadFileButton.WaitForAsync();
        
    private ILocator DownloadFileButton => Page.Locator("#download-template");
    private ILocator UploadFileButton => Page.Locator("//input[@type = 'file']");
    private ILocator SuccessfulDownload => Page.Locator("//div[contains(@class, 'status-completed')]");
    public ILocator DownloadProcessed => Page.Locator("#download-processed");

    public async Task<IDownload> DownloadFileAsync() => 
        await Page.RunAndWaitForDownloadAsync(async () => await DownloadFileButton.ClickAsync());

    public async Task<IDownload> DownloadProcessedFileAsync() => 
        await Page.RunAndWaitForDownloadAsync(async () => await DownloadProcessed.ClickAsync());

    public async Task<FileLoadingPage> UploadFileAsync(string pathToFile)
    {
        await UploadFileButton.SetInputFilesAsync(pathToFile);
        await SuccessfulDownload.WaitForAsync(new LocatorWaitForOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 30000
        });
        return this;
    }

    public ILocator IsFileUploaded(string fileName) => SuccessfulDownload.Locator($"//span[. = '{fileName}']");

}