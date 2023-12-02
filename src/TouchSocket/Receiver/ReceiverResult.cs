//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ReceiverResult
    /// </summary>
    public readonly struct ReceiverResult : IDisposable
    {
        private readonly Action m_disAction;

        /// <summary>
        /// SocketReceiveResult
        /// </summary>
        /// <param name="disAction"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        public ReceiverResult(Action disAction, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.m_disAction = disAction;
            this.ByteBlock = byteBlock;
            this.RequestInfo = requestInfo;
        }

        /// <summary>
        /// 字节块
        /// </summary>
        public ByteBlock ByteBlock { get; }

        /// <summary>
        /// 数据对象
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// 连接已关闭
        /// </summary>
        public bool IsClosed => this.ByteBlock == null && this.RequestInfo == null;

        /// <inheritdoc/>
        public void Dispose()
        {
            this.m_disAction?.Invoke();
        }
    }
}