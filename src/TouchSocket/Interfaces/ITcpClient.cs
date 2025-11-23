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
/// 定义了一个接口，该接口继承了多个与TCP客户端相关的接口，用于统一和扩展TCP客户端的功能。
/// </summary>
public interface ITcpClient : ITcpSession, ISetupConfigObject, ITcpConnectableClient, IClientSender, IReceiverClient<IReceiverResult>
{
    /// <summary>
    /// 连接事件处理程序，用于处理与 ITcpClient 接口相关的连接事件
    /// </summary>
    ConnectedEventHandler<ITcpClient> Connected { get; set; }

    /// <summary>
    /// 准备连接的时候
    /// </summary>
    ConnectingEventHandler<ITcpClient> Connecting { get; set; }

    /// <summary>
    /// 断开连接
    /// </summary>
    ClosedEventHandler<ITcpClient> Closed { get; set; }

    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// <para>
    /// </para>
    /// </summary>
    ClosingEventHandler<ITcpClient> Closing { get; set; }

    /// <summary>
    /// 接收到数据
    /// </summary>
    ReceivedEventHandler<ITcpClient> Received { get; set; }
}