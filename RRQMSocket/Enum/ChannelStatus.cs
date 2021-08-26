using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 状态
    /// </summary>
    public enum ChannelStatus : byte
    {
        /// <summary>
        /// 本次操作成功
        /// </summary>
        Success,

        /// <summary>
        /// 超时
        /// </summary>
        Timeout,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel,

        /// <summary>
        /// 完成
        /// </summary>
        Completed,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 已释放
        /// </summary>
        Disposed
    }
}
