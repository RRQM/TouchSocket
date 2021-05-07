using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 传输文件操作处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RRQMFileOperationEventHandler(object sender, FileOperationEventArgs e);

    /// <summary>
    /// 传输文件消息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RRQMTransferFileMessageEventHandler(object sender, TransferFileMessageArgs e);
}
