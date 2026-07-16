using System.Reflection;
using Allure.Net.Commons;
using Allure.NUnit;
using Aqua.Automation.AuthHelpers;
using Aqua.Framework.Browser;
using Aqua.Framework.Core;
using Aqua.Framework.Extensions;
using Aqua.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace Aqua.Automation;

/// <summary>
/// Abstract base class for all Playwright-based UI tests.
/// <para>
/// Manages the full per-test lifecycle: DI scope creation, browser context initialization,
/// authentication, error collection, artifact capture on failure, and resource cleanup.
/// </para>
/// <para>
/// Each test gets a fully isolated environment:
/// <list type="bullet">
///   <item><description>Its own DI scope via <see cref="AquaServices.CreateTestScope"/>.</description></item>
///   <item><description>Its own <see cref="IBrowserContext"/> and <see cref="IPage"/> instance.</description></item>
///   <item><description>Its own <see cref="BrowserErrors"/> collection populated during the test run.</description></item>
/// </list>
/// </para>
/// <para>
/// Authentication is opt-in via <see cref="WithAuthAttribute"/> on the test class.
/// When present, the browser context is initialized with a pre-generated storage state
/// (cookies + localStorage) for the specified role, bypassing the login UI.
/// </para>
/// </summary>
[TestFixture]
[SetCulture("en-US")]
[AllureNUnit]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class BasePlaywrightTest
{
    /// <summary>
    /// The Playwright page instance for the current test.
    /// Initialized in <see cref="InitBrowserAsync"/> and valid for the duration of the test.
    /// </summary>
    protected IPage Page { get; private set; } = null!;

    /// <summary>
    /// The browser context for the current test.
    /// Owns the page, cookies, and storage state.
    /// Disposed in <see cref="TryCloseBrowserAsync"/> during teardown.
    /// </summary>
    private IBrowserContext? _browserContext;

    /// <summary>
    /// Logger instance for the current test class, created lazily on first access.
    /// The log category is set to the concrete test class name for easy filtering.
    /// </summary>
    protected ILogger Log => field ??= AquaServices.LoggerFactory.CreateLogger(GetType().Name);

    /// <summary>
    /// Accumulates browser-side errors detected during the test, including:
    /// <list type="bullet">
    ///   <item><description>Uncaught JS exceptions via <c>Page.PageError</c>.</description></item>
    ///   <item><description>Console error messages via <c>Page.Console</c> (type = "error").</description></item>
    ///   <item><description>Network responses with status >= 400 via <c>Page.Response</c>.</description></item>
    /// </list>
    /// Can be asserted in tests or checked in TearDown to fail tests that produced unexpected errors.
    /// </summary>
    protected List<string> BrowserErrors { get; } = [];

    /// <summary>
    /// Creates and registers a new DI scope for the current test via <see cref="AquaServices"/>.
    /// Must run before <see cref="InitBrowserAsync"/> since browser initialization depends on scoped services.
    /// </summary>
    [SetUp]
    public virtual void InitScope()
    {
        AquaServices.CreateTestScope(GlobalSetup.RootProvider);
        Log.Info($"Test {TestContext.CurrentContext.Test.MethodName} started");
    }

    /// <summary>
    /// Creates and registers a new DI scope for the current test via <see cref="AquaServices"/>.
    /// Initializes the browser context and page for the current test.
    /// <para>
    /// If the test class is decorated with <see cref="WithAuthAttribute"/>,
    /// the context is created with a pre-saved storage state for the specified role,
    /// so the test starts already authenticated without going through the login flow.
    /// </para>
    /// <para>
    /// Also configures:
    /// <list type="bullet">
    ///   <item><description>Playwright tracing with screenshots, DOM snapshots, and sources.</description></item>
    ///   <item><description><see cref="BrowserErrors"/> population via Page event subscriptions.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    [SetUp]
    public virtual async Task InitBrowserAsync()
    {
        var browserManager = AquaServices.Provider.GetRequiredService<IPlaywrightBrowserManager>();
        var options = new BrowserNewContextOptions();
        var authAttribute = GetType().GetCustomAttribute<WithAuthAttribute>(true);
        if (authAttribute is not null)
        {
            var storagePath = authAttribute.Role switch
            {
                AuthRole.LearnQaUser => AquaServices.Config.LoginCookieStoragePathLearnQa,
                AuthRole.QaBrainsUser => AquaServices.Config.LoginCookieStoragePathQaBrains,
                _ => throw new ArgumentOutOfRangeException($"There is no such role: {authAttribute.Role}")
            };
            if (!File.Exists(storagePath))
            {
                throw new FileNotFoundException(
                    $"Auth state file not found for role '{authAttribute.Role}' at '{storagePath}'. " +
                    "Check the setup logs — auth generation likely failed.");
            }
            options.StorageStatePath = storagePath;
        }

        _browserContext = await browserManager.CreateContextAsync(options);
        Page = await _browserContext.NewPageAsync();

        Page.SetDefaultTimeout(AquaServices.Config.PlaywrightConfig.ElementWaitMs);
        Page.SetDefaultNavigationTimeout(AquaServices.Config.PlaywrightConfig.PageLoadMs);

        await Page.Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        // Capture uncaught JS exceptions thrown on the page
        Page.PageError += (_, exception) => BrowserErrors.Add(exception);
        // Capture console.error() calls from page scripts
        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error") BrowserErrors.Add(msg.Text);
        };
        // Capture failed network responses (4xx client errors, 5xx server errors)
        Page.Response += (_, response) =>
        {
            if (response.Status >= 400)
            {
                BrowserErrors.Add($"Network Error: {response.Status} [{response.Request.Method}] {response.Url}");
            }
        };
    }

    /// <summary>
    /// Runs after each test to capture artifacts and release all resources.
    /// Executes unconditionally regardless of test outcome.
    /// <para>
    /// Teardown order:
    /// <list type="number">
    ///   <item><description>Screenshot captured and attached to Allure report (failure only).</description></item>
    ///   <item><description>Playwright trace saved and attached to Allure report (failure only).</description></item>
    ///   <item><description>Browser context disposed.</description></item>
    ///   <item><description>DI scope disposed via <see cref="AquaServices.ClearScopeAsync"/> (always, in finally).</description></item>
    /// </list>
    /// </para>
    /// </summary>
    [TearDown]
    public virtual async Task TearDown()
    {
        var isTestFailed = TestContext.CurrentContext.Result.Outcome != ResultState.Success;
        try
        {
            await TryTakeScreenshotOnFailAsync(isTestFailed);
            await TrySavePlaywrightTraceOnFailAsync(isTestFailed);
            await TryCloseBrowserAsync();
        }
        finally
        {
            // Always dispose the DI scope even if artifact capture or browser close throws
            await AquaServices.ClearScopeAsync();
        }
    }

    private async Task TryCloseBrowserAsync()
    {
        try
        {
            if (_browserContext != null) await _browserContext.DisposeAsync();
        }
        catch (Exception e)
        {
            Log.Error("Browser quit failed with error: " + e);
        }
    }

    private async Task TryTakeScreenshotOnFailAsync(bool isTestFailed)
    {
        if (!isTestFailed) return;
        try
        {
            var validatedTestName = TestContext.CurrentContext.Test.Name;
            var invalidChars = Path.GetInvalidFileNameChars();
            validatedTestName = string.Join("_", validatedTestName.Split(invalidChars));
            var fileName = $"{validatedTestName}_{DateTime.Now:dd-MM-yyyy--HH-mm-ss}.png";
            var path = Path.Combine(PathProvider.ScreenshotsPath, fileName);
            await Page.ScreenshotAsync(new PageScreenshotOptions
                { Path = path });

            AllureApi.AddAttachment("Failed test screenshot", "image/png", path);
        }
        catch (Exception e)
        {
            Log.Error($"Screenshot failed with error: {e}");
        }
    }

    /// <summary>
    /// Stops the Playwright trace and, on failure, saves it as a zip archive attached to the Allure report.
    /// On success, stops tracing without saving to avoid accumulating large trace files.
    /// Suppresses exceptions to prevent trace handling from masking the original test failure.
    /// </summary>
    private async Task TrySavePlaywrightTraceOnFailAsync(bool isTestFailed)
    {
        try
        {
            if (isTestFailed)
            {
                var tracePath = Path.Combine(PathProvider.ArtifactsPath,
                    $"trace_{TestContext.CurrentContext.Test.MethodName}_{DateTime.Now:dd-MM-yyyy--HH-mm}.zip");
                await Page.Context.Tracing.StopAsync(new TracingStopOptions
                {
                    Path = tracePath
                });

                AllureApi.AddAttachment("Playwright Trace", "application/zip", tracePath);
            }
            else
            {
                await Page.Context.Tracing.StopAsync();
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to handle Playwright Trace: {e}");
        }
    }
}