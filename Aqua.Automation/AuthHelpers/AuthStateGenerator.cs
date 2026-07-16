using Aqua.Framework.Browser;
using Microsoft.Extensions.Logging;

namespace Aqua.Automation.AuthHelpers;

public class AuthStateGenerator(
    IEnumerable<IAuthStrategy> strategies,
    IPlaywrightBrowserManager browserManager,
    ILogger<AuthStateGenerator> log)
{
    public async Task GenerateAllAsync()
    {
        var strategyList = strategies.ToList();
        if (strategyList.Count == 0) return;
        var playwright = browserManager.PlaywrightInstance;
        var tasks = strategyList.Select(async strategy =>
        {
            var strategyName = strategy.GetType().Name;
            try
            {
                await using var context = await browserManager.CreateContextAsync();
                await strategy.GenerateStateAsync(playwright, context);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Auth generation FAILED for {Strategy}", strategyName);
            }
        });
        await Task.WhenAll(tasks);
    }
}