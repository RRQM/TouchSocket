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

using System.Net;
using TouchSocket.Sockets;

namespace TouchSocket.CoAP;

/// <summary>
/// 定义 CoAP 会话的公共功能接口，客户端（<see cref="CoAPClient"/>）和服务端（<see cref="CoAPServer"/>）均实现此接口。
/// </summary>
public interface ICoAPSession : IUdpSessionBase
{
    /// <summary>
    /// 向指定端点发送一条 CoAP 消息。
    /// </summary>
    /// <param name="remoteEndPoint">目标端点。</param>
    /// <param name="message">要发送的 CoAP 消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task SendCoAPMessageAsync(EndPoint remoteEndPoint, CoAPMessage message, CancellationToken cancellationToken = default);
}
