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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// Dmtp插件管理器扩展类
/// </summary>
public static class DmtpPluginManagerExtension
{
    /// <summary>
    /// 启用DmtpRpc心跳功能。该功能既可用于客户端，也可用于服务器端，但通常建议仅在客户端使用。
    /// <para>
    /// 心跳默认每3秒发送一次。当心跳失败次数达到最大值（默认为3次）时，将判定为连接断开。
    /// </para>
    /// </summary>
    /// <param name="pluginManager">插件管理器，用于管理包括心跳插件在内的各种插件。</param>
    /// <returns>返回新创建并已添加到插件管理器的DmtpHeartbeatPlugin实例。</returns>
    public static DmtpHeartbeatPlugin UseDmtpHeartbeat(this IPluginManager pluginManager)
    {
        var heartbeat = new DmtpHeartbeatPlugin();
        pluginManager.Add(heartbeat);
        return heartbeat;
    }
}