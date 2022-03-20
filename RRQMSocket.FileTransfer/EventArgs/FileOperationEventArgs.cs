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
    /// 操作文件事件类
    /// </summary>
    public class FileOperationEventArgs : FileTransferEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <param name="fileInfo"></param>
        public FileOperationEventArgs(TransferType transferType, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata, RRQMFileInfo fileInfo)
            : base(transferType, fileRequest, metadata, fileInfo)
        {
            this.isPermitOperation = true;
            this.fileOperator = fileOperator;
        }

        private bool isPermitOperation;

        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool IsPermitOperation
        {
            get { return this.isPermitOperation; }
            set { this.isPermitOperation = value; }
        }

        private FileOperator fileOperator;

        /// <summary>
        /// 文件操作器
        /// </summary>
        public FileOperator FileOperator
        {
            get { return this.fileOperator; }
        }
    }
}