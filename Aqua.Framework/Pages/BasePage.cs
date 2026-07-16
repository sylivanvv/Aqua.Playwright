using Aqua.Framework.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Aqua.Framework.Pages;

public abstract class BasePage(IPage page)
{
    protected IPage Page { get; } = page;
    protected ILogger Log => field ??= AquaServices.LoggerFactory.CreateLogger(GetType().Name);
    
    public abstract Task WaitForLoadedAsync();
}