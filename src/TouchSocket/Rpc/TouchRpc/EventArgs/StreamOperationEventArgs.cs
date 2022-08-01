//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 接收流数据
    /// </summary>
    public class StreamOperationEventArgs : StreamEventArgs
    {
        private readonly StreamOperator streamOperator;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <param name="streamInfo"></param>
        public StreamOperationEventArgs(StreamOperator streamOperator, Metadata metadata, StreamInfo streamInfo) : base(metadata, streamInfo)
        {
            this.streamOperator = streamOperator ?? throw new ArgumentNullException(nameof(streamOperator));
            this.AddOperation(Operation.Permit);
        }


        /// <summary>
        /// 流操作
        /// </summary>
        public StreamOperator StreamOperator => this.streamOperator;
    }
}