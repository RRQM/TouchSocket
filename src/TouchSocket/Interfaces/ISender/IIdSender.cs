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

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了一个接口，用于向特定客户端发送数据
/// </summary>
public interface IIdSender
{
    /// <summary>
    /// 异步向对应Id的客户端发送数据
    /// </summary>
    /// <param name="id">目标客户端的唯一标识符</param>
    /// <param name="memory">要发送的数据，以字节形式存储在内存中</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <exception cref="ClientNotConnectedException">如果目标客户端未连接，则抛出此异常</exception>
    /// <exception cref="ClientNotFindException">如果无法根据Id找到对应的客户端，则抛出此异常</exception>
    /// <exception cref="Exception">如果发生其他异常情况</exception>
    Task SendAsync(string id, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default);
}