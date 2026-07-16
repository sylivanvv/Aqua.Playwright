using Aqua.AppConfig.Configuration;
using Aqua.Framework.Browser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Aqua.Framework.Core;

/// <summary>
/// Builds and configures the root DI container for the test session.
/// <para>
/// Call <see cref="BuildRootProvider"/> once in <c>SetupFixture</c> before any tests run.
/// The returned <see cref="IServiceProvider"/> is used to create per-test scopes via
/// <see cref="AquaServices.CreateTestScope"/>.
/// </para>
/// </summary>
public static class Startup
{
    /// <summary>
    /// Creates and returns a new root <see cref="IServiceProvider"/> with all
    /// framework-level services registered.
    /// <para>Registered services:</para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Logging</b> — NLog provider with Trace-level minimum, replaces default providers.
    ///   </description></item>
    ///   <item><description>
    ///     <b><see cref="IConfig"/></b> — singleton with the loaded test environment configuration.
    ///   </description></item>
    ///   <item><description>
    ///     <b><see cref="IPlaywrightBrowserManager"/></b> — singleton managing the shared browser instance.
    ///   </description></item>
    /// </list>
    /// <para>
    /// Pass <paramref name="additionalConfig"/> to register project-specific services
    /// (API clients, page objects, test data helpers) without modifying this class.
    /// </para>
    /// <para>
    /// Built with <see cref="ServiceProviderOptions.ValidateScopes"/> and
    /// <see cref="ServiceProviderOptions.ValidateOnBuild"/> enabled — misconfigured
    /// lifetimes and missing registrations are caught immediately at startup,
    /// not mid-run during tests.
    /// </para>
    /// </summary>
    /// <param name="config">
    /// Loaded environment configuration. Registered as a singleton so the same
    /// instance is available throughout the session.
    /// </param>
    /// <param name="additionalConfig">
    /// Optional callback for registering project-specific services.
    /// Invoked after all framework services are registered.
    /// </param>
    /// <returns>
    /// A fully configured root <see cref="IServiceProvider"/>.
    /// Must be called exactly once — call site is responsible for storing
    /// and reusing the returned instance.
    /// </returns>
    public static IServiceProvider BuildRootProvider(IConfig config,
        Action<IServiceCollection>? additionalConfig = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.AddNLog();
        });
        services.AddSingleton(config);
        services.AddSingleton<IPlaywrightBrowserManager, PlaywrightBrowserManager>();

        additionalConfig?.Invoke(services);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }
}