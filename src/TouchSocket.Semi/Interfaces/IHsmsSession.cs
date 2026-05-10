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

namespace TouchSocket.Semi;

/// <summary>
/// 定义 HSMS 会话的公共功能，客户端（<see cref="IHsmsClient"/>）和服务端会话（<see cref="IHsmsSessionClient"/>）均实现此接口。
/// </summary>
public interface IHsmsSession : ITcpSession
{
    /// <summary>
    /// 发送一条 HSMS 消息，并等待响应（若 <see cref="HsmsMessage.ReplyExpected"/> 为 <see langword="true"/>）。
    /// </summary>
    /// <param name="message">要发送的消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>响应消息，若无需回复则返回 <see langword="null"/>。</returns>
    Task<HsmsMessage?> SendHsmsMessageAsync(HsmsMessage message, CancellationToken cancellationToken = default);
}
