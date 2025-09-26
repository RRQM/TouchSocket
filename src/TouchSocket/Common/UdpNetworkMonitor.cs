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

using System.Net.Sockets;

namespace TouchSocket.Sockets;

/// <summary>
/// Udp监听器
/// </summary>
public class UdpNetworkMonitor
{
    /// <summary>
    /// 初始化Udp监听器
    /// </summary>
    /// <param name="iPHost">IP主机信息</param>
    /// <param name="socket">Socket对象，用于网络通信</param>
    /// <exception cref="ArgumentNullException">如果socket为<see langword="null"/>，则抛出此异常</exception>
    public UdpNetworkMonitor(IPHost iPHost, Socket socket)
    {
        this.IPHost = iPHost;
        // 检查socket参数是否为空，为空则抛出ArgumentNullException
        this.Socket = ThrowHelper.ThrowArgumentNullExceptionIf(socket, nameof(socket));
    }

    /// <summary>
    /// 获取IP主机信息
    /// </summary>
    public IPHost IPHost { get; }

    /// <summary>
    /// 获取或设置Socket组件
    /// </summary>
    public Socket Socket { get; private set; }
}