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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了等待客户端的接口，该客户端既支持发送操作，又能在异步操作中等待结果。
/// </summary>
/// <typeparam name="TClient">客户端类型参数，该客户端既是一个接收者，也是一个发送者。</typeparam>
/// <typeparam name="TResult">结果类型参数，表示接收者客户端处理操作后返回的结果。</typeparam>
public interface IWaitingClient<TClient, TResult> : IDisposableObject
    where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
    where TResult : IReceiverResult
{
    /// <summary>
    /// 等待设置。
    /// </summary>
    WaitingOptions WaitingOptions { get; }

    /// <summary>
    /// 客户端终端
    /// </summary>
    TClient Client { get; }

    /// <summary>
    /// 异步发送
    /// </summary>
    /// <param name="memory">要发送的数据，使用内存表示</param>
    /// <param name="token">取消令箭，用于取消操作</param>
    /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
    /// <exception cref="OverlengthException">发送数据超长</exception>
    /// <exception cref="Exception">其他异常</exception>
    /// <returns>返回的数据，类型为ResponsedData</returns>
    Task<ResponsedData> SendThenResponseAsync(ReadOnlyMemory<byte> memory, CancellationToken token);


    /// <summary>
    /// 异步发送请求并等待响应
    /// </summary>
    /// <param name="requestInfo">请求信息，包含发送请求所需的所有细节</param>
    /// <param name="token">用于取消操作的取消令牌</param>
    /// <returns>返回一个任务，该任务的结果是服务器的响应数据</returns>
    Task<ResponsedData> SendThenResponseAsync(IRequestInfo requestInfo, CancellationToken token);
}