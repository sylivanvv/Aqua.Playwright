using Microsoft.Playwright;

namespace Aqua.Framework.Browser;

/// <summary>
/// Manages the lifecycle of a shared Playwright browser instance used across the test session.
/// <para>
/// Initialized once in <c>GlobalSetup</c> before any tests run, and disposed after all tests complete.
/// Each test creates its own isolated <see cref="IBrowserContext"/> via <see cref="CreateContextAsync"/> -
/// the browser process itself is shared for performance.
/// </para>
/// </summary>
public interface IPlaywrightBrowserManager
{      
    /// <summary>
    /// The <see cref="IPlaywright"/> driver instance created during <see cref="InitializeAsync"/>.
    /// Exposed so it can be registered as a separate singleton in the DI container
    /// for classes that depend on <see cref="IPlaywright"/> directly.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if accessed before <see cref="InitializeAsync"/> has been called.
    /// </exception>
    IPlaywright PlaywrightInstance { get; } 
    
    /// <summary>
    /// Launches the browser process based on the <c>PlaywrightConfig</c> settings
    /// (<c>BrowserName</c> and <c>Headless</c> flag).
    /// <para>
    /// Must be called once in <c>GlobalSetup</c> after the DI container is built
    /// and before any test creates a browser context.
    /// Supported browser names: <c>"chromium"</c> (default), <c>"firefox"</c>, <c>"webkit"</c>.
    /// </para>
    /// </summary>
    public Task InitializeAsync();
    
    /// <summary>
    /// Creates a new isolated <see cref="IBrowserContext"/> from the shared browser instance.
    /// Each test should get its own context to ensure full isolation of cookies,
    /// storage state, and network conditions between parallel tests.
    /// <para>
    /// Applies sensible defaults if not overridden by <paramref name="customOptions"/>:
    /// <list type="bullet">
    ///   <item><description><b>ViewportSize</b> — set to <c>NoViewport</c> (uses the OS window size).</description></item>
    ///   <item><description><b>AcceptDownloads</b> — enabled by default.</description></item>
    ///   <item><description><b>IgnoreHTTPSErrors</b> — enabled for test environments with self-signed certificates.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="customOptions">
    /// Optional context options. Only <c>null</c> fields are filled with defaults —
    /// any explicitly set values in <paramref name="customOptions"/> are preserved.
    /// Pass <see cref="BrowserNewContextOptions.StorageStatePath"/> to initialize
    /// the context with pre-generated auth cookies.
    /// </param>
    /// <returns>A new <see cref="IBrowserContext"/> ready for page creation.</returns>
    public Task<IBrowserContext> CreateContextAsync(BrowserNewContextOptions? customOptions = null);
    
    /// <summary>
    /// Closes the browser process and disposes the Playwright driver.
    /// </summary>
    public ValueTask DisposeAsync();
}