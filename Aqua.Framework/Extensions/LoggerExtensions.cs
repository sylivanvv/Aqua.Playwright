using Microsoft.Extensions.Logging;

namespace Aqua.Framework.Extensions;

public static class LoggerExtensions
{
    extension(ILogger logger)
    {
        public void Info(string message, params object[] args)
            => logger.LogInformation(message, args);

        public void Warn(string message, params object[] args)
            => logger.LogWarning(message, args);

        public void Error(string message, params object[] args)
            => logger.LogError(message, args);

        public void Debug(string message, params object[] args)
            => logger.LogDebug(message, args);

        public void Fatal(string message, params object[] args)
            => logger.LogCritical(message, args);

        public void Warn(Exception ex, string message, params object[] args)
            => logger.LogWarning(ex, message, args);

        public void Error(Exception ex, string message, params object[] args)
            => logger.LogError(ex, message, args);

        public void Fatal(Exception ex, string message, params object[] args)
            => logger.LogCritical(ex, message, args);
    }
}