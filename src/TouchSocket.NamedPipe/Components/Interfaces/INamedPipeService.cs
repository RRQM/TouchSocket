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

using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// INamedPipeService
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface INamedPipeService<TClient> : INamedPipeServiceBase where TClient : INamedPipeSocketClient
    {
        /// <summary>
        /// 用户连接完成
        /// </summary>
        ConnectedEventHandler<TClient> Connected { get; set; }

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        ConnectingEventHandler<TClient> Connecting { get; set; }

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        DisconnectEventHandler<TClient> Disconnected { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        DisconnectEventHandler<TClient> Disconnecting { get; set; }

        /// <summary>
        /// 尝试获取TClient
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="socketClient">TClient</param>
        /// <returns></returns>
        bool TryGetSocketClient(string id, out TClient socketClient);
    }

    /// <summary>
    /// INamedPipeService
    /// </summary>
    public interface INamedPipeService : INamedPipeService<NamedPipeSocketClient>
    {
    }
}