using RRQMSocket.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer.Plugins
{
    /// <summary>
    /// 文件插件基类
    /// </summary>
    public class FilePluginBase : ProtocolPluginBase, IFilePlugin
    {
        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfered(IFileClientBase client, FileTransferStatusEventArgs e)
        {

        }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfering(IFileClientBase client, FileOperationEventArgs e)
        {
            
        }

        void IFilePlugin.OnFileTransfered(IFileClientBase client, FileTransferStatusEventArgs e)
        {
            this.OnFileTransfered(client, e);
        }

        void IFilePlugin.OnFileTransfering(IFileClientBase client, FileOperationEventArgs e)
        {
            this.OnFileTransfering(client,e);
        }
    }
}
