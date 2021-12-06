using RRQMCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件传输状态
    /// </summary>
    public class FileTransferStatusEventArgs : FileTransferEventArgs
    {
        private Result result;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="fileRequest"></param>
        /// <param name="metadata"></param>
        /// <param name="result"></param>
        /// <param name="fileInfo"></param>
        public FileTransferStatusEventArgs(TransferType transferType, FileRequest fileRequest, Metadata metadata, Result result, RRQMFileInfo fileInfo) 
            :base(transferType,fileRequest,metadata,fileInfo)
        {
            this.result = result;
        }

        /// <summary>
        /// 结果
        /// </summary>
        public Result Result
        {
            get { return result; }
        }

    }
}
