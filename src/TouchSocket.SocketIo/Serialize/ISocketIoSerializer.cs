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
/// Socket.IO 序列化器接口，负责消息的编解码与数据的序列化/反序列化。
/// </summary>
public interface ISocketIoSerializer
{
    /// <summary>
    /// 将 Engine.IO 消息解码为 Socket.IO 消息。
    /// </summary>
    ISocketIoMessage Decode(in EngineIoMessage message);

    /// <summary>
    /// 将 Socket.IO 消息中指定索引处的数据反序列化为目标类型。
    /// </summary>
    object Deserialize(Type targetType, in ISocketIoMessage message, int index);

    /// <summary>
    /// 反序列化握手消息。
    /// </summary>
    IConnectMessage DeserializeHandshakeMessage(in EngineIoMessage message);

    /// <summary>
    /// 序列化 Ack 响应数据。
    /// </summary>
    List<DataItem> SerializeAck(int? packetId, string nsp, object[] data);

    /// <summary>
    /// 序列化事件数据。
    /// </summary>
    List<DataItem> SerializeEvent(string eventName, int? packetId, string nsp, object[] data);
}