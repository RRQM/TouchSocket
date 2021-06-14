//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using RRQMCore.ByteManager;
using RRQMCore.Serialization;
using RRQMSocket.RPC.RRQMRPC;
using System.Reflection;
using System.Text;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯服务端主类
    /// </summary>
    public class FileService : TcpParser<FileSocketClient>, IFileService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileService()
        {
            this.BufferLength = 1024 * 64;
        }

        #region 属性

        private bool breakpointResume;

        private long maxDownloadSpeed;

        private long maxUploadSpeed;

        /// <summary>
        /// 是否支持断点续传
        /// </summary>
        public bool BreakpointResume
        {
            get { return breakpointResume; }
        }

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
            set { maxUploadSpeed = value; }
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

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        public event RRQMReturnBytesEventHandler ReceivedBytesThenReturn;
        #endregion 事件

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected override void LoadConfig(ServerConfig serverConfig)
        {
            base.LoadConfig(serverConfig);
            this.breakpointResume = (bool)serverConfig.GetValue(FileServiceConfig.BreakpointResumeProperty);
            this.maxDownloadSpeed = (long)serverConfig.GetValue(FileServiceConfig.MaxDownloadSpeedProperty);
            this.maxUploadSpeed = (long)serverConfig.GetValue(FileServiceConfig.MaxUploadSpeedProperty);
        }

        /// <summary>
        /// 创建完成FileSocketClient
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="creatOption"></param>
        protected sealed override void OnCreateSocketCliect(FileSocketClient socketClient, CreateOption creatOption)
        {
            base.OnCreateSocketCliect(socketClient, creatOption);

            socketClient.breakpointResume = this.BreakpointResume;
            socketClient.MaxDownloadSpeed = this.MaxDownloadSpeed;
            socketClient.MaxUploadSpeed = this.MaxUploadSpeed;
            if (creatOption.NewCreate)
            {
                socketClient.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
                socketClient.BeforeFileTransfer = this.OnBeforeFileTransfer;
                socketClient.FinishedFileTransfer = this.OnFinishedFileTransfer;
                socketClient.ReceivedBytesThenReturn = this.OnReceivedBytesThenReturn;
            }
        }

        private void OnBeforeFileTransfer(object sender, FileOperationEventArgs e)
        {
            this.BeforeFileTransfer?.Invoke(sender, e);
        }

        private void OnFinishedFileTransfer(object sender, TransferFileMessageArgs e)
        {
            this.FinishedFileTransfer?.Invoke(sender, e);
        }

        private void OnReceivedBytesThenReturn(object sender, ReturnBytesEventArgs e)
        {
            this.ReceivedBytesThenReturn?.Invoke(sender, e);
        }
    }
}