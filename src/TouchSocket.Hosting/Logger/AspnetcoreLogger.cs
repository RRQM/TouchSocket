using Microsoft.Extensions.Logging;
using System;
using TouchSocket.Core;
using LogLevel = TouchSocket.Core.LogLevel;

namespace TouchSocket.Hosting
{
    internal class AspnetcoreLogger : LoggerBase
    {
        private readonly ILoggerFactory m_loggerFactory;

        public AspnetcoreLogger(ILoggerFactory loggerFactory)
        {
            this.m_loggerFactory = loggerFactory;
        }

        private ILogger GetLogger(object source)
        {
            source ??= this;
            var type = source.GetType();
            return this.m_loggerFactory.CreateLogger(type);
        }


        protected override void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    this.GetLogger(source).LogTrace(exception,message);
                    break;
                case LogLevel.Debug:
                    this.GetLogger(source).LogDebug(exception, message);
                    break;
                case LogLevel.Info:
                    this.GetLogger(source).LogInformation(exception, message);
                    break;
                case LogLevel.Warning:
                    this.GetLogger(source).LogWarning(exception, message);
                    break;
                case LogLevel.Error:
                    this.GetLogger(source).LogError(exception, message);
                    break;
                case LogLevel.Critical:
                    this.GetLogger(source).LogCritical(exception, message);
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }
}
