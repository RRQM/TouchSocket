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

using System.Buffers;
using System.Net;

namespace TouchSocket.Sockets;

/// <summary>
/// 发送者扩展类
/// </summary>
public static class SenderExtension
{
    #region ISend

    /// <summary>
    /// 异步发送只读内存数据。
    /// </summary>
    /// <typeparam name="TClient">发送器类型参数，必须实现ISender接口。</typeparam>
    /// <param name="client">发送器实例。</param>
    /// <param name="memory">待发送的数据。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个Task对象，表示异步操作。</returns>
    public static Task SendAsync<TClient>(this TClient client, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default) where TClient : ISender
    {
        var sequence = new ReadOnlySequence<byte>(memory);
        return client.SendAsync(sequence, cancellationToken);
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
        var writer = new SegmentedBytesWriter(1024);

        try
        {
            WriterExtension.WriteNormalString(ref writer, value, Encoding.UTF8);
            await client.SendAsync(writer.Sequence).ConfigureDefaultAwait();
        }
        finally
        {
            writer.Dispose();
        }
    }

    #endregion ISend

    #region IIdSender

    /// <summary>
    /// 异步向对应Id的客户端发送只读内存数据。
    /// </summary>
    /// <typeparam name="TClient">发送器类型，必须实现IIdSender接口。</typeparam>
    /// <param name="client">发送器实例。</param>
    /// <param name="id">发送的目标标识符。</param>
    /// <param name="memory">待发送的数据。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个Task对象，表示异步操作。</returns>
    public static Task SendAsync<TClient>(this TClient client, string id, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default) where TClient : IIdSender
    {
        var sequence = new ReadOnlySequence<byte>(memory);
        return client.SendAsync(id, sequence, cancellationToken);
    }

    /// <summary>
    /// 以UTF-8的编码异步发送字符串。
    /// </summary>
    /// <typeparam name="TClient">发送器类型，必须实现IIdSender接口。</typeparam>
    /// <param name="client">发送器实例。</param>
    /// <param name="id">发送的目标标识符。</param>
    /// <param name="value">要发送的字符串内容。</param>
    /// <returns>返回一个Task对象，表示异步操作。</returns>
    public static async Task SendAsync<TClient>(this TClient client, string id, string value) where TClient : IIdSender
    {
        var writer = new SegmentedBytesWriter(1024);

        try
        {
            WriterExtension.WriteNormalString(ref writer, value, Encoding.UTF8);
            await client.SendAsync(id, writer.Sequence).ConfigureDefaultAwait();
        }
        finally
        {
            writer.Dispose();
        }
    }

    #endregion IIdSender

    #region IUdpClientSender

    /// <summary>
    /// 异步向指定UDP终结点发送只读内存数据。
    /// </summary>
    /// <typeparam name="TClient">泛型参数，表示UDP客户端发送器的类型。</typeparam>
    /// <param name="client">UDP客户端实例，用于发送数据。</param>
    /// <param name="endPoint">发送数据的目的地。</param>
    /// <param name="memory">待发送的数据。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>返回一个Task对象，代表异步操作的完成状态。</returns>
    public static Task SendAsync<TClient>(this TClient client, EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default) where TClient : IUdpClientSender
    {
        var sequence = new ReadOnlySequence<byte>(memory);
        return client.SendAsync(endPoint, sequence, cancellationToken);
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
        var writer = new SegmentedBytesWriter(1024);

        try
        {
            WriterExtension.WriteNormalString(ref writer, value, Encoding.UTF8);
            await client.SendAsync(endPoint, writer.Sequence).ConfigureDefaultAwait();
        }
        finally
        {
            writer.Dispose();
        }
    }

    #endregion IUdpClientSender
}
