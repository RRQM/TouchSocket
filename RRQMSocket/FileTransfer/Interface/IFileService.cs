using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 服务器接口
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 最大下载速度
        /// </summary>
        long MaxDownloadSpeed { get; set;}

        /// <summary>
        /// 最大上传速度
        /// </summary>
        long MaxUploadSpeed { get; set;}
    }
}
