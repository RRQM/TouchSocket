//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace RRQMCore.Log
{
    /// <summary>
    /// 一组日志记录器
    /// </summary>
    public class LoggerGroup : ILog
    {
        private readonly ILog[] logs;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logs"></param>
        public LoggerGroup(params ILog[] logs)
        {
            this.logs = logs ?? throw new ArgumentNullException(nameof(logs));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Debug(LogType logType, object source, string message, Exception exception)
        {
            foreach (var log in this.logs)
            {
                try
                {
                    log.Debug(logType, source, message, exception);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Debug(LogType logType, object source, string message)
        {
            this.Debug(logType, source, message, null);
        }

        /// <summary>
        /// 使用指定类型的记录器输出
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Debug<T>(LogType logType, object source, string message, Exception exception) where T : ILog
        {
            foreach (var log in this.logs)
            {
                if (log.GetType() == typeof(T))
                {
                    try
                    {
                        log.Debug(logType, source, message, exception);
                    }
                    catch
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 使用指定类型的记录器输出
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Debug<T>(LogType logType, object source, string message) where T : ILog
        {
            this.Debug(logType, source, message, null);
        }
    }
}
