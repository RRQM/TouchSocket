//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace TouchSocket.Core.Log
{
    /// <summary>
    /// 控制台日志记录器
    /// </summary>
    public class ConsoleLogger : LoggerBase
    {
        /// <summary>
        /// 初始化一个日志记录器
        /// </summary>
        public ConsoleLogger()
        {
            this.m_consoleForegroundColor = Console.ForegroundColor;
            this.m_consoleBackgroundColor = Console.BackgroundColor;
        }

        private readonly ConsoleColor m_consoleForegroundColor;
        private readonly ConsoleColor m_consoleBackgroundColor;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected override void WriteLog(LogType logType, object source, string message, Exception exception)
        {
            lock (typeof(ConsoleLogger))
            {
                Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
                Console.Write(" | ");
                switch (logType)
                {
                    case LogType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case LogType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case LogType.Information:
                    default:
                        Console.ForegroundColor = this.m_consoleForegroundColor;
                        break;
                }
                Console.Write(logType.ToString());
                Console.ForegroundColor = this.m_consoleForegroundColor;
                Console.Write(" | ");
                Console.Write(message);

                if (exception != null)
                {
                    Console.Write(" | ");
                    Console.Write($"【异常消息】：{exception.Message}");
                    Console.Write($"【堆栈】：{(exception == null ? "未知" : exception.StackTrace)}");
                }
                Console.WriteLine();

                Console.ForegroundColor = this.m_consoleForegroundColor;
                Console.BackgroundColor = this.m_consoleBackgroundColor;
            }
        }
    }
}