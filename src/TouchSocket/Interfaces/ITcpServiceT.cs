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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 定义了一个泛型接口 ITcpService{TClient}，用于处理 TCP 服务的核心功能。
    /// </summary>
    /// <typeparam name="TClient">客户端会话类型，必须实现 ITcpSessionClient 接口。</typeparam>
    public interface ITcpService<TClient> : ITcpServiceBase<TClient>, IIdSender, IIdRequestInfoSender
        where TClient : ITcpSessionClient

    {
        /// <summary>
        /// 用户连接完成时的事件处理程序
        /// </summary>
        ConnectedEventHandler<TClient> Connected { get; set; }


        /// <summary>
        /// 当有用户连接时触发的事件
        /// </summary>
        ConnectingEventHandler<TClient> Connecting { get; set; }


        /// <summary>
        /// 用户断开连接事件的事件处理程序
        /// </summary>
        /// <typeparam name="TClient">客户端类型的泛型参数</typeparam>
        /// <remarks>
        /// 此属性用于获取或设置一个事件处理程序，该处理程序在用户断开连接时被调用
        /// </remarks>
        ClosedEventHandler<TClient> Closed { get; set; }


        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        ClosingEventHandler<TClient> Closing { get; set; }
    }
}