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
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using RRQMSocket.RPC.RRQMRPC;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯客户端主类
    /// </summary>
    public class FileClient : TcpRPCClient, IFileClient, IDisposable
    {
        static FileClient()
        {
            AddUsedProtocol(110, "同步设置");
            AddUsedProtocol(111, "通用信道返回");
            AddUsedProtocol(112, "请求下载");
            AddUsedProtocol(113, "下载文件分块");
            AddUsedProtocol(114, "退出下载通道");
            AddUsedProtocol(115, "确认下载完成");
            AddUsedProtocol(116, "请求上传");
            AddUsedProtocol(117, "上传分块");
            AddUsedProtocol(118, "停止上传");
            AddUsedProtocol(119, "确认上传完成");
        }

        /// <summary>
        /// 无参数构造函数
        /// </summary>
        public FileClient()
        {
            waitHandle = new AutoResetEvent(false);
            this.FileTransferCollection = new TransferCollection();
            this.TransferStatus = TransferStatus.None;
            this.FileTransferCollection.OnCollectionChanged += this.FileTransferCollection_OnCollectionChanged;
        }

        #region 自定义

        private bool breakpointResume;

        private RRQMStream downloadStream;

        private ProgressBlockCollection fileBlocks;

        private long position;

        private float progress;

        private string receiveDirectory = string.Empty;

        private long speed;

        private bool stop;

        private long tempLength;

        private Thread thread_Transfer;

        private int timeout = 10;

        private WaitData<ByteBlock> waitDataSend;

        private EventWaitHandle waitHandle;

        private UrlFileInfo transferUrlFileInfo;

        /// <summary>
        /// 传输文件之前
        /// </summary>
        public event RRQMFileOperationEventHandler BeforeFileTransfer;

        /// <summary>
        /// 当文件传输集合更改时
        /// </summary>
        public event RRQMMessageEventHandler FileTransferCollectionChanged;

        /// <summary>
        /// 当文件传输完成时
        /// </summary>
        public event RRQMTransferFileMessageEventHandler FinishedFileTransfer;

        /// <summary>
        /// 传输文件错误
        /// </summary>
        public event RRQMTransferFileMessageEventHandler TransferFileError;

        /// <summary>
        /// 获取支持短点续传状态，
        /// 该属性与服务器同步
        /// </summary>
        public bool BreakpointResume { get { return breakpointResume; } }

        /// <summary>
        /// 获取当前传输文件包
        /// </summary>
        public ProgressBlockCollection FileBlocks { get { return fileBlocks == null ? null : fileBlocks; } }

        /// <summary>
        /// 获取文件传输集合
        /// </summary>
        public TransferCollection FileTransferCollection { get; private set; }

        /// <summary>
        /// 默认接收文件的存放目录
        /// </summary>
        public string ReceiveDirectory
        {
            get { return receiveDirectory; }
        }

        /// <summary>
        /// 单次请求超时时间 min=5,max=60 单位：秒
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set
            {
                timeout = value < 5 ? 5 : (value > 60 ? 60 : value);
            }
        }

        /// <summary>
        /// 获取当前传输文件信息
        /// </summary>
        public FileInfo TransferFileInfo { get { return this.transferUrlFileInfo; } }

        /// <summary>
        /// 获取当前传输进度
        /// </summary>
        public float TransferProgress
        {
            get
            {
                if (fileBlocks == null)
                {
                    return 0;
                }
                if (fileBlocks.UrlFileInfo != null)
                {
                    this.progress = fileBlocks.UrlFileInfo.FileLength > 0 ? (float)position / fileBlocks.UrlFileInfo.FileLength : 0;//计算下载完成进度
                }
                else
                {
                    this.progress = 0;
                }
                return progress <= 1 ? progress : 1;
            }
        }

        /// <summary>
        /// 获取当前传输速度
        /// </summary>
        public long TransferSpeed
        {
            get
            {
                this.speed = tempLength;
                tempLength = 0;
                return speed;
            }
        }

        /// <summary>
        /// 获取当前传输状态
        /// </summary>
        public TransferStatus TransferStatus { get; private set; }

        private void FileTransferCollection_OnCollectionChanged(object sender, MesEventArgs e)
        {
            this.FileTransferCollectionChanged?.Invoke(this, e);
        }

        #endregion 自定义

        /// <summary>
        /// 请求下载文件
        /// </summary>
        /// <param name="urlFileInfo"></param>
        /// <param name="host">IP及端口</param>
        /// <param name="verifyToken">验证令箭</param>
        /// <param name="finishedCallBack">完成时回调</param>
        /// <param name="errorCallBack"></param>
        /// <returns></returns>
        public static FileClient RequestFile
            (UrlFileInfo urlFileInfo, string host, string verifyToken = null,
            RRQMTransferFileMessageEventHandler finishedCallBack = null,
            RRQMTransferFileMessageEventHandler errorCallBack = null)
        {
            FileClient fileClient = new FileClient();
            try
            {
                var config = new FileClientConfig();
                config.SetValue(TcpClientConfig.RemoteIPHostProperty, new IPHost(host))
                    .SetValue(TokenClientConfig.VerifyTokenProperty, verifyToken);

                fileClient.Setup(config);
                fileClient.Connect();

                fileClient.FinishedFileTransfer +=
                    (object sender, TransferFileMessageArgs e) =>
                    {
                        fileClient.Dispose();
                        finishedCallBack?.Invoke(sender, e);
                    };
                fileClient.TransferFileError +=
                    (object sender, TransferFileMessageArgs e) =>
                    {
                        fileClient.Dispose();
                        errorCallBack?.Invoke(sender, e);
                    };
                fileClient.RequestTransfer(urlFileInfo);
                return fileClient;
            }
            catch (Exception ex)
            {
                fileClient.Dispose();
                throw new RRQMException(ex.Message);
            }
        }

        /// <summary>
        /// 取消指定传输任务
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public bool CancelTransfer(UrlFileInfo fileInfo)
        {
            return this.FileTransferCollection.Remove(fileInfo);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="RRQMTimeoutException"></exception>
        public override void Connect()
        {
            base.Connect();
            SynchronizeTransferSetting();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.TransferStatus == TransferStatus.Download)
            {
                this.OutDownload(true);
            }
            else if (this.TransferStatus == TransferStatus.Upload)
            {
                this.OutUpload();
            }
            this.TransferStatus = TransferStatus.None;
            this.progress = 0;
            this.speed = 0;
        }

        /// <summary>
        /// 暂停传输
        /// </summary>
        public void PauseTransfer()
        {
            if (this.TransferStatus == TransferStatus.Download)
            {
                this.TransferStatus = TransferStatus.PauseDownload;
            }
            else if (this.TransferStatus == TransferStatus.Upload)
            {
                this.TransferStatus = TransferStatus.PauseUpload;
            }
        }

        /// <summary>
        /// 请求传输文件
        /// </summary>
        /// <param name="fileInfo"></param>
        public void RequestTransfer(UrlFileInfo fileInfo)
        {
            this.FileTransferCollection.Add(fileInfo);
            BeginTransfer();
        }

        /// <summary>
        /// 恢复传输
        /// </summary>
        /// <returns>是否有任务成功继续</returns>
        public bool ResumeTransfer()
        {
            if (this.TransferStatus == TransferStatus.PauseDownload)
            {
                this.TransferStatus = TransferStatus.Download;
            }
            else if (this.TransferStatus == TransferStatus.PauseUpload)
            {
                this.TransferStatus = TransferStatus.Upload;
            }
            else
            {
                return false;
            }

            return this.waitHandle.Set();
        }

        /// <summary>
        /// 终止所有传输
        /// </summary>
        public void StopAllTransfer()
        {
            this.stop = true;
            if (this.TransferStatus == TransferStatus.Download || this.TransferStatus == TransferStatus.PauseDownload)
            {
                this.StopDownload();
            }
            else if (this.TransferStatus == TransferStatus.Upload || this.TransferStatus == TransferStatus.PauseUpload)
            {
                this.StopUpload();
            }

            this.FileTransferCollection.Clear();
        }

        /// <summary>
        /// 终止当前传输
        /// </summary>
        ///<exception cref="RRQMException"></exception>
        public void StopThisTransfer()
        {
            this.stop = true;
            if (this.TransferStatus == TransferStatus.Download || this.TransferStatus == TransferStatus.PauseDownload)
            {
                this.StopDownload();
            }
            else if (this.TransferStatus == TransferStatus.Upload || this.TransferStatus == TransferStatus.PauseUpload)
            {
                this.StopUpload();
            }

            this.BeginTransfer();
        }

        /// <summary>
        /// 密封方法
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected sealed override void RPCHandleDefaultData(short? procotol, ByteBlock byteBlock)
        {
            switch (procotol)
            {
                case 111:
                    {
                        if (this.waitDataSend != null)
                        {
                            byteBlock.SetHolding(true);
                            this.waitDataSend.Set(byteBlock);
                        }
                        break;
                    }
                default:
                    {
                        FileClientHandleDefaultData(procotol, byteBlock);
                        break;
                    }
            }
        }

        /// <summary>
        /// 文件客户端处理其他协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void FileClientHandleDefaultData(short? procotol, ByteBlock byteBlock)
        {
            this.OnHandleDefaultData(procotol, byteBlock);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            clientConfig.OnlySend = false;
            base.LoadConfig(clientConfig);
            this.SetDataHandlingAdapter(new FixedHeaderDataHandlingAdapter());
            this.receiveDirectory = (string)clientConfig.GetValue(FileClientConfig.ReceiveDirectoryProperty);
        }

        private void BeginTransfer()
        {
            Task.Run(() =>
            {
                lock (locker)
                {
                    if (this.TransferStatus == TransferStatus.None)
                    {
                        if (this.FileTransferCollection.GetFirst(out UrlFileInfo fileInfo))
                        {
                            try
                            {
                                if (fileInfo.TransferType == TransferType.Download)
                                {
                                    this.DownloadFile(fileInfo);
                                }
                                else
                                {
                                    this.UploadFile(fileInfo);
                                }
                                this.transferUrlFileInfo = fileInfo;
                            }
                            catch (Exception ex)
                            {
                                TransferFileMessageArgs args = new TransferFileMessageArgs();
                                args.FileInfo = fileInfo;
                                args.Message = ex.Message;
                                args.TransferType = fileInfo.TransferType;
                                this.TransferFileError?.Invoke(this, args);
                                BeginTransfer();
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 请求下载文件
        /// </summary>
        /// <param name="urlFileInfo"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMTransferingException"></exception>
        /// <exception cref="RRQMException"></exception>
        private void DownloadFile(UrlFileInfo urlFileInfo)
        {
            if (!this.Online)
            {
                throw new RRQMException("未连接服务器");
            }
            else if (this.TransferStatus != TransferStatus.None)
            {
                throw new RRQMTransferingException("已有传输任务在进行中");
            }

            byte[] datas = SerializeConvert.RRQMBinarySerialize(urlFileInfo, true);
            ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
            byteBlock.Write(datas);
            try
            {
                ByteBlock returnByteBlock = this.SendWait(112, urlFileInfo.Timeout, byteBlock);
                if (returnByteBlock == null)
                {
                    throw new RRQMTimeoutException("等待结果超时");
                }
                FileWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<FileWaitResult>(returnByteBlock.Buffer, 2);
                returnByteBlock.SetHolding(false);
                if (waitResult.Status == 2)
                {
                    throw new RRQMTransferErrorException(waitResult.Message);
                }

                StartDownloadFile(waitResult.PBCollectionTemp.ToPBCollection(), urlFileInfo);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void DownloadFileBlock(object o)
        {
            this.stop = false;
            foreach (FileProgressBlock fileBlock in this.fileBlocks)
            {
                if (!fileBlock.Finished)
                {
                    this.position = fileBlock.StreamPosition;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
                        if (!this.Online)
                        {
                            Logger.Debug(LogType.Error, this, $"已断开连接，使用专业版可进行断网续传");
                            OutDownload(false);
                            return;
                        }
                        if (disposable || stop)
                        {
                            OutDownload(true);
                            return;
                        }
                        if (this.TransferStatus == TransferStatus.PauseDownload)
                        {
                            waitHandle.WaitOne();
                        }

                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        byteBlock.Write(BitConverter.GetBytes(this.position));

                        long requestLength = surplusLength > (this.BufferLength - 7) ? (this.BufferLength - 7) : surplusLength;
                        byteBlock.Write(BitConverter.GetBytes(requestLength));

                        try
                        {
                            ByteBlock returnByteBlock = this.SendWait(113, this.timeout, byteBlock);
                            if (returnByteBlock == null || returnByteBlock.Buffer[2] == 2)
                            {
                                reTryCount++;
                                Logger.Debug(LogType.Message, this, $"下载文件错误，正在尝试第{reTryCount}次重试");
                                if (reTryCount > 10)
                                {
                                    this.OutDownload(true);
                                    TransferFileMessageArgs args = new TransferFileMessageArgs();
                                    args.FileInfo = this.TransferFileInfo;
                                    args.TransferType = TransferType.Download;
                                    args.Message = "请求下载文件错误";
                                    this.TransferFileError?.Invoke(this, args);
                                    this.BeginTransfer();
                                    return;
                                }
                            }
                            else if (returnByteBlock.Buffer[2] == 1 || returnByteBlock.Buffer[2] == 3)
                            {
                                reTryCount = 0;

                                while (true)
                                {
                                    if (this.TransferStatus != TransferStatus.Download && this.TransferStatus != TransferStatus.PauseDownload)
                                    {
                                        return;
                                    }
                                    string mes;
                                    if (FileBaseTool.WriteFile(downloadStream, out mes, this.position, returnByteBlock.Buffer, 3, (int)returnByteBlock.Position - 3))
                                    {
                                        tempLength += requestLength;
                                        this.position += requestLength;
                                        surplusLength -= requestLength;
                                        if (returnByteBlock.Buffer[2] == 3)
                                        {
                                            this.SynchronizeTransferSetting();
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        reTryCount++;
                                        Logger.Debug(LogType.Message, this, $"下载文件时，发生写入错误，正在尝试第{reTryCount}次重试");
                                        if (reTryCount > 10)
                                        {
                                            TransferFileMessageArgs args = new TransferFileMessageArgs();
                                            args.FileInfo = this.TransferFileInfo;
                                            args.TransferType = TransferType.Download;
                                            args.Message = "文件写入错误：" + mes;
                                            this.OutDownload(true);
                                            this.TransferFileError?.Invoke(this, args);
                                            this.BeginTransfer();
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TransferFileMessageArgs args = new TransferFileMessageArgs();
                                args.FileInfo = this.TransferFileInfo;
                                args.TransferType = TransferType.Download;
                                args.Message = "服务器无此传输信息，已终止本次传输";
                                OutDownload(false);
                                this.TransferFileError?.Invoke(this, args);
                                this.BeginTransfer();
                                return;
                            }

                            if (returnByteBlock != null)
                            {
                                returnByteBlock.SetHolding(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message);
                        }
                        finally
                        {
                            byteBlock.Dispose();
                        }
                    }
                    fileBlock.Finished = true;
                    FileBaseTool.SaveProgressBlockCollection(downloadStream, this.fileBlocks);
                }
            }

            Task.Run(() => { this.DownloadFileFinished_s(); });
        }

        private void DownloadFileFinished_s()
        {
            try
            {
                ByteBlock resultByteBlock = this.SendWait(115, this.timeout);

                if (resultByteBlock != null)
                {
                    if (resultByteBlock.Length == 3 && resultByteBlock.Buffer[2] == 1)
                    {
                        RRQMStream.FileFinished(downloadStream);
                        FilePathEventArgs args = new FilePathEventArgs();
                        args.FileInfo = this.TransferFileInfo;
                        args.TargetPath = args.FileInfo.FilePath;
                        args.TransferType = TransferType.Download;
                        TransferFileHashDictionary.AddFile(fileBlocks.UrlFileInfo);

                        OutDownload(false);
                        this.FinishedFileTransfer?.Invoke(this, args);
                        this.BeginTransfer();
                    }
                    resultByteBlock.SetHolding(false);
                }
            }
            catch (Exception e)
            {
                TransferFileMessageArgs args = new TransferFileMessageArgs();
                args.FileInfo = this.TransferFileInfo;
                args.TransferType = TransferType.Download;
                args.Message = e.Message;
                this.TransferFileError?.Invoke(this, args);
                this.BeginTransfer();
            }
        }

        private void OutDownload(bool abort)
        {
            if (this.downloadStream != null)
            {
                this.downloadStream.Dispose();
            }
            this.fileBlocks = null;
            this.TransferStatus = TransferStatus.None;
            this.progress = 0;
            this.speed = 0;
            if (abort)
            {
                ByteBlock byteBlock = this.SendWait(114, this.timeout);
                if (byteBlock != null)
                {
                    byteBlock.SetHolding(false);
                }
            }
        }

        private void OutUpload()
        {
            if (this.TransferFileInfo != null)
            {
                TransferFileStreamDic.DisposeFileStream(this.TransferFileInfo.FilePath);
            }
            this.fileBlocks = null;
            this.TransferStatus = TransferStatus.None;
            this.progress = 0;
            this.speed = 0;
        }

        private ByteBlock SendWait(short procotol, int waitTime, ByteBlock byteBlock = null, bool reserved = false)
        {
            lock (locker)
            {
                this.waitDataSend = new WaitData<ByteBlock>();
                try
                {
                    if (byteBlock == null)
                    {
                        this.InternalSend(procotol, new byte[0], 0, 0);
                    }
                    else
                    {
                        this.InternalSend(procotol, byteBlock.Buffer, 0, (int)byteBlock.Length, reserved);
                    }
                }
                catch
                {
                }

                this.waitDataSend.Wait(waitTime * 1000);
                ByteBlock resultByteBlock = this.waitDataSend.WaitResult;
                this.waitDataSend.Dispose();
                this.waitDataSend = null;

                return resultByteBlock;
            }
        }

        private void StartDownloadFile(ProgressBlockCollection blocks, UrlFileInfo urlFileInfo)
        {
            if (this.TransferStatus != TransferStatus.None)
            {
                return;
            }
            this.TransferStatus = TransferStatus.Download;

            FileOperationEventArgs args = new FileOperationEventArgs();
            args.FileInfo = blocks.UrlFileInfo;
            args.TransferType = TransferType.Download;
            if (string.IsNullOrEmpty(urlFileInfo.SaveFolder))
            {
                args.TargetPath = Path.Combine(receiveDirectory, blocks.UrlFileInfo.FileName);
            }
            else
            {
                args.TargetPath = Path.Combine(urlFileInfo.SaveFolder, blocks.UrlFileInfo.FileName);
            }
            args.IsPermitOperation = true;
            this.BeforeFileTransfer?.Invoke(this, args);
            blocks.UrlFileInfo.FilePath = args.TargetPath;

            downloadStream = RRQMStream.GetRRQMStream(ref blocks, urlFileInfo.Restart, this.breakpointResume);
            fileBlocks = blocks;

            this.SynchronizeTransferSetting();
            thread_Transfer = new Thread(this.DownloadFileBlock);
            thread_Transfer.IsBackground = true;
            thread_Transfer.Name = "文件下载线程";
            thread_Transfer.Start();
        }

        private void StartUploadFile(ProgressBlockCollection blocks)
        {
            this.TransferStatus = TransferStatus.Upload;

            FileOperationEventArgs args = new FileOperationEventArgs();
            args.FileInfo = blocks.UrlFileInfo;
            args.TargetPath = blocks.UrlFileInfo.FilePath;
            args.TransferType = TransferType.Upload;
            args.IsPermitOperation = true;
            this.BeforeFileTransfer?.Invoke(this, args);
            blocks.UrlFileInfo.FilePath = args.TargetPath;
            this.fileBlocks = blocks;

            this.SynchronizeTransferSetting();

            thread_Transfer = new Thread(this.UploadFileBlock);
            thread_Transfer.IsBackground = true;
            thread_Transfer.Name = "文件上传线程";
            thread_Transfer.Start();
        }

        private void StopDownload()
        {
            OutDownload(true);
        }

        private void StopUpload()
        {
            try
            {
                ByteBlock byteBlock = this.SendWait(118, this.timeout);
                if (byteBlock != null && byteBlock.Buffer[2] == 1)
                {
                    byteBlock.SetHolding(false);
                    OutUpload();
                }
                else
                {
                    throw new RRQMException("未能成功停止上传，请重试");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SynchronizeTransferSetting()
        {
            ByteBlock byteBlock = this.SendWait(110, this.timeout);
            if (byteBlock == null)
            {
                return;
            }
            TransferSetting transferSetting = TransferSetting.Deserialize(byteBlock.Buffer, 2);
            this.breakpointResume = transferSetting.breakpointResume;
            this.SetBufferLength(transferSetting.bufferLength);
            byteBlock.SetHolding(false);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="urlFileInfo"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMTransferingException"></exception>
        /// <exception cref="RRQMException"></exception>
        private void UploadFile(UrlFileInfo urlFileInfo)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("未连接到服务器");
            }
            else if (this.TransferStatus != TransferStatus.None)
            {
                throw new RRQMTransferingException("已有传输任务在进行中");
            }
            ProgressBlockCollection requsetBlocks = FileBaseTool.GetProgressBlockCollection(urlFileInfo, this.breakpointResume);
            byte[] datas = SerializeConvert.RRQMBinarySerialize(PBCollectionTemp.GetFromProgressBlockCollection(requsetBlocks), true);
            ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
            byteBlock.Write(BitConverter.GetBytes(urlFileInfo.Restart));
            byteBlock.Write(datas);

            try
            {
                ByteBlock resultByteBlock = this.SendWait(116, this.timeout, byteBlock);
                if (resultByteBlock == null)
                {
                    throw new RRQMTimeoutException("等待结果超时");
                }
                FileWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<FileWaitResult>(resultByteBlock.Buffer, 2);
                resultByteBlock.SetHolding(false);
                if (waitResult.Status == 0)
                {
                    throw new RRQMTimeoutException("等待结果超时");
                }
                else if (waitResult.Status == 2)
                {
                    throw new RRQMTransferErrorException(waitResult.Message);
                }
                else if (waitResult.Status == 3)
                {
                    this.TransferStatus = TransferStatus.Upload;

                    FileOperationEventArgs args = new FileOperationEventArgs();
                    args.FileInfo = requsetBlocks.UrlFileInfo;
                    args.TransferType = TransferType.Upload;
                    args.TargetPath = requsetBlocks.UrlFileInfo.FilePath;
                    args.IsPermitOperation = true;
                    this.BeforeFileTransfer?.Invoke(this, args);

                    requsetBlocks.UrlFileInfo.FilePath = args.TargetPath;
                    this.fileBlocks = requsetBlocks;

                    Task.Run(() =>
                    {
                        UploadFileFinished_s();
                    });
                }
                else
                {
                    ProgressBlockCollection blocks = waitResult.PBCollectionTemp.ToPBCollection();
                    blocks.UrlFileInfo.FilePath = urlFileInfo.FilePath;
                    this.StartUploadFile(blocks);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void UploadFileBlock()
        {
            this.stop = false;

            foreach (FileProgressBlock fileBlock in this.fileBlocks)
            {
                if (!fileBlock.Finished)
                {
                    this.position = fileBlock.StreamPosition;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
                        if (!this.Online)
                        {
                            Logger.Debug(LogType.Error, this, $"已断开连接，使用专业版可进行断网续传");
                            OutUpload();
                            return;
                        }
                        if (disposable || stop)
                        {
                            OutUpload();
                            return;
                        }
                        if (this.TransferStatus == TransferStatus.PauseUpload)
                        {
                            waitHandle.WaitOne();
                        }
                        byte[] positionBytes = BitConverter.GetBytes(this.position);
                        long submitLength = surplusLength > (this.BufferLength - 27) ? (this.BufferLength - 27) : surplusLength;
                        byte[] submitLengthBytes = BitConverter.GetBytes(submitLength);
                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        byteBlock.Position = 6;
                        if (this.position + submitLength == fileBlock.StreamPosition + fileBlock.UnitLength)
                        {
                            byteBlock.Write(1);
                        }
                        else
                        {
                            byteBlock.Write(0);
                        }

                        byteBlock.Write(BitConverter.GetBytes(fileBlock.Index));
                        byteBlock.Write(positionBytes);
                        byteBlock.Write(submitLengthBytes);
                        try
                        {
                            if (this.TransferStatus != TransferStatus.Upload && this.TransferStatus != TransferStatus.PauseUpload)
                            {
                                return;
                            }
                            if (FileBaseTool.ReadFileBytes(this.fileBlocks.UrlFileInfo.FilePath, this.position, byteBlock, 27, (int)submitLength))
                            {
                                ByteBlock returnByteBlock = this.SendWait(117, this.timeout, byteBlock, true);
                                if (returnByteBlock != null && returnByteBlock.Length == 3)
                                {
                                    if (returnByteBlock.Buffer[2] == 1)
                                    {
                                        reTryCount = 0;
                                        this.tempLength += submitLength;
                                        this.position += submitLength;
                                        surplusLength -= submitLength;
                                    }
                                    else if (returnByteBlock.Buffer[2] == 2)
                                    {
                                        reTryCount++;
                                        Logger.Debug(LogType.Message, this, $"上传文件错误，正在尝试第{reTryCount}次重试");
                                        if (reTryCount > 10)
                                        {
                                            TransferFileMessageArgs args = new TransferFileMessageArgs();
                                            args.FileInfo = this.fileBlocks.UrlFileInfo;
                                            args.TransferType = TransferType.Upload;
                                            args.Message = "上传文件错误";
                                            this.TransferFileError?.Invoke(this, args);
                                            this.BeginTransfer();
                                            return;
                                        }
                                    }
                                    else if (returnByteBlock.Buffer[2] == 3)
                                    {
                                        reTryCount = 0;
                                        this.tempLength += submitLength;
                                        this.position += submitLength;
                                        surplusLength -= submitLength;
                                        this.SynchronizeTransferSetting();
                                    }
                                    else
                                    {
                                        TransferFileMessageArgs args = new TransferFileMessageArgs();
                                        args.FileInfo = this.fileBlocks.UrlFileInfo;
                                        args.TransferType = TransferType.Upload;
                                        args.Message = "服务器无此传输信息，已终止本次传输";
                                        OutUpload();
                                        this.TransferFileError?.Invoke(this, args);
                                        this.BeginTransfer();
                                        return;
                                    }
                                }
                                else
                                {
                                    reTryCount++;
                                    Logger.Debug(LogType.Message, this, $"上传文件错误，正在尝试第{reTryCount}次重试");
                                    if (reTryCount > 10)
                                    {
                                        TransferFileMessageArgs args = new TransferFileMessageArgs();
                                        args.FileInfo = this.fileBlocks.UrlFileInfo;
                                        args.TransferType = TransferType.Upload;
                                        args.Message = "上传文件错误";
                                        this.TransferFileError?.Invoke(this, args);
                                        this.BeginTransfer();
                                        return;
                                    }
                                }

                                if (returnByteBlock != null)
                                {
                                    returnByteBlock.SetHolding(false);
                                }
                            }
                            else
                            {
                                reTryCount++;
                                Logger.Debug(LogType.Message, this, $"读取上传文件错误，正在尝试第{reTryCount}次重试");
                                if (reTryCount > 10)
                                {
                                    TransferFileMessageArgs args = new TransferFileMessageArgs();
                                    args.FileInfo = this.fileBlocks.UrlFileInfo;
                                    args.TransferType = TransferType.Upload;
                                    args.Message = "读取上传文件错误";
                                    this.TransferFileError?.Invoke(this, args);
                                    this.BeginTransfer();
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message);
                        }
                        finally
                        {
                            byteBlock.Dispose();
                        }
                    }
                }
            }

            Task.Run(() =>
            {
                UploadFileFinished_s();
            });
        }

        private void UploadFileFinished_s()
        {
            int reTryCount = 0;
            while (reTryCount < 10)
            {
                ByteBlock byteBlock = this.SendWait(119, this.timeout);
                if (byteBlock != null)
                {
                    if (byteBlock.Length == 3 && byteBlock.Buffer[2] == 1)
                    {
                        FilePathEventArgs args = new FilePathEventArgs();
                        args.FileInfo = fileBlocks.UrlFileInfo;
                        args.TargetPath = args.FileInfo.FilePath;
                        args.TransferType = TransferType.Upload;

                        OutUpload();
                        this.FinishedFileTransfer?.Invoke(this, args);
                        this.BeginTransfer();
                        break;
                    }
                    byteBlock.SetHolding(false);
                }

                reTryCount++;
            }
        }
    }
}