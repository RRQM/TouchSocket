using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件终端接口
    /// </summary>
    public interface IFileClient
    {
        /// <summary>
        /// 下载文件信息
        /// </summary>
        FileInfo DownloadFileInfo { get; }

        /// <summary>
        /// 上传文件信息
        /// </summary>
        FileInfo UploadFileInfo { get; }

        /// <summary>
        /// 下载进度
        /// </summary>
        float DownloadProgress { get; }

        /// <summary>
        /// 上传进度
        /// </summary>
        float UploadProgress { get; }
       
        /// <summary>
        /// 下载速度
        /// </summary>
        long DownloadSpeed { get; }

        /// <summary>
        /// 下载速度
        /// </summary>
        long UploadSpeed { get; }

        /// <summary>
        /// 文件传输类型
        /// </summary>
        TransferType TransferType { get; }

        /// <summary>
        /// 下载的文件包
        /// </summary>
        ProgressBlockCollection DownloadFileBlocks { get; }

        /// <summary>
        /// 上传的文件包
        /// </summary>
        ProgressBlockCollection UploadFileBlocks { get; }
    }
}
