using System;

namespace TouchSocket.Smtp.Rpc
{
    /// <summary>
    /// Rpc无注册异常
    /// </summary>
    [Serializable]
    public class RpcNoRegisterException : Exception
    {
        /// <summary>
        ///构造函数
        /// </summary>
        public RpcNoRegisterException() : base() { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        public RpcNoRegisterException(string message) : base(message) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RpcNoRegisterException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RpcNoRegisterException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
