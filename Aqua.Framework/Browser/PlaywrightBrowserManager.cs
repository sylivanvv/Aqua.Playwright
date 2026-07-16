using Aqua.AppConfig.Configuration;
using Aqua.AppConfig.ConfigurationModels;
using Microsoft.Playwright;

namespace Aqua.Framework.Browser;

public class PlaywrightBrowserManager(IConfig config) : IPlaywrightBrowserManager, IAsyncDisposable
{
    private PlaywrightConfig Config => config.PlaywrightConfig;
    public IPlaywright PlaywrightInstance => _playwright ?? throw new InvalidOperationException("Playwright is not initialized yet!");
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = Config.BrowserName.ToLower() switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = Config.Headless }),
            "webkit"  => await _playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = Config.Headless }),
            _ => await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = Config.Headless,
                Args = [
                        "--start-maximized",
                        "--disable-features=Translate",
                        "--lang=en-US"
                        ]
            })
        };
    }

    public async Task<IBrowserContext> CreateContextAsync(BrowserNewContextOptions? customOptions = null)
    {
        var options = customOptions ?? new BrowserNewContextOptions();
        options.ViewportSize ??= ViewportSize.NoViewport; 
        options.AcceptDownloads ??= true;
        options.IgnoreHTTPSErrors ??= true; 
        return await _browser!.NewContextAsync(options);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
        _playwright?.Dispose();
        _playwright = null;
        GC.SuppressFinalize(this);
    }
}