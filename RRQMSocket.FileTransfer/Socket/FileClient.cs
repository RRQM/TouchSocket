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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯客户端主类
    /// </summary>
    public class FileClient : TokenClient, IFileClient, IDisposable
    {
        /// <summary>
        /// 无参数构造函数
        /// </summary>
        public FileClient()
        {
            this.FileTransferCollection = new TransferCollection();
            this.TransferStatus = TransferStatus.None;
            this.FileTransferCollection.OnCollectionChanged += this.FileTransferCollection_OnCollectionChanged;
        }

        #region 自定义

        private RRQMAgreementHelper agreementHelper;

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
        /// 收到字节
        /// </summary>
        public event RRQMBytesEventHandler ReceivedBytes;

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
        public FileInfo TransferFileInfo { get { return fileBlocks == null ? null : fileBlocks.FileInfo; } }

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
                if (fileBlocks.FileInfo != null)
                {
                    this.progress = fileBlocks.FileInfo.FileLength > 0 ? (float)position / fileBlocks.FileInfo.FileLength : 0;//计算下载完成进度
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
        /// <param name="filePath">文件路径</param>
        /// <param name="host">IP及端口</param>
        /// <param name="restart"></param>
        /// <param name="verifyToken">验证令箭</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="receiveDir">存放目录</param>
        /// <param name="finishedCallBack">完成时回调</param>
        /// <returns></returns>
        public static FileClient RequestDownloadFile(string filePath, string host, bool restart, string verifyToken = null, int waitTime = 10, string receiveDir = null, RRQMTransferFileMessageEventHandler finishedCallBack = null)
        {
            FileClient fileClient = new FileClient();
            try
            {
                var config = new FileClientConfig();
                config.SetValue(TcpClientConfig.RemoteIPHostProperty, new IPHost(host))
                    .SetValue(TokenClientConfig.VerifyTokenProperty, verifyToken);

                fileClient.Setup(config);
                if (finishedCallBack != null)
                {
                    fileClient.FinishedFileTransfer += finishedCallBack;
                }
                fileClient.FinishedFileTransfer += (object sender, TransferFileMessageArgs e) => { fileClient.Dispose(); };
                fileClient.BeforeFileTransfer += (object sender, FileOperationEventArgs e) =>
                {
                    if (e.TransferType == TransferType.Download)
                    {
                        e.TargetPath = Path.Combine(receiveDir == null ? "" : receiveDir, e.FileInfo.FileName);
                    }
                };
                UrlFileInfo fileInfo = UrlFileInfo.CreatDownload(filePath, restart);
                fileInfo.Timeout = waitTime;
                fileClient.RequestTransfer(fileInfo);
                return fileClient;
            }
            catch (Exception ex)
            {
                fileClient.Dispose();
                throw new RRQMException(ex.Message);
            }
        }

        /// <summary>
        /// 请求上传
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="host">IP及端口</param>
        /// <param name="restart"></param>
        /// <param name="verifyToken">验证令箭</param>
        /// <param name="finishedCallBack">完成时回调</param>
        /// <returns></returns>
        public static FileClient RequestUploadFile(string filePath, string host, bool restart, string verifyToken = null, RRQMTransferFileMessageEventHandler finishedCallBack = null)
        {
            FileClient fileClient = new FileClient();
            try
            {
                var config = new FileClientConfig();
                config.SetValue(TcpClientConfig.RemoteIPHostProperty, new IPHost(host))
                    .SetValue(TokenClientConfig.VerifyTokenProperty, verifyToken);

                fileClient.Setup(config);

                if (finishedCallBack != null)
                {
                    fileClient.FinishedFileTransfer += finishedCallBack;
                }
                fileClient.FinishedFileTransfer += (object sender, TransferFileMessageArgs e) => { fileClient.Dispose(); };

                UrlFileInfo fileInfo = UrlFileInfo.CreatUpload(filePath, restart, fileClient.BreakpointResume);
                fileClient.UploadFile(fileInfo);
                return fileClient;
            }
            catch (Exception ex)
            {
                fileClient.Dispose();
                throw new RRQMException(ex.Message);
            }
        }

        /// <summary>
        /// 调用操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="opeartionName"></param>
        /// <param name="waitTime"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T CallOperation<T>(string opeartionName, int waitTime = 5, params object[] parameters)
        {
            OperationContext context = new OperationContext();
            context.OperationName = opeartionName;
            context.ParametersBytes = new List<byte[]>();
            foreach (var item in parameters)
            {
                context.ParametersBytes.Add(SerializeConvert.RRQMBinarySerialize(item, true));
            }
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            context.Serialize(byteBlock);
            try
            {
                ByteBlock returnBlock = this.SendWait(1040, waitTime, byteBlock);
                if (returnBlock != null)
                {
                    OperationContext operationContext = OperationContext.Deserialize(returnBlock.Buffer, 4);
                    if (operationContext.Status == 1)
                    {
                        if (typeof(T) != typeof(Nullable))
                        {
                            return SerializeConvert.RRQMBinaryDeserialize<T>(operationContext.ReturnParameterBytes, 0);
                        }
                        return default;
                    }
                    else
                    {
                        throw new RRQMException(operationContext.Message);
                    }
                }
                else
                {
                    throw new RRQMTimeoutException("操作超时");
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
            agreementHelper = new RRQMAgreementHelper(this);
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
            if (waitHandle != null)
            {
                this.waitHandle.Set();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void Send(byte[] buffer, int offset, int length)
        {
            this.agreementHelper.SocketSend(1030, buffer, offset, length);
        }

        /// <summary>
        /// 发送数据，依然采用会同步
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void SendAsync(byte[] buffer, int offset, int length)
        {
            this.agreementHelper.SocketSend(1030, buffer, offset, length);
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
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            int agreement = BitConverter.ToInt32(byteBlock.Buffer, 0);
            switch (agreement)
            {
                case 0:
                    {
                        break;
                    }
                case 999:
                    {
                        if (this.waitDataSend != null)
                        {
                            byteBlock.SetHolding(true);
                            this.waitDataSend.Set(byteBlock);
                        }
                        break;
                    }
                case 1030:
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                byte[] data = new byte[byteBlock.Length - 4];
                                byteBlock.Position = 4;
                                byteBlock.Read(data);
                                this.ReceivedBytes?.Invoke(this, new BytesEventArgs(data));
                            }
                            catch (Exception ex)
                            {
                                Logger.Debug(LogType.Error, this, ex.Message, ex);
                            }
                        });
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            clientConfig.OnlySend = false;
            base.LoadConfig(clientConfig);
            this.BufferLength = 1024 * 64;
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
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
        /// <param name="fileInfo"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMTransferingException"></exception>
        /// <exception cref="RRQMException"></exception>
        private void DownloadFile(UrlFileInfo fileInfo)
        {
            lock (locker)
            {
                if (!this.Online)
                {
                    throw new RRQMException("未连接服务器");
                }
                else if (this.TransferStatus != TransferStatus.None)
                {
                    throw new RRQMTransferingException("已有传输任务在进行中");
                }

                byte[] datas = SerializeConvert.RRQMBinarySerialize(fileInfo, true);
                ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
                byteBlock.Write(datas);
                try
                {
                    ByteBlock returnByteBlock = this.SendWait(1001, fileInfo.Timeout, byteBlock);
                    if (returnByteBlock == null)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                    FileWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<FileWaitResult>(returnByteBlock.Buffer, 4);
                    returnByteBlock.SetHolding(false);
                    if (waitResult.Status == 2)
                    {
                        throw new RRQMTransferErrorException("服务器拒绝下载");
                    }

                    StartDownloadFile(waitResult.PBCollectionTemp.ToPBCollection(), fileInfo.Restart);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        private void DownloadFileBlock(object o)
        {
            this.stop = false;
            waitHandle = new AutoResetEvent(false);
            foreach (FileProgressBlock fileBlock in this.fileBlocks)
            {
                if (!fileBlock.Finished)
                {
                    this.position = fileBlock.StreamPosition;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
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

                        long requestLength = surplusLength > (this.BufferLength - 5) ? (this.BufferLength - 5) : surplusLength;
                        byteBlock.Write(BitConverter.GetBytes(requestLength));

                        try
                        {
                            ByteBlock returnByteBlock = this.SendWait(1002, this.timeout, byteBlock);
                            if (returnByteBlock == null || returnByteBlock.Buffer[4] == 2)
                            {
                                reTryCount++;
                                Logger.Debug(LogType.Message, this, $"下载文件错误，正在尝试第{reTryCount}次重试");
                                if (reTryCount > 10)
                                {
                                    this.OutDownload(true);
                                    TransferFileMessageArgs args = new TransferFileMessageArgs();
                                    args.FileInfo = this.fileBlocks.FileInfo;
                                    args.TransferType = TransferType.Download;
                                    args.Message = "请求下载文件错误";
                                    this.TransferFileError?.Invoke(this, args);
                                    this.BeginTransfer();
                                    return;
                                }
                            }
                            else if (returnByteBlock.Buffer[4] == 1 || returnByteBlock.Buffer[4] == 3)
                            {
                                reTryCount = 0;

                                while (true)
                                {
                                    if (this.TransferStatus != TransferStatus.Download && this.TransferStatus != TransferStatus.PauseDownload)
                                    {
                                        return;
                                    }
                                    string mes;
                                    if (FileBaseTool.WriteFile(downloadStream, out mes, this.position, returnByteBlock.Buffer, 5, (int)returnByteBlock.Position - 5))
                                    {
                                        tempLength += requestLength;
                                        this.position += requestLength;
                                        surplusLength -= requestLength;
                                        if (returnByteBlock.Buffer[4] == 3)
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
                ByteBlock resultByteBlock = this.SendWait(1004, this.timeout);

                if (resultByteBlock != null)
                {
                    if (resultByteBlock.Length == 5 && resultByteBlock.Buffer[4] == 1)
                    {
                        FileBaseTool.FileFinished(downloadStream);
                        FilePathEventArgs args = new FilePathEventArgs();
                        args.FileInfo = fileBlocks.FileInfo;
                        args.TargetPath = args.FileInfo.FilePath;
                        args.TransferType = TransferType.Download;
                        TransferFileHashDictionary.AddFile(fileBlocks.FileInfo);

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
            if (this.waitHandle != null)
            {
                this.waitHandle.Dispose();
                this.waitHandle = null;
            }
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
                ByteBlock byteBlock = this.SendWait(1003, this.timeout);
                if (byteBlock != null)
                {
                    byteBlock.SetHolding(false);
                }
            }
        }

        private void OutUpload()
        {
            if (this.waitHandle != null)
            {
                this.waitHandle.Dispose();
                this.waitHandle = null;
            }
            if (this.TransferFileInfo != null)
            {
                TransferFileStreamDic.DisposeFileStream(this.TransferFileInfo.FilePath);
            }
            this.fileBlocks = null;
            this.TransferStatus = TransferStatus.None;
            this.progress = 0;
            this.speed = 0;
        }

        private void ReConnect()
        {
            if (!this.Online)
            {
                this.Disconnect();
                this.Logger.Debug(LogType.Warning, this, $"客户端已断连，正在尝试恢复连接！！！");
                try
                {
                    this.Connect();
                }
                catch (Exception ex)
                {
                    this.Logger.Debug(LogType.Error, this, $"尝试恢复连接时发生错误：{ex.Message}");
                }
            }
        }

        private ByteBlock SendWait(int agreement, int waitTime, ByteBlock byteBlock = null)
        {
            //1001:请求下载
            //1002:请求下载分包
            //1003:停止下载
            //1004:下载完成

            lock (locker)
            {
                this.waitDataSend = new WaitData<ByteBlock>();
                try
                {
                    if (byteBlock == null)
                    {
                        agreementHelper.SocketSend(agreement);
                    }
                    else
                    {
                        agreementHelper.SocketSend(agreement, byteBlock.Buffer, 0, (int)byteBlock.Length);
                    }
                }
                catch
                {
                }

                this.waitDataSend.Wait(waitTime * 1000);
                ByteBlock resultByteBlock = this.waitDataSend.WaitResult;
                this.waitDataSend.Dispose();
                this.waitDataSend = null;
                if (resultByteBlock == null)
                {
                    ReConnect();
                }
                return resultByteBlock;
            }
        }

        private void StartDownloadFile(ProgressBlockCollection blocks, bool restart)
        {
            if (this.TransferStatus != TransferStatus.None)
            {
                return;
            }
            this.TransferStatus = TransferStatus.Download;

            FileOperationEventArgs args = new FileOperationEventArgs();
            args.FileInfo = blocks.FileInfo;
            args.TransferType = TransferType.Download;
            args.TargetPath = Path.Combine(receiveDirectory, blocks.FileInfo.FileName);
            args.IsPermitOperation = true;
            this.BeforeFileTransfer?.Invoke(this, args);
            blocks.FileInfo.FilePath = args.TargetPath;

            downloadStream = RRQMStream.GetRQMStream(ref blocks, restart, this.breakpointResume);
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
            args.FileInfo = blocks.FileInfo;
            args.TargetPath = blocks.FileInfo.FilePath;
            args.TransferType = TransferType.Upload;
            args.IsPermitOperation = true;
            this.BeforeFileTransfer?.Invoke(this, args);
            blocks.FileInfo.FilePath = args.TargetPath;
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
                ByteBlock byteBlock = this.SendWait(1013, this.timeout);
                if (byteBlock != null && byteBlock.Buffer[4] == 1)
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
            ByteBlock byteBlock = this.SendWait(1020, this.timeout);
            if (byteBlock == null)
            {
                return;
            }
            TransferSetting transferSetting = TransferSetting.Deserialize(byteBlock.Buffer, 4);
            this.breakpointResume = transferSetting.breakpointResume;
            this.BufferLength = transferSetting.bufferLength;
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
            lock (locker)
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
                    ByteBlock resultByteBlock = this.SendWait(1010, this.timeout, byteBlock);
                    if (resultByteBlock == null)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                    FileWaitResult waitResult = SerializeConvert.RRQMBinaryDeserialize<FileWaitResult>(resultByteBlock.Buffer, 4);
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
                        args.FileInfo = requsetBlocks.FileInfo;
                        args.TransferType = TransferType.Upload;
                        args.TargetPath = requsetBlocks.FileInfo.FilePath;
                        args.IsPermitOperation = true;
                        this.BeforeFileTransfer?.Invoke(this, args);

                        requsetBlocks.FileInfo.FilePath = args.TargetPath;
                        this.fileBlocks = requsetBlocks;

                        Task.Run(() =>
                        {
                            UploadFileFinished_s();
                        });
                    }
                    else
                    {
                        ProgressBlockCollection blocks = waitResult.PBCollectionTemp.ToPBCollection();
                        blocks.FileInfo.FilePath = urlFileInfo.FilePath;
                        this.StartUploadFile(blocks);
                    }
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }
        private void UploadFileBlock()
        {
            this.stop = false;
            waitHandle = new AutoResetEvent(false);
            foreach (FileProgressBlock fileBlock in this.fileBlocks)
            {
                if (!fileBlock.Finished)
                {
                    this.position = fileBlock.StreamPosition;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
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
                        long submitLength = surplusLength > (this.BufferLength - 21) ? (this.BufferLength - 21) : surplusLength;
                        byte[] submitLengthBytes = BitConverter.GetBytes(submitLength);
                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
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
                            if (FileBaseTool.ReadFileBytes(this.fileBlocks.FileInfo.FilePath, this.position, byteBlock, 21, (int)submitLength))
                            {
                                ByteBlock returnByteBlock = this.SendWait(1011, this.timeout, byteBlock);
                                if (returnByteBlock != null && returnByteBlock.Buffer.Length > 4)
                                {
                                    if (returnByteBlock.Buffer[4] == 1)
                                    {
                                        reTryCount = 0;
                                        this.tempLength += submitLength;
                                        this.position += submitLength;
                                        surplusLength -= submitLength;
                                    }
                                    else if (returnByteBlock.Buffer[4] == 2)
                                    {
                                        reTryCount++;
                                        Logger.Debug(LogType.Message, this, $"上传文件错误，正在尝试第{reTryCount}次重试");
                                        if (reTryCount > 10)
                                        {
                                            TransferFileMessageArgs args = new TransferFileMessageArgs();
                                            args.FileInfo = this.fileBlocks.FileInfo;
                                            args.TransferType = TransferType.Upload;
                                            args.Message = "上传文件错误";
                                            this.TransferFileError?.Invoke(this, args);
                                            this.BeginTransfer();
                                        }
                                    }
                                    else if (returnByteBlock.Buffer[4] == 3)
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
                                        args.FileInfo = this.fileBlocks.FileInfo;
                                        args.TransferType = TransferType.Upload;
                                        args.Message = "服务器无此传输信息，已终止本次传输";
                                        OutUpload();
                                        this.TransferFileError?.Invoke(this, args);
                                        this.BeginTransfer();
                                    }
                                }
                                else
                                {
                                    reTryCount++;
                                    Logger.Debug(LogType.Message, this, $"上传文件错误，正在尝试第{reTryCount}次重试");
                                    if (reTryCount > 10)
                                    {
                                        TransferFileMessageArgs args = new TransferFileMessageArgs();
                                        args.FileInfo = this.fileBlocks.FileInfo;
                                        args.TransferType = TransferType.Upload;
                                        args.Message = "上传文件错误";
                                        this.TransferFileError?.Invoke(this, args);
                                        this.BeginTransfer();
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
                                    args.FileInfo = this.fileBlocks.FileInfo;
                                    args.TransferType = TransferType.Upload;
                                    args.Message = "读取上传文件错误";
                                    this.TransferFileError?.Invoke(this, args);
                                    this.BeginTransfer();
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
                ByteBlock byteBlock = this.SendWait(1012, this.timeout);
                if (byteBlock != null)
                {
                    if (byteBlock.Length == 5 && byteBlock.Buffer[4] == 1)
                    {
                        FilePathEventArgs args = new FilePathEventArgs();
                        args.FileInfo = fileBlocks.FileInfo;
                        args.TargetPath = args.FileInfo.FilePath;
                        args.TransferType = TransferType.Upload;

                        this.fileBlocks = null;
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