using System;

namespace Business
{
    public interface ICacheLoggerService
    {
        void LogInfo(string message, object details = null);
        void LogError(string message, Exception exception);
    }

    public class CacheLoggerService : ICacheLoggerService
    {
        private readonly ICache _cache;
        private const string LogCacheKey = "LoggerService_Logs";

        public CacheLoggerService(ICache cache)
        {
            _cache = cache;
        }

        public void Log(string message)
        {
            var logEntry = $"{DateTime.UtcNow:u}: {message}";
            _cache.PushToListAsync(LogCacheKey, logEntry);
        }

        public void LogInfo(string message, object details = null)
        {
            var logEntry = $"{DateTime.UtcNow:u} [INFO]: {message}";
            if (details != null)
            {
                logEntry += $" | Details: {details}";
            }
            _cache.PushToListAsync(LogCacheKey, logEntry);
        }

        public void LogError(string message, Exception exception)
        {
            var logEntry = $"{DateTime.UtcNow:u} [ERROR]: {message}";
            if (exception != null)
            {
                logEntry += $" | Exception: {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}";
            }
            _cache.PushToListAsync(LogCacheKey, logEntry);
        }
    }
}
