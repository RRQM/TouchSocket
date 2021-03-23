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
    /// 服务器接口
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 最大下载速度
        /// </summary>
        long MaxDownloadSpeed { get; set; }

        /// <summary>
        /// 最大上传速度
        /// </summary>
        long MaxUploadSpeed { get; set; }
    }
}