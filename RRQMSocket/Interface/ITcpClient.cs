//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using RRQMCore.Exceptions;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端终端接口
    /// </summary>
    public interface ITcpClient : ITcpClientBase, ISendBase
    {
        /// <summary>
        /// 客户端配置
        /// </summary>
        TcpClientConfig ClientConfig { get; }

        /// <summary>
        /// 仅发送，即不会开启接收线程。
        /// </summary>
        bool OnlySend { get; }

        /// <summary>
        /// 独立线程发送
        /// </summary>
        bool SeparateThreadSend { get; }

        /// <summary>
        /// 关闭Socket信道，并随后释放资源
        /// </summary>
        void Close();

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        ITcpClient Connect();

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        Task<ITcpClient> ConnectAsync();

        /// <summary>
        /// 断开连接
        /// </summary>
        ITcpClient Disconnect();

        /// <summary>
        /// 同步组合发送
        /// </summary>
        /// <param name="transferBytes"></param>
        void Send(IList<TransferByte> transferBytes);

        /// <summary>
        /// 异步组合发送
        /// </summary>
        /// <param name="transferBytes"></param>
        void SendAsync(IList<TransferByte> transferBytes);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        ITcpClient Setup(TcpClientConfig clientConfig);

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        void Shutdown(SocketShutdown how);
    }
}