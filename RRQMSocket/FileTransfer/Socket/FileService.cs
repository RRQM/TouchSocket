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
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Serialization;
using System;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯服务端主类
    /// </summary>
    public sealed class FileService : TcpService<FileSocketClient>
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

        private bool isFsatUpload;

        /// <summary>
        /// 是否支持快速上传
        /// </summary>
        public bool IsFsatUpload
        {
            get { return isFsatUpload; }
            set
            {
                isFsatUpload = value;
                if (isFsatUpload)
                {
                    TransferFileHashDictionary.Initialization();
                }
                else
                {
                    TransferFileHashDictionary.UnInitialization();
                }
            }
        }

        #endregion 属性

        #region 字段

        private long downloadSpeed;
        private long uploadSpeed;
        #endregion 字段

        #region 事件

        /// <summary>
        /// 当接收到系统信息的时候
        /// </summary>
        public event RRQMShowMesEventHandler ReceiveSystemMes;

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

        #region 公用方法

        #endregion 公用方法

        /// <summary>
        /// 将连接进来的用户进行储存
        /// </summary>
        protected override FileSocketClient CreatSocketCliect()
        {
            FileSocketClient socketCliect = new FileSocketClient(this.BytePool);
            socketCliect.breakpointResume = this.BreakpointResume;
            socketCliect.BeforeReceiveFile += this.BeforeReceiveFile;
            socketCliect.SendFileFinished += this.SendFileFinished;
            socketCliect.BeforeSendFile += this.BeforeSendFile;
            socketCliect.ReceiveSystemMes += this.ReceiveSystemMes;
            socketCliect.ReceiveFileFinished += this.ReceiveFileFinished;
            socketCliect.ReceivedBytesThenReturn += this.ReceivedBytesThenReturn;
            socketCliect.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
            return socketCliect;
        }
    }
}