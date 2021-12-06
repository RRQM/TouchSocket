//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 路径不合法
    /// </summary>
    [System.Serializable]
    public class RRQMPathInvalidException : RRQMException
    {
        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        public RRQMPathInvalidException() : base() { }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="message"></param>
        public RRQMPathInvalidException(string message) : base(message) { }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMPathInvalidException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMPathInvalidException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
