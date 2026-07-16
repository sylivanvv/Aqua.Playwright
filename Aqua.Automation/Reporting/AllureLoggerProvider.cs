using Microsoft.Extensions.Logging;

namespace Aqua.Automation.Reporting;

[ProviderAlias("Allure")]
internal sealed class AllureLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) =>  new AllureLogger(categoryName);

    public void Dispose() { }
}