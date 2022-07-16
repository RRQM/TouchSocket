using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 消息已注册
    /// </summary>
    [Serializable]
    public class MessageRegisteredException : Exception
    {
        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="mes"></param>
        public MessageRegisteredException(string mes) : base(mes)
        {
        }
    }
}