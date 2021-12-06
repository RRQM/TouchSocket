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
            this.metadata = metadata;
            this.fileRequest = fileRequest;
            this.transferType = transferType;
            this.fileInfo = fileInfo;
        }

        private FileRequest fileRequest;

        /// <summary>
        /// 文件请求
        /// </summary>
        public FileRequest FileRequest
        {
            get { return fileRequest; }
        }

        /// <summary>
        /// 传输类型
        /// </summary>
        public TransferType TransferType
        {
            get { return transferType; }
        }

        private Metadata metadata;
        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata
        {
            get { return metadata; }
        }

        private RRQMFileInfo fileInfo;
        /// <summary>
        /// 文件信息
        /// </summary>
        public RRQMFileInfo FileInfo
        {
            get { return fileInfo; }
        }

    }
}