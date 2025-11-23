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

using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// 用户终端接口
/// </summary>
public interface IWebSocketClient : IDependencyClient, IClosableClient, ISetupConfigObject, ITcpConnectableClient, IWebSocket
{
    /// <summary>
    /// 当WebSocket断开时触发。
    /// </summary>
    ClosedEventHandler<IWebSocketClient> Closed { get; set; }

    /// <summary>
    /// 当WebSocket收到Close报文时触发。
    /// </summary>
    ClosingEventHandler<IWebSocketClient> Closing { get; set; }

    /// <summary>
    /// 表示完成握手后。
    /// </summary>
    HttpContextEventHandler<IWebSocketClient> Connected { get; set; }

    /// <summary>
    /// 表示在即将握手连接时。
    /// </summary>
    HttpContextEventHandler<IWebSocketClient> Connecting { get; set; }

    /// <summary>
    /// 收到WebSocket数据
    /// </summary>
    WSDataFrameEventHandler<IWebSocketClient> Received { get; set; }
}