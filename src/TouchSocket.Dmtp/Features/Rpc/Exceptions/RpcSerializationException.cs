using System;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 序列化异常类
    /// </summary>
    [Serializable]
    public class RpcSerializationException : Exception
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RpcSerializationException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RpcSerializationException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RpcSerializationException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RpcSerializationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}