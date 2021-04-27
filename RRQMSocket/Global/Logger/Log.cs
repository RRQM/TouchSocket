//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Log;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    public class Log : ILog
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        public void Debug(LogType logType, object source, string message, string stackTrace)
        {
            Console.WriteLine($"错误：类型：{logType}，消息：{message}，堆：{stackTrace}");
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Debug(LogType logType, object source, string message)
        {
            Console.WriteLine($"错误：类型：{logType}，消息：{message}");
        }
    }
}