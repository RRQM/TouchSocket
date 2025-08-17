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
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 发送者扩展类
/// </summary>
public static class SenderExtension
{
    #region ISend

    /// <summary>
    /// 同步发送数据。
    /// </summary>
    /// <typeparam name="TClient">发送器类型参数，必须实现ISender接口。</typeparam>
    /// <param name="client">发送器实例。</param>
    /// <param name="memory">待发送的字节内存块，使用<see cref="ReadOnlyMemory{T}"/>类型以强调数据不会被修改。</param>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, ReadOnlyMemory<byte> memory) where TClient : ISender
    {
        // 调用SendAsync方法发送数据，并立即返回，不等待发送完成。这种设计用于提高性能，特别是在高负载情况下。
        client.SendAsync(memory).GetFalseAwaitResult();
    }

    /// <summary>
    /// 以UTF-8的编码同步发送字符串。
    /// </summary>
    /// <typeparam name="TClient">发送者类型，必须实现ISender接口。</typeparam>
    /// <param name="client">发送者实例。</param>
    /// <param name="value">待发送的字符串。</param>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, string value) where TClient : ISender
    {
        client.SendAsync(value).GetFalseAwaitResult();
    }

    /// <summary>
    /// 以UTF-8的编码异步发送字符串。
    /// </summary>
    /// <typeparam name="TClient">发送器类型参数，必须实现ISender接口。</typeparam>
    /// <param name="client">发送器实例。</param>
    /// <param name="value">待发送的字符串。</param>
    /// <returns>返回一个Task对象，表示异步操作。</returns>
    public static async Task SendAsync<TClient>(this TClient client, string value) where TClient : ISender
    {
        var byteBlock = new ByteBlock(1024);

        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, value, Encoding.UTF8);
            await client.SendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    #endregion ISend

    #region IRequestInfoSender

    /// <summary>
    /// 同步发送请求信息。
    /// </summary>
    /// <param name="client">发起请求的客户端对象。</param>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <typeparam name="TClient">客户端对象的类型，必须实现<see cref="IRequestInfoSender"/>接口。</typeparam>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, IRequestInfo requestInfo) where TClient : IRequestInfoSender
    {
        // 直接调用客户端对象的SendAsync方法并获取错误的等待结果
        // 这里使用GetFalseAwaitResult()是因为SendAsync方法可能不返回Task对象
        // 这种情况下，GetFalseAwaitResult()可以防止编译器警告，并且不会影响程序的执行
        client.SendAsync(requestInfo).GetFalseAwaitResult();
    }

    #endregion IRequestInfoSender

    #region IIdSender

    /// <summary>
    /// 同步发送请求方法
    /// </summary>
    /// <typeparam name="TClient">泛型参数，表示客户端类型，必须实现IIdRequestInfoSender接口</typeparam>
    /// <param name="client">客户端实例</param>
    /// <param name="id">请求的目标ID</param>
    /// <param name="requestInfo">请求信息对象，包含请求的各种细节</param>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, string id, IRequestInfo requestInfo) where TClient : IIdRequestInfoSender
    {
        // 调用异步发送方法而不等待结果，实现同步发送的效果
        client.SendAsync(id, requestInfo).GetFalseAwaitResult();
    }

    /// <summary>
    /// 以UTF-8的编码同步发送字符串。
    /// </summary>
    /// <typeparam name="TClient">泛型参数，表示客户端类型，必须实现IIdSender接口。</typeparam>
    /// <param name="client">客户端实例，用于发送数据。</param>
    /// <param name="id">标识符，用于指定发送的目标。</param>
    /// <param name="value">要发送的字符串内容。</param>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, string id, string value) where TClient : IIdSender
    {
        client.SendAsync(id, value).GetFalseAwaitResult();
    }

    /// <summary>
    /// 同步发送数据。
    /// </summary>
    /// <typeparam name="TClient">发送器类型参数，必须实现IIdSender接口。</typeparam>
    /// <param name="client">发送器实例。</param>
    /// <param name="id">发送的数据的唯一标识。</param>
    /// <param name="memory">待发送的字节内存块，使用<see cref="ReadOnlyMemory{T}"/>以强调数据不会被修改。</param>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, string id, in ReadOnlyMemory<byte> memory) where TClient : IIdSender
    {
        // 直接调用SendAsync方法并获取结果，这里使用GetFalseAwaitResult是因为发送操作不需要等待完成。
        client.SendAsync(id, memory).GetFalseAwaitResult();
    }

    /// <summary>
    /// 以UTF-8的编码异步发送字符串。
    /// </summary>
    /// <typeparam name="TClient">发送器类型，必须实现IIdSender接口。</typeparam>
    /// <param name="client">发送器实例。</param>
    /// <param name="id">发送的目标标识符。</param>
    /// <param name="value">要发送的字符串内容。</param>
    /// <returns>返回一个Task对象，表示异步操作。</returns>
    public static Task SendAsync<TClient>(this TClient client, string id, string value) where TClient : IIdSender
    {
        // 将字符串转换为UTF-8编码的字节数组，以便发送。
        return client.SendAsync(id, Encoding.UTF8.GetBytes(value));
    }

    #endregion IIdSender

    #region IUdpClientSender

    /// <summary>
    /// 以UTF-8的编码同步发送字符串。
    /// </summary>
    /// <typeparam name="TClient">泛型参数，限定为实现了IUdpClientSender接口的类型。</typeparam>
    /// <param name="client">用于发送数据的客户端实例。</param>
    /// <param name="endPoint">发送数据的目的地，表示为一个端点。</param>
    /// <param name="value">需要发送的字符串数据。</param>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, EndPoint endPoint, string value) where TClient : IUdpClientSender
    {
        client.SendAsync(endPoint, value).GetFalseAwaitResult();
    }

    /// <summary>
    /// 同步发送数据到指定的端点。
    /// </summary>
    /// <typeparam name="TClient">发送客户端的类型，必须实现<see cref="IUdpClientSender"/>接口。</typeparam>
    /// <param name="client">发送客户端实例。</param>
    /// <param name="endPoint">数据发送的目标端点。</param>
    /// <param name="memory">待发送的数据，以只读内存的方式提供。</param>
    [AsyncToSyncWarning]
    public static void Send<TClient>(this TClient client, EndPoint endPoint, ReadOnlyMemory<byte> memory)
        where TClient : IUdpClientSender
    {
        // 调用SendAsync方法并忽略结果，实现同步发送数据的逻辑。
        client.SendAsync(endPoint, memory).GetFalseAwaitResult();
    }

    /// <summary>
    /// 以UTF-8的编码异步发送字符串。
    /// </summary>
    /// <typeparam name="TClient">泛型参数，表示UDP客户端发送器的类型。</typeparam>
    /// <param name="client">UDP客户端实例，用于发送数据。</param>
    /// <param name="endPoint">发送数据的目的地，可以是IP地址和端口号的组合。</param>
    /// <param name="value">需要发送的字符串内容。</param>
    /// <returns>返回一个Task对象，代表异步操作的完成状态。</returns>
    public static async Task SendAsync<TClient>(this TClient client, EndPoint endPoint, string value) where TClient : IUdpClientSender
    {
        var byteBlock = new ByteBlock(1024);

        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, value, Encoding.UTF8);
            await client.SendAsync(endPoint, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    #endregion IUdpClientSender
}