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
    public class RRQMFileInfo
    {
        /// <summary>
        /// 文件哈希ID
        /// </summary>
        public string FileHashID { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileLength { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 流位置
        /// </summary>
        public long Posotion { get; set; }
    }
}
