//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件信息基类
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileLength { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash { get; set; }

        /// <summary>
        /// 文件标志
        /// </summary>
        public string Flag { get; set; }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="fileInfo"></param>
        public void Copy(FileInfo fileInfo)
        {
            this.Flag = fileInfo.Flag;
            this.FileHash = fileInfo.FileHash;
            this.FileLength = fileInfo.FileLength;
            this.FileName = fileInfo.FileName;
            this.FilePath = fileInfo.FilePath;
        }
    }
}