using System;
using System.Threading.Tasks;

namespace RRQMCore.Run
{
    /// <summary>
    /// 易用组件
    /// </summary>
    public class EasyAction
    {
        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeSpan"></param>
        public static void DelayRun(TimeSpan timeSpan, Action action)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeSpan);
                action?.Invoke();
            });
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="seconds"></param>
        public static void DelayRun(double seconds, Action action)
        {
            DelayRun(TimeSpan.FromSeconds(seconds), action);
        }
    }
}