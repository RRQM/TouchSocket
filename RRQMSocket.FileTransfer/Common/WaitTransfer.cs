using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 等待传输
    /// </summary>
    public class WaitTransfer : WaitResult
    {
        /// <summary>
        /// 通道标识
        /// </summary>
        public int ChannelID { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 流位置
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// 事件Code
        /// </summary>
        public int EventHashCode { get; set; }

        /// <summary>
        /// 包长度
        /// </summary>
        public int PackageSize { get; set; }
    }
}
