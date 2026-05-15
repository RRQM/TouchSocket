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

namespace TouchSocket.SocketIo;

/// <summary>
/// Socket.IO 消息接口，表示一条已解码的 Socket.IO 数据包。
/// </summary>
public interface ISocketIoMessage : IWaitHandle
{
    /// <summary>
    /// 获取消息的参数数量。
    /// </summary>
    int ArgsCount { get; }

    /// <summary>
    /// 获取包含二进制数据的参数索引集合。
    /// </summary>
    int[] BytesIndices { get; }

    /// <summary>
    /// 获取错误信息。仅当 <see cref="MessageType"/> 为 <see cref="SocketIoMessageType.Error"/> 时有效。
    /// </summary>
    string Error { get; }

    /// <summary>
    /// 获取事件名称。仅当 <see cref="MessageType"/> 为 <see cref="SocketIoMessageType.Event"/> 或 <see cref="SocketIoMessageType.Binary"/> 时有效。
    /// </summary>
    string Event { get; }

    /// <summary>
    /// 获取数据包 ID。不为 <see langword="null"/> 时表示发送方期望收到 Ack 响应。
    /// </summary>
    int? Id { get; }

    /// <summary>
    /// 获取消息类型。
    /// </summary>
    SocketIoMessageType MessageType { get; }

    /// <summary>
    /// 获取命名空间。
    /// </summary>
    string Namespace { get; }

    /// <summary>
    /// 尝试获取指定参数索引处的二进制数据。
    /// </summary>
    /// <param name="index">参数索引。</param>
    /// <param name="bytes">成功时返回对应的二进制数据。</param>
    /// <returns>若该索引处存在二进制数据则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    bool TryGetBytes(int index, out ReadOnlyMemory<byte> bytes);
}