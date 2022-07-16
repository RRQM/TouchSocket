using System;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 没有找到ID对应的客户端
    /// </summary>
    [Serializable]
    public class ClientNotFindException : Exception
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
        public ClientNotFindException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ClientNotFindException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
