using Microsoft.Playwright;

namespace Aqua.Automation.AuthHelpers;

public interface IAuthStrategy
{
    Task GenerateStateAsync(IPlaywright playwright, IBrowserContext context);
}