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
using System.IO;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 请求下载参数
    /// </summary>
    [Serializable]
    public class FileUrl
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public FileUrl(string filePath)
        {
            this.FilePath = filePath;
        }

        /// <summary>
        ///
        /// </summary>
        public FileUrl()
        {
        }

        /// <summary>
        /// 下载文件名
        /// </summary>
        public string FileName { get { return fileName; } }

        /// <summary>
        /// 是否重新开始
        /// </summary>
        public bool Restart { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string FilePath
        {
            get
            { return filePath; }
            set
            {
                this.fileName = Path.GetFileName(value);
                this.filePath = value;
            }
        }

        /// <summary>
        /// 标志
        /// </summary>
        public string Flag { get; set; }

        private string filePath;
        private string fileName;

        /// <summary>
        /// 判断
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected bool Equals(FileUrl url)
        {
            if (this.FilePath == url.FilePath)
            {
                return true;
            }
            return false;
        }
    }
}