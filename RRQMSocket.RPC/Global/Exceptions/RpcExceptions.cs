//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using RRQMCore;
using System;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 放弃Rpc执行
    /// </summary>
    public class AbandonRpcException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="feedback">是否反馈信息</param>
        /// <param name="message">信息</param>
        public AbandonRpcException(bool feedback, string message) : base(message)
        {
            this.Feedback = feedback;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">信息</param>
        public AbandonRpcException(string message) : this(true, message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AbandonRpcException() : this(true, null)
        {
        }

        /// <summary>
        /// 是否反馈信息
        /// </summary>
        public bool Feedback { get; private set; }
    }

    /// <summary>
    /// Rpc异常
    /// </summary>
    [Serializable]
    public class RpcException : RRQMException
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RpcException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RpcException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RpcException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RpcException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Rpc调用异常
    /// </summary>
    [Serializable]
    public class RRQMRpcInvokeException : RRQMException
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RRQMRpcInvokeException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMRpcInvokeException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMRpcInvokeException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMRpcInvokeException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
