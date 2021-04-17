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
    /// 文件信息类
    /// </summary>
    
    public class UrlFileInfo : FileInfo
    {
        /// <summary>
        /// 重新开始
        /// </summary>
        public bool Restart { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public bool Equals(UrlFileInfo fileInfo)
        {
            if (this.FileHash == fileInfo.FileHash)
            {
                return true;
            }
            else if (this.FilePath == fileInfo.FilePath)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}