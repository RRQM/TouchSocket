using System;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 清理统计类型
    /// </summary>
    [Flags]
    public enum ClearType
    {
        /// <summary>
        /// 从发送统计
        /// </summary>
        Send = 1,

        /// <summary>
        /// 从接收统计
        /// </summary>
        Receive = 2
    }
}
