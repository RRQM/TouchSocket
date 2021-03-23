//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

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