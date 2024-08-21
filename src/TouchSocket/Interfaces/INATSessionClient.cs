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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 定义了<see cref="INatSessionClient"/>接口，它扩展了ITcpSessionClient接口。
    /// 该接口专门用于处理需要网络地址转换（NAT）支持的TCP会话客户端操作。
    /// </summary>
    public interface INatSessionClient : ITcpSessionClient
    {
        /// <summary>
        /// 添加一个转发客户端。
        /// </summary>
        /// <param name="config">配置文件，用于设置TcpClient的连接参数。</param>
        /// <param name="setupAction">当完成配置，但是还未连接时的回调操作。</param>
        /// <returns>返回创建的ITcpClient对象。</returns>
        Task<ITcpClient> AddTargetClientAsync(TouchSocketConfig config, Action<ITcpClient> setupAction = default);

        /// <summary>
        /// 获取所有目标客户端。
        /// </summary>
        /// <returns>返回一个包含所有目标客户端的数组。</returns>
        ITcpClient[] GetTargetClients();
        /// <summary>
        /// 异步发送数据到目标客户端。
        /// </summary>
        /// <param name="memory">要发送的数据，以只读内存的形式传入。</param>
        /// <remarks>
        /// 本方法旨在提供一种高效的数据传输方式，通过直接传递内存地址和长度，
        /// 避免了不必要的数据拷贝，从而提高性能。使用<see cref="ReadOnlyMemory{T}"/>可以确保
        /// 传递给本方法的数据在发送过程中不会被修改。
        /// </remarks>
        Task SendToTargetClientAsync(ReadOnlyMemory<byte> memory);
    }
}