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

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 定义UDP请求信息发送接口。
/// </summary>
public interface IUdpRequestInfoSender
{
    /// <summary>
    /// 异步发送数据。
    /// <para>该发送会经过适配器封装，具体封装内容由适配器决定。</para>
    /// </summary>
    /// <param name="endPoint">发送数据的目标端点。</param>
    /// <param name="requestInfo">要发送的请求信息，包含具体的请求数据和元信息。</param>
    /// <param name="token">可取消令箭</param>
    /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
    /// <exception cref="OverlengthException">发送数据超长</exception>
    /// <exception cref="Exception">其他异常</exception>
    /// <remarks>
    /// 此方法为异步非阻塞方式，调用后立即返回，不保证数据发送成功。
    /// </remarks>
    Task SendAsync(EndPoint endPoint, IRequestInfo requestInfo, CancellationToken token = default);
}