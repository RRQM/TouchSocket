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

namespace RRQMSocket
{
    /// <summary>
    /// 未连接异常
    /// </summary>
    [Serializable]
    public class RRQMNotConnectedException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMNotConnectedException() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMNotConnectedException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMNotConnectedException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMNotConnectedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// 超长异常
    /// </summary>
    [Serializable]
    public class RRQMOverlengthException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMOverlengthException() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMOverlengthException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMOverlengthException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMOverlengthException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Token验证异常
    /// </summary>
    [Serializable]
    public class RRQMTokenVerifyException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMTokenVerifyException() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMTokenVerifyException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMTokenVerifyException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMTokenVerifyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// 没有找到ID对应的客户端
    /// </summary>
    [Serializable]
    public class ClientNotFindException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ClientNotFindException() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public ClientNotFindException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ClientNotFindException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ClientNotFindException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// 协议异常
    /// </summary>
    [Serializable]
    public class ProtocolException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtocolException() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public ProtocolException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ProtocolException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ProtocolException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
