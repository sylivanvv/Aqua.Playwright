using System.Text;
using Allure.Net.Commons;
using Microsoft.Extensions.Logging;

namespace Aqua.Automation.Reporting;

public class AllureLogger(string categoryName) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || logLevel < LogLevel.Information) return;
        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message)) return;
        var lifecycle = AllureLifecycle.Instance;
        var shortCategory = categoryName.Split('.').LastOrDefault() ?? categoryName;
        var stepStarted = false;
        
        try
        {
            lifecycle.StartStep(new StepResult
            {
                name = $"[{shortCategory}] {message}",
                status = logLevel switch
                {
                    LogLevel.Error or LogLevel.Critical => Status.failed,
                    LogLevel.Warning => Status.broken,
                    _ => Status.passed
                }
            });
            stepStarted = true;

            if (exception != null)
            {
                AllureApi.AddAttachment(
                    "Exception",
                    "text/plain",
                    Encoding.UTF8.GetBytes(exception.ToString()));
            }
        }
        catch (InvalidOperationException)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = logLevel >= LogLevel.Error ? ConsoleColor.Red : ConsoleColor.Gray;
            Console.WriteLine($"[{logLevel}] [{shortCategory}] {message}");
            if (exception != null) Console.WriteLine(exception);
            Console.ForegroundColor = originalColor;
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            if (stepStarted)
            {
                try
                {
                    lifecycle.StopStep();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}