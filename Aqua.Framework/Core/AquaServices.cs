using Aqua.AppConfig.Configuration;
using Aqua.Framework.Browser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aqua.Framework.Core;

/// <summary>
/// Static service locator for resolving test-scoped dependencies during test execution.
/// <para>
/// Each test gets its own isolated <see cref="AsyncServiceScope"/> stored in <see cref="AsyncLocal{T}"/>
/// which ensures that parallel tests running on different async contexts never share the same scope.
/// </para>
/// <para>
/// Typical test lifecycle:
/// <list type="number">
///   <item><description><see cref="CreateTestScope"/> — called in [SetUp] to create a scope for the current test.</description></item>
///   <item><description><see cref="Get{T}"/> — called anywhere in the test to resolve scoped services.</description></item>
///   <item><description><see cref="ClearScopeAsync"/> — called in [TearDown] to dispose the scope and release all resources.</description></item>
/// </list>
/// </para>
/// </summary>
public static class AquaServices
{
    /// <summary>
    /// Holds the current test's <see cref="IServiceScope"/> in async-local storage.
    /// Each async execution context (i.e. each parallel test) gets its own independent value,
    /// preventing scope leakage between concurrently running tests.
    /// </summary>
    private static readonly AsyncLocal<IServiceScope?> CurrentScope = new();

    /// <summary>
    /// Returns the <see cref="IServiceProvider"/> for the current test scope.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if accessed before <see cref="CreateTestScope"/> is called,
    /// or after <see cref="ClearScopeAsync"/> has disposed the scope.
    /// </exception>
    public static IServiceProvider Provider =>
        CurrentScope.Value?.ServiceProvider
        ?? throw new InvalidOperationException("Aqua services called out of test scope");

    /// <summary>
    /// Creates and stores a new async service scope for the current execution context.
    /// The caller is responsible for disposing it by calling <see cref="ClearScopeAsync"/>.
    /// </summary>
    /// <param name="rootProvider">
    /// The root <see cref="IServiceProvider"/> built during <c>GlobalSetup</c>.
    /// Scoped services are resolved within a child scope created from this provider.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rootProvider"/> is null.</exception>
    public static void CreateTestScope(IServiceProvider rootProvider)
    {
        ArgumentNullException.ThrowIfNull(rootProvider);
        if (CurrentScope.Value is not null)
            throw new InvalidOperationException("Call ClearScopeAsync() before creating a new scope.");
        CurrentScope.Value = rootProvider.CreateAsyncScope();
    }

    /// <summary>
    /// Resolves a required service of type <typeparamref name="T"/> from the current test scope.
    /// Throws if the service is not registered or the scope is not initialized.
    /// </summary>
    /// <typeparam name="T">The service type to resolve. Must be non-null.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the scope is not initialized or the service is not registered.
    /// </exception>
    public static T Get<T>() where T : notnull => Provider.GetRequiredService<T>();

    /// <summary>
    /// Resolves the <see cref="ILoggerFactory"/> from the current test scope.
    /// Used to create named loggers for test classes and framework components.
    /// </summary>
    public static ILoggerFactory LoggerFactory => Get<ILoggerFactory>();

    /// <summary>
    /// Resolves a strongly-typed <see cref="ILogger{T}"/> for the given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type that owns the logger, used as the log category name.</typeparam>
    public static ILogger<T> GetLogger<T>() => Get<ILogger<T>>();

    /// <summary>
    /// Resolves the test environment configuration from the current test scope.
    /// Provides access to URLs, credentials, timeouts, and other settings loaded at startup.
    /// </summary>
    public static IConfig Config => Get<IConfig>();

    /// <summary>
    /// Resolves the Playwright browser manager from the current test scope.
    /// Used to create browser contexts and pages for UI test execution.
    /// </summary>
    public static IPlaywrightBrowserManager PlaywrightBrowserManager => Get<IPlaywrightBrowserManager>();

    /// <summary>
    /// Disposes the current test scope and clears it from async-local storage.
    /// Must be called in [TearDown] to release all scoped resources (API clients, HTTP connections, etc.)
    /// after each test completes, regardless of whether the test passed or failed.
    /// <para>
    /// Clears the scope reference in a <c>finally</c> block to guarantee cleanup
    /// even if <see cref="AsyncServiceScope.DisposeAsync"/> throws.
    /// </para>
    /// </summary>
    public static async Task ClearScopeAsync()
    {
        var scope = CurrentScope.Value;
        if (scope == null) return;
        try
        {
            if (scope is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
        }
        finally
        {
            CurrentScope.Value = null;
        }
    }
}