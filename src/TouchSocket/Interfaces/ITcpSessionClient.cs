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
    /// 定义 ITcpSessionClient 接口，继承自多个接口，以支持 TCP 会话客户端的功能。
    /// </summary>
    public interface ITcpSessionClient : ITcpSession, IClientSender, IIdSender, IIdRequestInfoSender, ITcpListenableClient, ISessionClient, IReceiverClient<IReceiverResult>
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        ClosedEventHandler<ITcpSessionClient> Closed { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 此事件标识在与 <see cref="ITcpSessionClient"/> 的连接即将主动断开时发生的事件。提供此事件是为了允许执行断开连接前的清理操作。
        /// </para>
        /// </summary>
        ClosingEventHandler<ITcpSessionClient> Closing { get; set; }

    }
}