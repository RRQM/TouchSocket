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
using System.Net;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 提供与UDP发送操作相关的事件处理方法。
    /// </summary>
    /// <remarks>
    /// 该类继承自<see cref="SendingEventArgs"/>，专门化处理UDP发送事件，
    /// 包含了发送数据的目的端点信息。
    /// </remarks>
    public class UdpSendingEventArgs : SendingEventArgs
    {
        /// <summary>
        /// 初始化<see cref="UdpSendingEventArgs"/>类的新实例。
        /// </summary>
        /// <param name="memory">要发送的数据内存区块。</param>
        /// <param name="endPoint">数据发送的目的端点。</param>
        public UdpSendingEventArgs(in ReadOnlyMemory<byte> memory, EndPoint endPoint) : base(memory)
        {
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// 获取发送数据的目的端点。
        /// </summary>
        public EndPoint EndPoint { get; }
    }
}
