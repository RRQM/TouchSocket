//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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