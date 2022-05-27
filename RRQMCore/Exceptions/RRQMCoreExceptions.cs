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
using RRQMCore.Run;
using System;

namespace RRQMCore
{
    /// <summary>
    /// 若汝棋茗程序集异常类基类
    /// </summary>
    [Serializable]
    public class RRQMException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMException() : base()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RRQMException(ResType resType, params object[] fms) : base(resType.GetDescription(fms)) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public RRQMException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// 消息已注册
    /// </summary>
    [Serializable]
    public class MessageRegisteredException : RRQMException
    {
        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="mes"></param>
        public MessageRegisteredException(string mes) : base(mes)
        {
        }
    }

    /// <summary>
    /// 未找到消息异常类
    /// </summary>
    [Serializable]
    public class MessageNotFoundException : RRQMException
    {
        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="mes"></param>
        public MessageNotFoundException(string mes) : base(mes)
        {
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    [Serializable]
    public class LicenceKeyInvalidException : RRQMException
    {
        /// <summary>
        /// 7ashd7ashd7ashdahsd77
        /// </summary>
        public int i;

        private int I
        {
            set => this.I = value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LicenceKeyInvalidException()
        {
            EasyAction.DelayRun(5000, () => { this.I = 1000; });
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public LicenceKeyInvalidException(string message) : base(message)
        {
            EasyAction.DelayRun(5000, () => { this.I = 1000; });
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public LicenceKeyInvalidException(string message, Exception inner) : base(message, inner)
        {
            EasyAction.DelayRun(5000, () => { this.I = 1000; });
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected LicenceKeyInvalidException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}