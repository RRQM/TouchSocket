//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMCore.Run
{
    /// <summary>
    /// 时间执行
    /// </summary>
    public class TimeRun
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeSpan"></param>
        public static void Run(Action action, TimeSpan timeSpan)
        {
            Task.Run(() =>
            {
                WaitHandle waitHandle = new AutoResetEvent(false);
                waitHandle.WaitOne(timeSpan);
                action?.Invoke();
            });
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="seconds"></param>
        public static void Run(Action action, double seconds)
        {
            Task.Run(() =>
            {
                WaitHandle waitHandle = new AutoResetEvent(false);
                waitHandle.WaitOne(TimeSpan.FromSeconds(seconds));
                action?.Invoke();
            });
        }
    }
}