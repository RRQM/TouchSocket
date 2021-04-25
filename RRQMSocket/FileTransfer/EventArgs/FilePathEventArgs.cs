using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件路径事件类
    /// </summary>
   public class FilePathEventArgs: TransferFileMessageArgs
    {
        /// <summary>
        /// 获取或设置目标文件的最终路径，包含文件名及文件扩展名
        /// </summary>
        public string TargetPath { get; set; }
    }
}
