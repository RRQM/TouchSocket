using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore.Log
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public static class LoggerExtensions
    {
        #region 日志

        /// <summary>
        /// 输出消息日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Message(this ILog logger, string msg)
        {
            logger.Debug(LogType.Message, null, msg);
        }

        /// <summary>
        /// 输出警示日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Warning(this ILog logger, string msg)
        {
            logger.Debug(LogType.Warning, null, msg);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Error(this ILog logger, string msg)
        {
            logger.Debug(LogType.Error, null, msg);
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        public static void Exception(this ILog logger, Exception ex)
        {
            logger.Debug(LogType.Error, null, ex.Message, ex);
        }

        /// <summary>
        /// 输出消息日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        public static void Message(this ILog logger, object source, string msg)
        {
            logger.Debug(LogType.Message, source, msg);
        }

        /// <summary>
        /// 输出警示日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        public static void Warning(this ILog logger, object source, string msg)
        {
            logger.Debug(LogType.Warning, source, msg);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        public static void Error(this ILog logger, object source, string msg)
        {
            logger.Debug(LogType.Error, source, msg);
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="ex"></param>
        public static void Exception(this ILog logger, object source, Exception ex)
        {
            logger.Debug(LogType.Error, source, ex.Message, ex);
        }

        #endregion 日志
    }
}
