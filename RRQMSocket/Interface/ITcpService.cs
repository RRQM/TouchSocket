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
    /// TCP系列服务器接口
    /// </summary>
    public interface ITcpService : IService
    {
        /// <summary>
        /// 获取或设置最大可连接数
        /// </summary>
        int MaxCount { get; }

        /// <summary>
        /// 检验客户端活性（避免异常而导致的失活）
        /// </summary>
        bool IsCheckClientAlive { get;}

        /// <summary>
        /// 客户端成功连接时
        /// </summary>
        event RRQMMessageEventHandler ClientConnected;

        /// <summary>
        /// 有用户断开连接的时候
        /// </summary>
        event RRQMMessageEventHandler ClientDisconnected;
    }
}