using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Repository;

namespace CATWeb.Infrastructure.Logging
{
    public class Log4NetLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerRepository _loggerRepository;

        public Log4NetLoggerProvider(string log4netConfigFile)
        {
            _loggerRepository = LogManager.CreateRepository(
                Assembly.GetEntryAssembly(),
                typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(_loggerRepository, new FileInfo(log4netConfigFile));
        }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = LogManager.GetLogger(_loggerRepository.Name, categoryName);
            return new Log4NetLogger(logger);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }

}
