//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯服务端主类
    /// </summary>
    public sealed class FileService : TokenTcpService<FileSocketClient>, IFileService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileService()
        {
            this.BufferLength = 1024 * 64;
        }

        #region 属性

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

        /// <summary>
        /// 是否支持断点续传
        /// </summary>
        public bool BreakpointResume { get; set; }

        /// <summary>
        /// 最大下载速度
        /// </summary>
        public long MaxDownloadSpeed { get; set; } = 1024 * 1024;

        /// <summary>
        /// 最大上传速度
        /// </summary>
        public long MaxUploadSpeed { get; set; } = 1024 * 1024;

        #endregion 属性

        #region 字段

        private long downloadSpeed;
        private long uploadSpeed;

        #endregion 字段

        #region 事件

        /// <summary>
        /// 当接收到系统信息的时候
        /// </summary>
        public event RRQMMessageEventHandler ReceiveSystemMes;

        /// <summary>
        /// 刚开始接受文件的时候
        /// </summary>
        public event RRQMTransferFileEventHandler BeforeReceiveFile;

        /// <summary>
        /// 刚开始发送文件的时候
        /// </summary>
        public event RRQMTransferFileEventHandler BeforeSendFile;

        /// <summary>
        /// 当文件发送完
        /// </summary>
        public event RRQMFileFinishedEventHandler SendFileFinished;

        /// <summary>
        /// 当文件接收完成时
        /// </summary>
        public event RRQMFileFinishedEventHandler ReceiveFileFinished;

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        public event RRQMBytesEventHandler ReceivedBytesThenReturn;

        #endregion 事件

        /// <summary>
        /// 创建完成FileSocketClient
        /// </summary>
        /// <param name="tcpSocketClient"></param>
        protected override void OnCreatSocketCliect(FileSocketClient tcpSocketClient)
        {
            tcpSocketClient.breakpointResume = this.BreakpointResume;
            tcpSocketClient.MaxDownloadSpeed = this.MaxDownloadSpeed;
            tcpSocketClient.MaxUploadSpeed = this.MaxUploadSpeed;
            if (tcpSocketClient.NewCreat)
            {
                tcpSocketClient.BeforeReceiveFile += this.BeforeReceiveFile;
                tcpSocketClient.SendFileFinished += this.SendFileFinished;
                tcpSocketClient.BeforeSendFile += this.BeforeSendFile;
                tcpSocketClient.ReceiveSystemMes += this.ReceiveSystemMes;
                tcpSocketClient.ReceiveFileFinished += this.ReceiveFileFinished;
                tcpSocketClient.ReceivedBytesThenReturn += this.ReceivedBytesThenReturn;
            }
        }
    }
}