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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core.Run
{
    /// <summary>
    /// 易用组件
    /// </summary>
    public class EasyAction
    {
        private static readonly ConcurrentDictionary<object, Timer> timers = new ConcurrentDictionary<object, Timer>();

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTimeSpan"></param>
        public static void DelayRun(TimeSpan delayTimeSpan, Action action)
        {
            DelayRun(delayTimeSpan.Milliseconds, action);
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTimeSpan"></param>
        /// <param name="status"></param>
        public static void DelayRun<T>(TimeSpan delayTimeSpan, T status, Action<T> action)
        {
            DelayRun(delayTimeSpan.Milliseconds, status, action);
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public static void DelayRun(int delay, Action action)
        {
            object obj = new object();
            Timer timer = new Timer((o) =>
            {
                if (timers.TryRemove(o, out Timer timer1))
                {
                    timer1.Dispose();
                }
                action?.Invoke();
            }, obj, delay, Timeout.Infinite);
            timers.TryAdd(obj, timer);
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        /// <param name="status"></param>
        public static void DelayRun<T>(int delay, T status, Action<T> action)
        {
            object obj = new object();
            Timer timer = new Timer((o) =>
            {
                if (timers.TryRemove(o, out Timer timer1))
                {
                    timer1.Dispose();
                }
                action?.Invoke(status);
            }, obj, delay, Timeout.Infinite);
            timers.TryAdd(obj, timer);
        }

        /// <summary>
        /// Task异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statu"></param>
        /// <param name="action"></param>
        public static Task TaskRun<T>(T statu, Action<T> action)
        {
            return Task.Run(() =>
            {
                action.Invoke(statu);
            });
        }
    }
}