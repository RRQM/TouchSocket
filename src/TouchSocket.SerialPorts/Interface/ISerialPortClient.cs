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

using System.IO.Ports;
using TouchSocket.Sockets;

namespace TouchSocket.SerialPorts;

/// <summary>
/// 串口客户端接口。
/// </summary>
public interface ISerialPortClient : ISerialPortSession, IConnectableClient, IClientSender, IReceiverClient<IReceiverResult>
{
    /// <summary>
    /// 成功打开串口
    /// </summary>
    ConnectedEventHandler<ISerialPortClient> Connected { get; set; }

    /// <summary>
    /// 准备连接串口的时候
    /// </summary>
    ConnectingEventHandler<ISerialPortClient> Connecting { get; set; }

    /// <summary>
    /// 断开连接
    /// </summary>
    ClosedEventHandler<ISerialPortClient> Closed { get; set; }

    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// <para>
    /// </para>
    /// </summary>
    ClosingEventHandler<ISerialPortClient> Closing { get; set; }

    /// <summary>
    /// 主通信器
    /// </summary>
    SerialPort MainSerialPort { get; }

    /// <summary>
    /// 接收到数据
    /// </summary>
    ReceivedEventHandler<ISerialPortClient> Received { get; set; }
}