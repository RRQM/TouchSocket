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

using RRQMSocket.RPC.RRQMRPC;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯服务端主类
    /// </summary>
    public class FileService : TcpParser<FileSocketClient>
    {
        #region 属性


        #endregion 属性

        #region 字段
        private ResponseType responseType;
        private string rootPath;
        #endregion 字段

        #region 事件

        /// <summary>
        /// 文件传输开始之前
        /// </summary>
        public event RRQMFileOperationEventHandler<FileSocketClient> BeforeFileTransfer;

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        public event RRQMTransferFileEventHandler<FileSocketClient> FinishedFileTransfer;

        #endregion 事件

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(ServiceConfig serviceConfig)
        {
            this.responseType = serviceConfig.GetValue<ResponseType>(FileServiceConfig.ResponseTypeProperty);
            this.rootPath = serviceConfig.GetValue<string>(FileServiceConfig.RootPathProperty);
            base.LoadConfig(serviceConfig);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(FileSocketClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.BeforeFileTransfer += this.OnBeforeFileTransfer;
            socketClient.FinishedFileTransfer += this.OnFinishedFileTransfer;
            socketClient.ResponseType = this.responseType;
            socketClient.RootPath = this.rootPath;
            base.OnConnecting(socketClient, e);
        }

        private void OnBeforeFileTransfer(FileSocketClient client, FileOperationEventArgs e)
        {
            this.BeforeFileTransfer?.Invoke(client, e);
        }

        private void OnFinishedFileTransfer(FileSocketClient client, FileTransferStatusEventArgs e)
        {
            this.FinishedFileTransfer?.Invoke(client, e);
        }
    }
}