using Microsoft.Extensions.Logging;
using YukoBot.Models.Log.Loggers;

namespace YukoBot.Models.Log.Providers
{
    internal class DiscordClientLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel _minimumLevel;
        private FileLogger _logger;

        public DiscordClientLoggerProvider(LogLevel minimumLevel = LogLevel.Information)
        {
            _minimumLevel = minimumLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (_logger == null)
            {
                _logger = new FileLogger(LogWriter.GetOrCreate(), _minimumLevel);
            }
            return _logger;
        }

        public void Dispose() => _logger?.Dispose();
    }
}