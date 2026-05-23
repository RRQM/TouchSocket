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

namespace TouchSocket.CoAP;

/// <summary>
/// 提供 <see cref="ICoAPSession"/> 相关扩展方法。
/// </summary>
public static class CoAPSessionExtensions
{
    /// <summary>
    /// 向指定端点发送 CoAP 请求消息。
    /// </summary>
    /// <param name="session">CoAP 会话。</param>
    /// <param name="remoteEndPoint">目标端点。</param>
    /// <param name="request">要发送的 CoAP 请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static Task SendCoAPRequestAsync(this ICoAPSession session, EndPoint remoteEndPoint, CoAPRequest request, CancellationToken cancellationToken = default)
    {
        return session.SendCoAPMessageAsync(remoteEndPoint, request, cancellationToken);
    }

    /// <summary>
    /// 向指定端点发送 CoAP 响应消息。
    /// </summary>
    /// <param name="session">CoAP 会话。</param>
    /// <param name="remoteEndPoint">目标端点。</param>
    /// <param name="response">要发送的 CoAP 响应。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static Task SendCoAPResponseAsync(this ICoAPSession session, EndPoint remoteEndPoint, CoAPResponse response, CancellationToken cancellationToken = default)
    {
        return session.SendCoAPMessageAsync(remoteEndPoint, response, cancellationToken);
    }
}
