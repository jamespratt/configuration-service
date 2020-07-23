using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConfigurationService.Client
{
    public static class Logger
    {
        private static ILoggerFactory _factory;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = new NullLoggerFactory();
                }
                return _factory;
            }
            set => _factory = value;
        }

        public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}