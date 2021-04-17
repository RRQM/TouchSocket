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
    ///
    /// </summary>
    
    public class SubmitFileData
    {
        /// <summary>
        /// 文件流位置
        /// </summary>
        public long SubmitStreamPosition { get; set; }

        /// <summary>
        /// 请求长度
        /// </summary>
        public long SubmitLength { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] DataBuffer { get; set; }

        /// <summary>
        /// 文件块完成
        /// </summary>
        public bool BlockFinished { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash { get; set; }
    }
}