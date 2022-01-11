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
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端终端接口
    /// </summary>
    public interface ITcpClient : ITcpClientBase, ISendBase
    {
        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        event RRQMMessageEventHandler<ITcpClient> Connected;

        /// <summary>
        /// 准备连接的时候
        /// </summary>
        event RRQMTcpClientConnectingEventHandler<ITcpClient> Connecting;

        /// <summary>
        /// 断开连接
        /// </summary>
        event RRQMMessageEventHandler<ITcpClient> Disconnected;

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
        /// <returns></returns>
        ITcpClient Disconnect();

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="RRQMException"></exception>
        ITcpClient Setup(TcpClientConfig clientConfig);

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="ipHost"></param>
        /// <returns></returns>
        ITcpClient Setup(string ipHost);
    }
}