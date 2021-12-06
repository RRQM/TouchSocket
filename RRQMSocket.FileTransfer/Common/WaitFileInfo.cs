using RRQMCore;
using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件信息
    /// </summary>
    public class WaitFileInfo : WaitResult
    {
        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// 文件信息
        /// </summary>
        public RRQMFileInfo FileInfo { get; set; }

        /// <summary>
        /// 文件请求
        /// </summary>
        public FileRequest FileRequest { get; set; }

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
