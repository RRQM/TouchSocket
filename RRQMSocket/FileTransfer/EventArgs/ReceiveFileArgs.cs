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
    /// 传输文件
    /// </summary>
    public class TransferFileArgs
    {
        /// <summary>
        /// 已接收的流位置
        /// </summary>
        public long StreamPosition { get; set; }

        /// <summary>
        /// 接收的文件信息（不要手动更改里面任何内容）
        /// </summary>
        public FileInfo FileInfo { get; set; }

        /// <summary>
        /// 传输文件进度
        /// </summary>
        public float TransferProgressValue { get; set; }
    }
}