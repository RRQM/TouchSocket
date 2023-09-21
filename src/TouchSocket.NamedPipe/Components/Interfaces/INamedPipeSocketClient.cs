﻿using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道服务器客户端接口
    /// </summary>
    public interface INamedPipeSocketClient : INamedPipeClientBase, IClientSender, IIdSender, IIdRequsetInfoSender
    {
        /// <summary>
        /// 服务器唯一Id。
        /// </summary>
        string Id { get; }
    }
}