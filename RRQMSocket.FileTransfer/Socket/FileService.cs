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
    public class FileService : TcpParser<FileSocketClient>, IFileService
    {
        #region 属性

        private long maxDownloadSpeed;

        private long maxUploadSpeed;

        /// <summary>
        /// 获取下载速度
        /// </summary>
        public long DownloadSpeed
        {
            get
            {
                this.downloadSpeed = Speed.downloadSpeed;
                Speed.downloadSpeed = 0;
                return this.downloadSpeed;
            }
        }

        /// <summary>
        /// 最大下载速度
        /// </summary>
        public long MaxDownloadSpeed
        {
            get { return maxDownloadSpeed; }
            set { maxDownloadSpeed = value; }
        }

        /// <summary>
        /// 最大上传速度
        /// </summary>
        public long MaxUploadSpeed
        {
            get { return maxUploadSpeed; }
            set { maxUploadSpeed = value; }
        }

        /// <summary>
        /// 获取上传速度
        /// </summary>
        public long UploadSpeed
        {
            get
            {
                this.uploadSpeed = Speed.uploadSpeed;
                Speed.uploadSpeed = 0;
                return this.uploadSpeed;
            }
        }

        #endregion 属性

        #region 字段

        private long downloadSpeed;
        private long uploadSpeed;

        #endregion 字段

        #region 事件

        /// <summary>
        /// 传输文件之前
        /// </summary>
        public event RRQMFileOperationEventHandler BeforeFileTransfer;

        /// <summary>
        /// 当文件传输完成时
        /// </summary>
        public event RRQMTransferFileMessageEventHandler FinishedFileTransfer;

        #endregion 事件

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(ServiceConfig serviceConfig)
        {
            base.LoadConfig(serviceConfig);
            this.maxDownloadSpeed = (long)serviceConfig.GetValue(FileServiceConfig.MaxDownloadSpeedProperty);
            this.maxUploadSpeed = (long)serviceConfig.GetValue(FileServiceConfig.MaxUploadSpeedProperty);
        }

        /// <summary>
        /// 创建完成FileSocketClient
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="creatOption"></param>
        protected override void OnCreateSocketClient(FileSocketClient socketClient, CreateOption creatOption)
        {
            base.OnCreateSocketClient(socketClient, creatOption);
            socketClient.MaxDownloadSpeed = this.MaxDownloadSpeed;
            socketClient.MaxUploadSpeed = this.MaxUploadSpeed;
            socketClient.downloadRoot = this.ServiceConfig.GetValue<string>(FileServiceConfig.DownloadRootProperty);
            socketClient.uploadRoot = this.ServiceConfig.GetValue<string>(FileServiceConfig.UploadRootProperty);
            socketClient.BeforeFileTransfer += this.OnBeforeFileTransfer;
            socketClient.FinishedFileTransfer += this.OnFinishedFileTransfer;
        }

        private void OnBeforeFileTransfer(IFileClient client, FileOperationEventArgs e)
        {
            this.BeforeFileTransfer?.Invoke(client, e);
        }

        private void OnFinishedFileTransfer(IFileClient client, TransferFileMessageArgs e)
        {
            this.FinishedFileTransfer?.Invoke(client, e);
        }
    }
}