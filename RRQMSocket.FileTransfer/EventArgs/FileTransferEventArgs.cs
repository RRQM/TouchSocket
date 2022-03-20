//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件传输事件
    /// </summary>
    public class FileTransferEventArgs : MesEventArgs
    {
        private TransferType transferType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="fileRequest"></param>
        /// <param name="metadata"></param>
        /// <param name="fileInfo"></param>
        public FileTransferEventArgs(TransferType transferType, FileRequest fileRequest, Metadata metadata, RRQMFileInfo fileInfo)
        {
            this.IsPermitOperation = true;
            this.metadata = metadata;
            this.fileRequest = fileRequest;
            this.transferType = transferType;
            this.fileInfo = fileInfo;
        }

        private FileRequest fileRequest;

        /// <summary>
        /// 文件请求
        /// </summary>
        public FileRequest FileRequest => this.fileRequest;

        /// <summary>
        /// 传输类型
        /// </summary>
        public TransferType TransferType => this.transferType;

        private Metadata metadata;

        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata => this.metadata;

        private RRQMFileInfo fileInfo;

        /// <summary>
        /// 文件信息
        /// </summary>
        public RRQMFileInfo FileInfo => this.fileInfo;
    }
}