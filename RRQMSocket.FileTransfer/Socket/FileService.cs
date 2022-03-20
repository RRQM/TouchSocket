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

using RRQMSocket.RPC.RRQMRPC;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 泛型文件服务器
    /// </summary>
    public class FileService<TClient> : TcpRpcParser<TClient> where TClient : FileSocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileService()
        {
            this.AddUsedProtocol(FileUtility.P200, "Client pull file from SocketClient.");
            this.AddUsedProtocol(FileUtility.P201, "Client begin pull file from SocketClient.");
            this.AddUsedProtocol(FileUtility.P202, "Client push file to SocketClient.");
            this.AddUsedProtocol(FileUtility.P203, "SocketClient pull file from client.");
            this.AddUsedProtocol(FileUtility.P204, "SocketClient begin pull file from client.");
            this.AddUsedProtocol(FileUtility.P205, "SocketClient push file to client.");
            for (short i = FileUtility.PMin; i < FileUtility.PMax; i++)
            {
                this.AddUsedProtocol(i, "保留协议");
            }
        }

        #region 字段

        private ResponseType responseType;
        private string rootPath;

        #endregion 字段

        #region 事件

        /// <summary>
        /// 文件传输开始之前
        /// </summary>
        public event RRQMFileOperationEventHandler<TClient> FileTransfering;

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        public event RRQMTransferFileEventHandler<TClient> FileTransfered;

        #endregion 事件

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(RRQMConfig serviceConfig)
        {
            this.responseType = serviceConfig.GetValue<ResponseType>(FileConfigExtensions.ResponseTypeProperty);
            this.rootPath = serviceConfig.GetValue<string>(FileConfigExtensions.RootPathProperty);
            base.LoadConfig(serviceConfig);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(TClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.internalFileTransfering += this.PrivateFileTransfering;
            socketClient.internalFileTransfered += this.PrivateFileTransfered;
            socketClient.ResponseType = this.responseType;
            socketClient.RootPath = this.rootPath;
            base.OnConnecting(socketClient, e);
        }

        private void PrivateFileTransfering(FileSocketClient client, FileOperationEventArgs e)
        {
            this.FileTransfering?.Invoke((TClient)client, e);
        }

        private void PrivateFileTransfered(FileSocketClient client, FileTransferStatusEventArgs e)
        {
            this.FileTransfered?.Invoke((TClient)client, e);
        }
    }

    /// <summary>
    /// 文件服务器
    /// </summary>
    public class FileService : FileService<FileSocketClient>
    {
    }
}