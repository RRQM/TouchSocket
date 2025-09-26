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
/// Tcp网络监听器
/// </summary>
public class TcpNetworkMonitor
{
    /// <summary>
    /// Tcp网络监听器
    /// </summary>
    /// <param name="option">监听配置选项</param>
    /// <param name="socket">Socket组件</param>
    /// <param name="e">Socket异步事件参数</param>
    /// <exception cref="ArgumentNullException">如果option或socket为<see langword="null"/>，则抛出此异常</exception>
    public TcpNetworkMonitor(TcpListenOption option, Socket socket, SocketAsyncEventArgs e)
    {
        this.Option = ThrowHelper.ThrowArgumentNullExceptionIf(option, nameof(option));
        this.Socket = ThrowHelper.ThrowArgumentNullExceptionIf(socket, nameof(socket));
        this.SocketAsyncEvent = e;
    }

    /// <summary>
    /// 监听配置
    /// </summary>
    public TcpListenOption Option { get; }

    /// <summary>
    /// Socket组件
    /// </summary>
    public Socket Socket { get; private set; }

    /// <summary>
    /// SocketAsyncEventArgs
    /// </summary>
    public SocketAsyncEventArgs SocketAsyncEvent { get; }
}