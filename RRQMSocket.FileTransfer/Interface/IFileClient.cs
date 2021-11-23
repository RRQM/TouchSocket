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

using RRQMCore.Exceptions;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件终端接口
    /// </summary>
    public interface IFileClient : IProtocolClient
    {
        /// <summary>
        /// 获取当前传输文件信息
        /// </summary>
        UrlFileInfo TransferFileInfo { get; }

        /// <summary>
        /// 获取当前传输进度
        /// </summary>
        float TransferProgress { get; }

        /// <summary>
        /// 获取当前传输速度
        /// </summary>
        long TransferSpeed { get; }

        /// <summary>
        /// 获取当前传输状态
        /// </summary>
        TransferStatus TransferStatus { get; }

        /// <summary>
        /// 终止当前传输
        /// </summary>
        ///<exception cref="RRQMException"></exception>
        void StopThisTransfer();

        /// <summary>
        /// 终止所有传输
        /// </summary>
        void StopAllTransfer();

        /// <summary>
        /// 暂停传输
        /// </summary>
        void PauseTransfer();

        /// <summary>
        /// 恢复传输
        /// </summary>
        /// <returns>是否有任务成功继续</returns>
        bool ResumeTransfer();
    }
}