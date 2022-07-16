using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 未找到消息异常类
    /// </summary>
    [Serializable]
    public class MessageNotFoundException : Exception
    {
        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="mes"></param>
        public MessageNotFoundException(string mes) : base(mes)
        {
        }
    }
}