using Microsoft.Playwright;

namespace Aqua.Framework.Components.Table;

public abstract class BaseTableRow(ILocator locator)
{
    public ILocator Locator { get; } = locator;
}