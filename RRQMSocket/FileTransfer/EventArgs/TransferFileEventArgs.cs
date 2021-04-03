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
namespace RRQMSocket.FileTransfer
{
    /*
    若汝棋茗
    */

    /// <summary>
    ///
    /// </summary>
    public class TransferFileEventArgs : FileEventArgs
    {
        /// <summary>
        /// 是否允许传输
        /// </summary>
        public bool IsPermitTransfer { get; set; }

        /// <summary>
        /// 获取或设置目标文件的最终路径，包含文件名及文件扩展名
        /// </summary>
        public string TargetPath { get; set; }
    }
}