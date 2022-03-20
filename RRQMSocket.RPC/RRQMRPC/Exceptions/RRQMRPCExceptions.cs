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

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 事件名称异常
    /// </summary>
    [Serializable]
    public class EventNameException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public EventNameException()
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public EventNameException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public EventNameException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventNameException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// 事件访问异常
    /// </summary>
    [Serializable]
    public class EventAccessException : RRQMException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public EventAccessException() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public EventAccessException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public EventAccessException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventAccessException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// RPC添加方法键异常
    /// </summary>
    public class RRQMRPCKeyException : RRQMException
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RRQMRPCKeyException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMRPCKeyException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMRPCKeyException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMRPCKeyException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// RPC无注册异常
    /// </summary>
    public class RRQMRPCNoRegisterException : RRQMException
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RRQMRPCNoRegisterException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMRPCNoRegisterException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMRPCNoRegisterException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMRPCNoRegisterException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// 序列化异常类
    /// </summary>
    public class RRQMSerializationException : RRQMException
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RRQMSerializationException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMSerializationException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMSerializationException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMSerializationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}