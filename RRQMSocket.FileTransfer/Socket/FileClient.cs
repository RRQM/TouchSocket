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
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using RRQMSocket.RPC.RRQMRPC;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯客户端主类
    /// </summary>
    public class FileClient : TcpRpcClient, IFileClient, IDisposable
    {
        private ProgressBlockCollection fileBlocks;

        private TransferCollection fileTransferCollection;

        private int packetSize = 1024 * 64;

        private long position;

        private float progress;

        private string receiveDirectory = string.Empty;

        private string rrqmPath;

        private long speed;

        private bool stop;

        private long tempLength;

        private int timeout = 60 * 1000;

        private TransferStatus transferStatus;

        private UrlFileInfo transferUrlFileInfo;

        private EventWaitHandle transferWaitHandle;

        private WaitData<ByteBlock> waitDataSend;

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
            AddUsedProtocol(120, "智能包调节");

            AddUsedProtocol(121, "服务器暂停当前任务");
            AddUsedProtocol(122, "服务器恢复当前任务");
            AddUsedProtocol(123, "服务器终止当前任务");
            AddUsedProtocol(124, "服务器终止所有任务");
            for (short i = 125; i < 200; i++)
            {
                AddUsedProtocol(i, "保留协议");
            }
        }

        /// <summary>
        /// 无参数构造函数
        /// </summary>
        public FileClient()
        {
            this.waitDataSend = new WaitData<ByteBlock>();
            this.transferWaitHandle = new AutoResetEvent(false);
            this.fileTransferCollection = new TransferCollection();
            this.transferStatus = TransferStatus.None;
            this.fileTransferCollection.OnCollectionChanged += this.FileTransferCollection_OnCollectionChanged;
        }

        /// <summary>
        /// 传输文件之前
        /// </summary>
        public event RRQMFileOperationEventHandler BeforeFileTransfer;

        /// <summary>
        /// 当文件传输集合更改时
        /// </summary>
        public event RRQMMessageEventHandler<FileClient> FileTransferCollectionChanged;

        /// <summary>
        /// 当文件传输完成时
        /// </summary>
        public event RRQMTransferFileMessageEventHandler FinishedFileTransfer;

        /// <summary>
        /// 传输文件错误
        /// </summary>
        public event RRQMTransferFileMessageEventHandler TransferFileError;

        /// <summary>
        /// 获取当前传输文件包
        /// </summary>
        public ProgressBlockCollection FileBlocks { get { return fileBlocks == null ? null : fileBlocks; } }

        /// <summary>
        /// 文件传输队列集合
        /// </summary>
        public TransferCollection FileTransferCollection
        {
            get { return fileTransferCollection; }
        }

        /// <summary>
        /// 默认接收文件的存放目录
        /// </summary>
        public string ReceiveDirectory
        {
            get { return receiveDirectory; }
        }

        /// <summary>
        /// 单次请求超时时间 min=5000,max=60*1000 ms
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
        }

        /// <summary>
        /// 获取当前传输文件信息
        /// </summary>
        public UrlFileInfo TransferFileInfo { get { return this.transferUrlFileInfo; } }

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
        public TransferStatus TransferStatus
        {
            get { return transferStatus; }
        }

        /// <summary>
        /// 请求下载文件
        /// </summary>
        /// <param name="urlFileInfo"></param>
        /// <param name="host">IP及端口</param>
        /// <param name="verifyToken">验证令箭</param>
        /// <param name="finishedCallBack">完成时回调</param>
        /// <param name="errorCallBack"></param>
        /// <returns></returns>
        public static FileClient RequestFile(UrlFileInfo urlFileInfo, string host, string verifyToken = null,
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
                    (IFileClient client, TransferFileMessageArgs e) =>
                    {
                        fileClient.Dispose();
                        finishedCallBack?.Invoke(client, e);
                    };
                fileClient.TransferFileError +=
                    (IFileClient client, TransferFileMessageArgs e) =>
                    {
                        fileClient.Dispose();
                        errorCallBack?.Invoke(client, e);
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
            return this.fileTransferCollection.Remove(fileInfo);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public override ITcpClient Disconnect()
        {
            base.Disconnect();
            this.ResetVariable();
            return this;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.transferStatus == TransferStatus.Download)
            {
                this.StopDownload();
            }
            else if (this.transferStatus == TransferStatus.Upload)
            {
                this.StopUpload();
            }
            this.waitDataSend.Dispose();
            this.transferStatus = TransferStatus.None;
            this.progress = 0;
            this.speed = 0;
        }

        /// <summary>
        /// 暂停传输
        /// </summary>
        public void PauseTransfer()
        {
            if (this.transferStatus == TransferStatus.Download)
            {
                this.transferStatus = TransferStatus.PauseDownload;
            }
            else if (this.transferStatus == TransferStatus.Upload)
            {
                this.transferStatus = TransferStatus.PauseUpload;
            }
        }

        /// <summary>
        /// 请求传输文件
        /// </summary>
        /// <param name="fileInfo"></param>
        public void RequestTransfer(UrlFileInfo fileInfo)
        {
            this.fileTransferCollection.Add(fileInfo);
            BeginTransfer();
        }

        /// <summary>
        /// 恢复传输
        /// </summary>
        /// <returns>是否有任务成功继续</returns>
        public bool ResumeTransfer()
        {
            if (this.transferStatus == TransferStatus.PauseDownload)
            {
                this.transferStatus = TransferStatus.Download;
            }
            else if (this.transferStatus == TransferStatus.PauseUpload)
            {
                this.transferStatus = TransferStatus.Upload;
            }
            else
            {
                return false;
            }

            return this.transferWaitHandle.Set();
        }

        /// <summary>
        /// 终止所有传输
        /// </summary>
        public void StopAllTransfer()
        {
            this.stop = true;
            this.fileTransferCollection.Clear();
        }

        /// <summary>
        /// 终止当前传输
        /// </summary>
        ///<exception cref="RRQMException"></exception>
        public void StopThisTransfer()
        {
            this.stop = true;
            this.BeginTransfer();
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
            base.LoadConfig(clientConfig);
            this.receiveDirectory = (string)clientConfig.GetValue(FileClientConfig.ReceiveDirectoryProperty);
            this.timeout = (int)clientConfig.GetValue(FileClientConfig.TimeoutProperty);
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
                        byteBlock.SetHolding(true);
                        this.waitDataSend.Set(byteBlock);
                        break;
                    }

                case 120:
                    {
                        byteBlock.Pos = 2;
                        this.packetSize = byteBlock.ReadInt32();
                        break;
                    }
                case 121:
                    {
                        this.PauseTransfer();
                        break;
                    }
                case 122:
                    {
                        this.ResumeTransfer();
                        break;
                    }
                case 123:
                    {
                        this.StopThisTransfer();
                        break;
                    }
                case 124:
                    {
                        this.StopAllTransfer();
                        break;
                    }
                default:
                    {
                        FileClientHandleDefaultData(procotol, byteBlock);
                        break;
                    }
            }
        }

        private void BeginTransfer()
        {
            Task.Run(() =>
            {
                lock (this)
                {
                    if (this.transferStatus == TransferStatus.None)
                    {
                        if (this.fileTransferCollection.GetFirst(out UrlFileInfo urlFileInfo))
                        {
                            try
                            {
                                if (urlFileInfo.TransferType == TransferType.Download)
                                {
                                    this.DownloadFile(urlFileInfo, true);
                                }
                                else
                                {
                                    this.UploadFile(urlFileInfo, true);
                                }
                            }
                            catch (Exception ex)
                            {
                                this.OnTransferError(urlFileInfo.TransferType, ex.Message, urlFileInfo);
                            }
                        }
                    }
                }
            });
        }

        private void DownloadFile(UrlFileInfo urlFileInfo, bool triggerEvent)
        {
            if (!this.Online)
            {
                throw new RRQMException("未连接服务器");
            }
            else if (this.transferStatus != TransferStatus.None)
            {
                throw new RRQMTransferingException("已有传输任务在进行中");
            }

            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);

            try
            {
                SerializeConvert.RRQMBinarySerialize(byteBlock, urlFileInfo, true);
                ByteBlock returnByteBlock = this.SingleSendWait(112, urlFileInfo.Timeout, byteBlock);
                FileWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<FileWaitResult>(returnByteBlock.Buffer, 2);
                returnByteBlock.SetHolding(false);
                if (waitResult.Status == 2)
                {
                    throw new RRQMTransferErrorException(waitResult.Message);
                }
                urlFileInfo = waitResult.PBCollectionTemp.UrlFileInfo;

                ProgressBlockCollection blocks = ProgressBlockCollection.CreateProgressBlockCollection(urlFileInfo);
                if (triggerEvent)
                {
                    this.OnBeforeFileTransfer(blocks);
                }
                if (!FileStreamPool.LoadWriteStream(ref blocks, !triggerEvent, out string mes))
                {
                    this.OnTransferError(urlFileInfo.TransferType, mes, urlFileInfo);
                    return;
                }

                this.transferStatus = TransferStatus.Download;
                this.fileBlocks = blocks;
                this.transferUrlFileInfo = blocks.UrlFileInfo;
                this.rrqmPath = blocks.UrlFileInfo.SaveFullPath + ".rrqm";
                Thread thread_Transfer = new Thread(this.DownloadFileBlock);
                thread_Transfer.IsBackground = true;
                thread_Transfer.Name = "文件下载线程";
                thread_Transfer.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void DownloadFileBlock()
        {
            this.stop = false;
            while (true)
            {
                if (FileStreamPool.GetFreeFileBlock(this.rrqmPath, out FileBlock fileBlock, out string errorMes))
                {
                    if (fileBlock == null)
                    {
                        break;
                    }
                    this.position = fileBlock.Position;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
                        if (!this.Online)
                        {
                            fileBlock.RequestStatus = RequestStatus.Hovering;
                            this.OnTransferError(TransferType.Download, "客户端已断开连接！！！");
                            return;
                        }
                        if (disposable || stop)
                        {
                            fileBlock.RequestStatus = RequestStatus.Hovering;
                            StopDownload();
                            return;
                        }
                        if (this.transferStatus == TransferStatus.PauseDownload)
                        {
                            transferWaitHandle.WaitOne();
                        }

                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        byteBlock.Write(this.position);
                        int requestLength = surplusLength > this.packetSize ? this.packetSize : (int)surplusLength;
                        byteBlock.Write(requestLength);

                        try
                        {
                            ByteBlock returnByteBlock = this.SingleSendWait(113, this.timeout, byteBlock);
                            if (returnByteBlock.Buffer[2] == 1)
                            {
                                if (this.transferStatus != TransferStatus.Download && this.transferStatus != TransferStatus.PauseDownload)
                                {
                                    fileBlock.RequestStatus = RequestStatus.Hovering;
                                    return;
                                }
                                if (FileStreamPool.WriteFile(this.rrqmPath, out string mes, out RRQMStream _, this.position, returnByteBlock.Buffer, 3, returnByteBlock.Len - 3))
                                {
                                    tempLength += requestLength;
                                    this.position += requestLength;
                                    surplusLength -= requestLength;
                                    reTryCount = 0;
                                }
                                else
                                {
                                    throw new RRQMException($"文件写入错误，信息：{mes}");
                                }
                            }
                            else if (returnByteBlock.Buffer[2] == 2)
                            {
                                string mes = Encoding.UTF8.GetString(returnByteBlock.Buffer, 3, returnByteBlock.Len - 3);
                                throw new RRQMException($"文件请求错误，信息：{mes}");
                            }
                            returnByteBlock.SetHolding(false);
                        }
                        catch (Exception ex)
                        {
                            reTryCount++;
                            Logger.Debug(LogType.Message, this, $"下载文件错误，信息：{ex.Message}，即将进行第{reTryCount}次重试");

                            if (reTryCount > 10)
                            {
                                fileBlock.RequestStatus = RequestStatus.Hovering;
                                this.OnTransferError(TransferType.Download, "重试次数达到最大，详细信息请查看日志");
                                return;
                            }
                        }
                        finally
                        {
                            byteBlock.Dispose();
                        }
                    }
                    fileBlock.RequestStatus = RequestStatus.Finished;
                    if (this.transferUrlFileInfo.Flags.HasFlag(TransferFlags.BreakpointResume))
                    {
                        try
                        {
                            FileStreamPool.SaveProgressBlockCollection(this.rrqmPath);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "保存进度文件错误", ex);
                        }
                    }
                }
                else
                {
                    this.Logger.Debug(LogType.Error, this, $"获取文件块错误，信息：{errorMes}");
                }
            }
            if (FileStreamPool.CheckAllFileBlockFinished(this.rrqmPath))
            {
                this.DownloadFinished();
            }
        }

        private void DownloadFinished()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    ByteBlock returnByteBlock = this.SingleSendWait(115, this.timeout);

                    if (returnByteBlock.Buffer[2] == 1)
                    {
                        OnDownloadFileFinished();
                        return;
                    }
                    else
                    {
                        this.Logger.Debug(LogType.Warning, this, $"确认下载完成返回状态错误，即将进行第{i + 1}次重试");
                    }
                    returnByteBlock.SetHolding(false);
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(LogType.Warning, this, $"确认下载完成错误，信息：{ex.Message}，即将进行第{i + 1}次重试");
                }
            }

            this.OnTransferError(TransferType.Download, "确认下载完成状态重试次数已达到最大，具体信息请查看日志输出");
        }

        private void FileTransferCollection_OnCollectionChanged(MesEventArgs e)
        {
            this.FileTransferCollectionChanged?.Invoke(this, e);
        }

        private void OnBeforeFileTransfer(ProgressBlockCollection blocks)
        {
            FileOperationEventArgs args = new FileOperationEventArgs();
            args.UrlFileInfo = blocks.UrlFileInfo;
            if (blocks.UrlFileInfo.TransferType == TransferType.Download)
            {
                //下载
                if (string.IsNullOrEmpty(blocks.UrlFileInfo.SaveFullPath))
                {
                    blocks.UrlFileInfo.SaveFullPath = Path.Combine(this.receiveDirectory, blocks.UrlFileInfo.FileName);
                }
            }

            args.TransferType = blocks.UrlFileInfo.TransferType;
            args.IsPermitOperation = true;
            try
            {
                this.BeforeFileTransfer?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(BeforeFileTransfer)}中发生异常", ex);
            }
            if (blocks.UrlFileInfo.TransferType == TransferType.Download)
            {
                //下载
                blocks.UrlFileInfo.SaveFullPath = Path.GetFullPath(args.UrlFileInfo.SaveFullPath);
            }
            else
            {
                blocks.UrlFileInfo.FilePath = Path.GetFullPath(args.UrlFileInfo.FilePath);
            }
        }

        private void OnDownloadFileFinished()
        {
            try
            {
                FileStreamPool.DisposeWriteStream(this.rrqmPath, true);
                TransferFileMessageArgs args = new TransferFileMessageArgs();
                args.UrlFileInfo = this.transferUrlFileInfo;
                args.TransferType = TransferType.Download;
                TransferFileHashDictionary.AddFile(this.transferUrlFileInfo);

                ResetVariable();
                try
                {
                    this.FinishedFileTransfer?.Invoke(this, args);
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(LogType.Error, this, $"在事件{nameof(FinishedFileTransfer)}中发生异常", ex);
                }
                this.BeginTransfer();
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在完成下载时中发生异常", ex);
            }
        }

        private void OnTransferError(TransferType transferType, string msg, UrlFileInfo urlFileInfo = null)
        {
            TransferFileMessageArgs args = new TransferFileMessageArgs();
            args.UrlFileInfo = urlFileInfo == null ? this.TransferFileInfo : urlFileInfo;
            args.TransferType = transferType;
            args.Message = msg;
            switch (transferType)
            {
                case TransferType.Upload:
                    {
                        StopUpload();
                        break;
                    }
                case TransferType.Download:
                    {
                        StopDownload();
                        break;
                    }
            }

            try
            {
                this.TransferFileError?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(this.TransferFileError)}中发生异常。", ex);
            }

            this.BeginTransfer();
        }

        private void OnUploadFileFinished()
        {
            TransferFileMessageArgs args = new TransferFileMessageArgs();
            try
            {
                FileStreamPool.DisposeReadStream(this.transferUrlFileInfo.FilePath);
                args.UrlFileInfo = this.transferUrlFileInfo;
                args.TransferType = TransferType.Upload;
                ResetVariable();
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, "未知异常", ex);
            }

            try
            {
                this.FinishedFileTransfer?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(FinishedFileTransfer)}中发生异常", ex);
            }
            this.BeginTransfer();
        }

        private void ResetVariable()
        {
            this.fileBlocks = null;
            this.transferStatus = TransferStatus.None;
            this.transferUrlFileInfo = null;
            this.progress = 0;
            this.speed = 0;
            if (!string.IsNullOrEmpty(this.rrqmPath))
            {
                FileStreamPool.DisposeWriteStream(this.rrqmPath, false);
            }
            this.rrqmPath = null;
        }

        private ByteBlock SingleSendWait(short procotol, int timeout, ByteBlock byteBlock = null)
        {
            lock (this)
            {
                this.waitDataSend.Reset();
                if (!this.Online)
                {
                    throw new RRQMNotConnectedException("客户端未连接");
                }
                if (byteBlock == null)
                {
                    this.InternalSend(procotol, new byte[0], 0, 0);
                }
                else
                {
                    this.InternalSend(procotol, byteBlock.Buffer, 0, byteBlock.Len);
                }

                if (this.waitDataSend.Wait(timeout) == WaitDataStatus.Overtime)
                {
                    throw new RRQMTimeoutException("请求超时");
                }
                return this.waitDataSend.WaitResult;
            }
        }

        private void StopDownload()
        {
            if (this.disposable)
            {
                return;
            }
            this.stop = true;
            FileStreamPool.DisposeWriteStream(this.rrqmPath, false);
            if (!this.Online)
            {
                ResetVariable();
                return;
            }
            try
            {
                ByteBlock returnByteBlock = this.SingleSendWait(114, this.timeout);
                returnByteBlock.SetHolding(false);
                ResetVariable();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void StopUpload()
        {
            try
            {
                if (this.transferUrlFileInfo == null)
                {
                    return;
                }
                FileStreamPool.DisposeReadStream(this.transferUrlFileInfo.FilePath);
                if (!this.Online)
                {
                    ResetVariable();
                    return;
                }
                ByteBlock returnByteBlock = this.SingleSendWait(118, this.timeout);
                returnByteBlock.SetHolding(false);
                ResetVariable();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UploadFile(UrlFileInfo urlFileInfo, bool triggerEvent)
        {
            if (!this.Online)
            {
                throw new RRQMNotConnectedException("未连接到服务器");
            }
            else if (this.transferStatus != TransferStatus.None)
            {
                throw new RRQMTransferingException("已有传输任务在进行中");
            }
            byte[] datas = SerializeConvert.RRQMBinarySerialize(urlFileInfo, true);
            ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
            byteBlock.Write(datas);

            try
            {
                ByteBlock returnByteBlock = this.SingleSendWait(116, this.timeout, byteBlock);
                FileWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<FileWaitResult>(returnByteBlock.Buffer, 2);
                returnByteBlock.SetHolding(false);
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
                    this.transferStatus = TransferStatus.Upload;
                    this.transferUrlFileInfo = urlFileInfo;
                    this.fileBlocks = ProgressBlockCollection.CreateProgressBlockCollection(urlFileInfo);
                    if (triggerEvent)
                    {
                        this.OnBeforeFileTransfer(this.fileBlocks);
                    }
                    Task.Run(() =>
                    {
                        UploadFinished();
                    });
                }
                else
                {
                    if (FileStreamPool.LoadReadStream(ref urlFileInfo, out string mes))
                    {
                        this.transferStatus = TransferStatus.Upload;
                        ProgressBlockCollection blocks = waitResult.PBCollectionTemp.ToPBCollection();
                        blocks.UrlFileInfo = urlFileInfo;
                        this.transferUrlFileInfo = urlFileInfo;
                        this.fileBlocks = blocks;
                        if (triggerEvent)
                        {
                            this.OnBeforeFileTransfer(blocks);
                        }

                        Thread thread_Transfer = new Thread(this.UploadFileBlock);
                        thread_Transfer.IsBackground = true;
                        thread_Transfer.Name = "文件上传线程";
                        thread_Transfer.Start();
                    }
                    else
                    {
                        throw new RRQMException(mes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void UploadFileBlock()
        {
            this.stop = false;

            foreach (FileBlock fileBlock in this.fileBlocks)
            {
                if (fileBlock.RequestStatus == RequestStatus.Hovering)
                {
                    this.position = fileBlock.Position;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
                        if (!this.Online)
                        {
                            fileBlock.RequestStatus = RequestStatus.Hovering;
                            this.OnTransferError(TransferType.Download, "客户端已断开连接！！！");
                            return;
                        }
                        if (disposable || stop)
                        {
                            StopUpload();
                            return;
                        }
                        if (this.transferStatus == TransferStatus.PauseUpload)
                        {
                            transferWaitHandle.WaitOne();
                        }

                        int submitLength = surplusLength > this.packetSize ? this.packetSize : (int)surplusLength;
                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.packetSize + 17);
                        if (this.position + submitLength == fileBlock.Position + fileBlock.UnitLength)
                        {
                            byteBlock.Write((byte)1);//1
                        }
                        else
                        {
                            byteBlock.Write((byte)1);
                        }

                        byteBlock.Write(fileBlock.Index);//4
                        byteBlock.Write(this.position);//8
                        byteBlock.Write(submitLength);//4
                        //17=23
                        try
                        {
                            if (this.transferStatus != TransferStatus.Upload && this.transferStatus != TransferStatus.PauseUpload)
                            {
                                return;
                            }
                            if (FileStreamPool.ReadFile(this.transferUrlFileInfo.FilePath, out string mes, this.position, byteBlock, 17, submitLength))
                            {
                                ByteBlock returnByteBlock = this.SingleSendWait(117, this.timeout, byteBlock);

                                if (returnByteBlock.Buffer[2] == 1)
                                {
                                    reTryCount = 0;
                                    this.tempLength += submitLength;
                                    this.position += submitLength;
                                    surplusLength -= submitLength;
                                }
                                else if (returnByteBlock.Buffer[2] == 2)
                                {
                                    throw new RRQMException(Encoding.UTF8.GetString(returnByteBlock.Buffer, 3, returnByteBlock.Len - 3));
                                }
                                else
                                {
                                    this.OnTransferError(TransferType.Upload, "服务器无此传输信息，已终止本次传输");
                                    return;
                                }
                                returnByteBlock.SetHolding(false);
                            }
                            else
                            {
                                throw new RRQMException(mes);
                            }
                        }
                        catch (Exception ex)
                        {
                            reTryCount++;
                            Logger.Debug(LogType.Message, this, $"上传文件错误，正在尝试第{reTryCount}次重试", ex);
                            if (reTryCount > 10)
                            {
                                this.OnTransferError(TransferType.Upload, "重试次数达到最大，详细信息请查看日志");
                                return;
                            }
                        }
                        finally
                        {
                            byteBlock.Dispose();
                        }
                    }
                }
            }
            UploadFinished();
        }

        private void UploadFinished()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    ByteBlock returnByteBlock = this.SingleSendWait(119, this.timeout);
                    if (returnByteBlock.Length == 3 && returnByteBlock.Buffer[2] == 1)
                    {
                        this.OnUploadFileFinished();
                        return;
                    }
                    else
                    {
                        this.Logger.Debug(LogType.Warning, this, $"确认上传完成返回状态错误，即将进行第{i + 1}次重试");
                    }
                    returnByteBlock.SetHolding(false);
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(LogType.Warning, this, $"确认下载完成错误，信息：{ex.Message}，即将进行第{i + 1}次重试");
                }
            }

            this.OnTransferError(TransferType.Upload, "确认上传完成状态重试次数已达到最大，具体信息请查看日志输出");
        }
    }
}