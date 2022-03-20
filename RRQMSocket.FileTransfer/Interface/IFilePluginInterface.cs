using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件插件接口
    /// </summary>
    public interface IFilePlugin : IPlugin
    {
        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnFileTransfering(IFileClientBase client, FileOperationEventArgs e);

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnFileTransfered(IFileClientBase client, FileTransferStatusEventArgs e);
    }
}
