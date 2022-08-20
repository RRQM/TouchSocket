using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.Log;

namespace TouchSocket.Core.Log
{
    /// <summary>
    /// 日志基类
    /// </summary>
    public abstract class LoggerBase : ILog
    {
        /// <summary>
        /// 全部的日志类型
        /// </summary>
        public const LogType All = LogType.None | LogType.Trace | LogType.Debug | LogType.Information | LogType.Warning | LogType.Error | LogType.Critical;

        /// <summary>
        /// 日志基类
        /// </summary>
        protected LoggerBase()
        {
            this.LogType = All;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public LogType LogType { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Log(LogType logType, object source, string message, Exception exception)
        {
            if (this.LogType.HasFlag(logType))
            {
                WriteLog(logType, source, message, exception);
            }
        }

        /// <summary>
        /// 筛选日志后输出
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected abstract void WriteLog(LogType logType, object source, string message, Exception exception);
    }
}
