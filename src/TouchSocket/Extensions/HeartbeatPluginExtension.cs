//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

namespace TouchSocket.Sockets;


/// <summary>
/// 心跳插件扩展类
/// </summary>
public static class HeartbeatPluginExtension
{
    /// <summary>
    /// 设置心跳间隔。默认3秒。
    /// </summary>
    /// <typeparam name="THeartbeatPlugin">心跳插件类型，必须是HeartbeatPlugin的派生类型。</typeparam>
    /// <param name="heartbeatPlugin">将要设置心跳间隔的心跳插件实例。</param>
    /// <param name="timeSpan">心跳间隔时间，包括小时、分钟和秒等。</param>
    /// <returns>返回设置后的心跳插件实例，支持链式调用。</returns>
    public static THeartbeatPlugin SetTick<THeartbeatPlugin>(this THeartbeatPlugin heartbeatPlugin, TimeSpan timeSpan)
        where THeartbeatPlugin : HeartbeatPlugin
    {
        // 设置心跳插件的Tick属性为指定的时间间隔
        heartbeatPlugin.Tick = timeSpan;
        // 返回设置后的心跳插件实例
        return heartbeatPlugin;
    }

    /// <summary>
    /// 设置最大失败次数，默认3。
    /// </summary>
    /// <typeparam name="THeartbeatPlugin">心跳插件类型</typeparam>
    /// <param name="heartbeatPlugin">具体的心跳插件实例</param>
    /// <param name="value">设置的最大失败次数</param>
    /// <returns>返回设置后的心跳插件实例</returns>
    public static THeartbeatPlugin SetMaxFailCount<THeartbeatPlugin>(this THeartbeatPlugin heartbeatPlugin, int value)
         where THeartbeatPlugin : HeartbeatPlugin
    {
        // 设置插件的最大失败次数
        heartbeatPlugin.MaxFailCount = value;
        // 返回设置后的插件实例
        return heartbeatPlugin;
    }
}