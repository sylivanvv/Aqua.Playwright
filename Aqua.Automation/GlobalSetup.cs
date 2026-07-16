using Aqua.AppConfig.Configuration;
using Aqua.Automation.AuthHelpers;
using Aqua.Automation.LearnQa.CapybaraApi;
using Aqua.Automation.Reporting;
using Aqua.Framework.Browser;
using Aqua.Framework.Core;
using Aqua.TestRailIntegration.TestRailIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

//Controls the maximum number of tests running concurrently across the session.
[assembly: LevelOfParallelism(8)]

namespace Aqua.Automation;

/// <summary>
/// One-time setup and teardown for the entire test session.
/// Runs once before the first test and once after the last test.
/// <para>
/// Responsibilities:
/// <list type="number">
///   <item><description>Build the root DI container with all framework and project-level services.</description></item>
///   <item><description>Generate and cache browser auth state (cookies) for all configured roles.</description></item>
///   <item><description>Initialize the shared Playwright browser instance used across all tests.</description></item>
///   <item><description>Dispose the browser after all tests complete.</description></item>
/// </list>
/// </para>
/// </summary>
[SetUpFixture]
public class GlobalSetup
{
    /// <summary>
    /// Root dependency injection container for the test session.
    /// Built once before any tests run and used to create isolated scopes for individual tests.
    /// </summary>
    internal static IServiceProvider RootProvider { get; private set; }

    /// <summary>
    /// Initializes all shared infrastructure before any test runs.
    /// <para>
    /// Execution order inside this method is intentional and must not be changed:
    /// <list type="number">
    ///   <item><description>
    ///     <b>Build DI container</b> - registers logging, config, browser manager,
    ///     API clients, and any project-specific services via the <c>additionalConfig</c> callback.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Initialize browser</b> - launches the shared <see cref="IBrowser"/> instance.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Generate auth state</b> - authentication strategies use the initialized
    ///     browser (or Playwright API) to create and persist storage state for each role.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        var config = ConfigLoader.Load();
        // Register services required for automation tests here
        RootProvider = Startup.BuildRootProvider(config, services =>
        {
            // Forward Allure log entries to the Allure report alongside test steps
            services.AddLogging(builder => { builder.AddProvider(new AllureLoggerProvider()); });
            services.AddScoped<TestRailApiClient>();
            // Scoped API client - each test gets its own instance tied to its DI scope
            services.AddScoped<CapybaraApiClient>();
            services.AddScoped<AuthStateGenerator>();
            services.AddScoped<IAuthStrategy, LearnQaApiAuthStrategy>();
            services.AddScoped<IAuthStrategy, QaBrainsUiAuthStrategy>();
        });

        // Configure the default timeout used by Playwright Expect assertions.
        Assertions.SetDefaultExpectTimeout(config.PlaywrightConfig.ElementWaitMs);

        // Launch the shared browser process.
        await RootProvider.GetRequiredService<IPlaywrightBrowserManager>().InitializeAsync();

        // Generate and persist auth state for each configured role.
        // The generated storage state files are reused by every test,
        // avoiding repeated login flows and reducing test execution time.
        // LearnQa: logs in via API and saves the session cookie.
        // QaBrains: logs in via UI (Playwright) and saves the full storage state.
        try
        {
            AquaServices.CreateTestScope(RootProvider);
            var authStateGenerator = AquaServices.Get<AuthStateGenerator>();
            await authStateGenerator.GenerateAllAsync();
        }
        finally
        {
            await AquaServices.ClearScopeAsync();
        }
    }

    /// <summary>
    /// Releases all shared browser resources after the last test in the session completes.
    /// Closes the browser process and disposes the Playwright instance.
    /// </summary>
    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        if (RootProvider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
    }
}