using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// ValueWaitResult
    /// </summary>
    [Serializable]
    public struct ValueWaitResult : IWaitResult
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 标记号
        /// </summary>
        public long Sign { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public byte Status { get; set; }
    }
}