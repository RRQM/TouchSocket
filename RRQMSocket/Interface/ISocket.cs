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
using System;
using System.Net.Sockets;
using RRQMCore.Log;

namespace RRQMSocket
{
    /// <summary>
    /// Socket基接口
    /// </summary>
    public interface ISocket:IDisposable
    {
        /// <summary>
        /// IP
        /// </summary>
        string IP { get; }

        /// <summary>
        /// 端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 主通信器
        /// </summary>
        Socket MainSocket { get; }

        /// <summary>
        /// 数据交互缓存池限制
        /// </summary>
        int BufferLength { get; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; set; }
    }
}