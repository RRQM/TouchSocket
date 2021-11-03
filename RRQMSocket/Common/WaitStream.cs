using RRQMCore;
using RRQMCore.Run;

namespace RRQMSocket
{
    /// <summary>
    /// 等待流状态返回
    /// </summary>
    public class WaitStream : WaitResult
    {
        /// <summary>
        /// 流长度
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// 开启得通道标识
        /// </summary>
        public int ChannelID { get; set; }
    }
}
