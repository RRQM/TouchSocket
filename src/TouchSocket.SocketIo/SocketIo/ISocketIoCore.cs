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
/// Socket.IO 核心功能接口，提供消息响应和数据反序列化能力。
/// </summary>
public interface ISocketIoCore
{
    /// <summary>
    /// 发送 Ack 响应。
    /// </summary>
    Task AckAsync(int packetId, object[] data, CancellationToken cancellationToken);

    /// <summary>
    /// 将消息中指定索引处的数据反序列化为目标类型。
    /// </summary>
    object Deserialize(Type targetType, in ISocketIoMessage message, int index);
}