//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System.IO;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件信息类
    /// </summary>
    public class UrlFileInfo
    {
        private string saveFullPath = string.Empty;

        private int timeout = 30 * 1000;

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash { get;  set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileLength { get;  set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get;  set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 传输标识
        /// </summary>
        public TransferFlags Flags { get; set; }

        /// <summary>
        /// 携带消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 存放目录
        /// </summary>
        public string SaveFullPath
        {
            get { return saveFullPath; }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                saveFullPath = value;
            }
        }

        /// <summary>
        /// 超时时间，默认30*1000 ms
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// 请求传输类型
        /// </summary>
        public TransferType TransferType { get; set; }

        /// <summary>
        /// 生成下载请求必要信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static UrlFileInfo CreateDownload(string path, TransferFlags flags)
        {
            UrlFileInfo fileInfo = new UrlFileInfo();
            fileInfo.FilePath = path;
            fileInfo.Flags = flags;
            fileInfo.FileName = Path.GetFileName(path);
            fileInfo.TransferType = TransferType.Download;
            return fileInfo;
        }

        /// <summary>
        /// 生成上传请求必要信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static UrlFileInfo CreateUpload(string path, TransferFlags flags)
        {
            UrlFileInfo fileInfo = new UrlFileInfo();
            fileInfo.TransferType = TransferType.Upload;
            using (FileStream stream = File.OpenRead(path))
            {
                fileInfo.Flags = flags;
                fileInfo.FilePath = path;
                if (flags.HasFlag(TransferFlags.BreakpointResume) || flags.HasFlag(TransferFlags.QuickTransfer))
                {
                    fileInfo.FileHash = FileHashGenerator.GetFileHash(stream);
                }
                fileInfo.FileLength = stream.Length;
                fileInfo.FileName = Path.GetFileName(path);
            }

            return fileInfo;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="urlFileInfo"></param>
        public void CopyFrom(UrlFileInfo urlFileInfo)
        {
            this.FileHash = urlFileInfo.FileHash;
            this.FileLength = urlFileInfo.FileLength;
            this.FileName = urlFileInfo.FileName;
            this.FilePath = urlFileInfo.FilePath;
        }

        /// <summary>
        /// 判断参数是否相同
        /// </summary>
        /// <param name="urlFileInfo"></param>
        /// <returns></returns>
        public bool Equals(UrlFileInfo urlFileInfo)
        {
            if (urlFileInfo.FileHash != this.FileHash)
            {
                return false;
            }
            if (urlFileInfo.FileLength != this.FileLength)
            {
                return false;
            }
            if (urlFileInfo.FileName != this.FileName)
            {
                return false;
            }
            if (urlFileInfo.FilePath != this.FilePath)
            {
                return false;
            }

            return true;
        }
    }
}