using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// HeartbeatPlugin
    /// </summary>
    public abstract class HeartbeatPlugin : PluginBase
    {
        /// <summary>
        /// 最大失败次数，默认3。
        /// </summary>
        public int MaxFailCount { get; set; } = 3;

        /// <summary>
        /// 心跳间隔。默认3秒。
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(3);
    }

    /// <summary>
    /// HeartbeatPluginExtension
    /// </summary>
    public static class HeartbeatPluginExtension
    {
        /// <summary>
        /// 设置心跳间隔。默认3秒。
        /// </summary>
        /// <typeparam name="THeartbeatPlugin"></typeparam>
        /// <param name="heartbeatPlugin"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static THeartbeatPlugin SetTick<THeartbeatPlugin>(this THeartbeatPlugin heartbeatPlugin, TimeSpan timeSpan)
            where THeartbeatPlugin : HeartbeatPlugin
        {
            heartbeatPlugin.Tick = timeSpan;
            return heartbeatPlugin;
        }

        /// <summary>
        /// 设置最大失败次数，默认3。
        /// </summary>
        /// <typeparam name="THeartbeatPlugin"></typeparam>
        /// <param name="heartbeatPlugin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static THeartbeatPlugin SetMaxFailCount<THeartbeatPlugin>(this THeartbeatPlugin heartbeatPlugin, int value)
             where THeartbeatPlugin : HeartbeatPlugin
        {
            heartbeatPlugin.MaxFailCount = value;
            return heartbeatPlugin;
        }
    }
}