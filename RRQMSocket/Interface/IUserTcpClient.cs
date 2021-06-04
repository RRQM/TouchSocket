//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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
using System;
using System.Net;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// TCP客户端终端接口
    /// </summary>
    public interface IUserTcpClient:ITcpClient, IUserClient
    {
        /// <summary>
        /// 仅发送，即不会开启接收线程。
        /// </summary>
        bool OnlySend { get;}

        /// <summary>
        /// 客户端配置
        /// </summary>
        TcpClientConfig ClientConfig { get; }

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();
    }
}