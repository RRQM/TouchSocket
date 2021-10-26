using System;

namespace RRQMCore.Run
{
    /// <summary>
    /// 等待返回类
    /// </summary>
    [Serializable]
    public class WaitResult : IWaitResult
    {
        /// <summary>
        /// 消息
        /// </summary>
        protected string message;

        /// <summary>
        /// 标记号
        /// </summary>
        protected int sign;

        /// <summary>
        /// 状态
        /// </summary>
        protected byte status;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// 标记号
        /// </summary>
        public int Sign
        {
            get { return sign; }
            set { sign = value; }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public byte Status
        {
            get { return status; }
            set { status = value; }
        }
    }
}