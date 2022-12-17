using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 一个空的日志。
    /// </summary>
    public sealed class EmptyLogger : LoggerBase
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected override void WriteLog(LogType logType, object source, string message, Exception exception)
        {
        }
    }
}