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
/// Socket.IO 响应接口，提供从消息中读取参数值的能力。
/// </summary>
public interface ISocketIoResponse
{
    /// <summary>
    /// 获取参数数量。
    /// </summary>
    int ArgsCount { get; }

    /// <summary>
    /// 获取包含二进制数据的参数索引集合。
    /// </summary>
    int[] BytesIndices { get; }

    /// <summary>
    /// 获取是否可以发送 Ack 响应。
    /// </summary>
    bool CanAck { get; }

    /// <summary>
    /// 将指定索引处的参数反序列化为目标类型。
    /// </summary>
    /// <param name="targetType">目标类型。</param>
    /// <param name="index">参数索引。</param>
    object GetValue(Type targetType, int index);

    /// <summary>
    /// 将指定索引处的参数反序列化为 <typeparamref name="T"/> 类型。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="index">参数索引。</param>
    T GetValue<T>(int index);
}