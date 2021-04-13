//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.IO;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 通讯客户端主类
    /// </summary>
    public class FileClient : TokenTcpClient, IFileClient
    {
        /// <summary>
        /// 无参数构造函数
        /// </summary>
        public FileClient()
        {
            this.BufferLength = 1024 * 64;
            this.TransferType = TransferType.None;
            this.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();
        }

        private string receiveDirectory = string.Empty;

        /// <summary>
        /// 获取正在下载的文件信息
        /// </summary>
        public FileInfo DownloadFileInfo { get { return downloadFileBlocks == null ? null : downloadFileBlocks.FileInfo; } }

        /// <summary>
        /// 获取下载进度
        /// </summary>
        public float DownloadProgress
        {
            get
            {
                if (downloadFileBlocks == null)
                {
                    return 0;
                }
                if (downloadFileBlocks.FileInfo != null)
                {
                    this.downloadProgress = downloadFileBlocks.FileInfo.FileLength > 0 ? (float)downloadPosition / downloadFileBlocks.FileInfo.FileLength : 0;//计算下载完成进度
                }
                else
                {
                    this.downloadProgress = 0;
                }
                return downloadProgress <= 1 ? downloadProgress : 1;
            }
        }

        /// <summary>
        /// 获取下载速度
        /// </summary>
        public long DownloadSpeed
        {
            get
            {
                this.downloadSpeed = tempReceiveLength;
                tempReceiveLength = 0;
                return downloadSpeed;
            }
        }

        /// <summary>
        /// 获取上传速度
        /// </summary>
        public long UploadSpeed
        {
            get
            {
                this.uploadSpeed = tempSendLength;
                tempSendLength = 0;
                return uploadSpeed;
            }
        }

        /// <summary>
        ///  获取或设置默认接收文件的存放目录
        /// </summary>
        public string ReceiveDirectory
        {
            get { return receiveDirectory; }
            set { receiveDirectory = value == null ? string.Empty : value; }
        }

        /// <summary>
        /// 获取下载状态
        /// </summary>
        public TransferType TransferType { get; private set; }

        /// <summary>
        /// 获取正在上传的文件信息
        /// </summary>
        public FileInfo UploadFileInfo { get { return uploadFileBlocks == null ? null : uploadFileBlocks.FileInfo; } }

        /// <summary>
        /// 获取上传进度
        /// </summary>
        public float UploadProgress
        {
            get
            {
                if (uploadFileBlocks == null)
                {
                    return 0;
                }
                if (uploadFileBlocks.FileInfo != null)
                {
                    this.uploadProgress = uploadFileBlocks.FileInfo.FileLength > 0 ? (float)uploadPosition / uploadFileBlocks.FileInfo.FileLength : 0;//计算上传完成进度
                }
                else
                {
                    this.uploadProgress = 0;
                }
                return uploadProgress <= 1 ? uploadProgress : 1;
            }
        }

        private int timeout = 10;

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
        /// 正在下载的文件包
        /// </summary>
        public ProgressBlockCollection DownloadFileBlocks
        {
            get { return downloadFileBlocks; }
        }

        /// <summary>
        /// 正在上传的文件包
        /// </summary>
        public ProgressBlockCollection UploadFileBlocks
        {
            get { return uploadFileBlocks; }
        }

        private ProgressBlockCollection downloadFileBlocks;
        private ProgressBlockCollection uploadFileBlocks;

        private float downloadProgress;
        private long downloadSpeed;
        private float uploadProgress;
        private long uploadSpeed;
        private bool isPauseDownload;
        private bool isPauseUpload;
        private long downloadPosition;
        private long uploadPosition;
        private RRQMStream downloadFileStream;
        private long tempReceiveLength;
        private long tempSendLength;
        private Thread thread_Download;
        private Thread thread_Upload;
        private EventWaitHandle waitHandleDownload;
        private EventWaitHandle waitHandleUpload;
        private WaitData<ByteBlock> waitDataSend;
        private RRQMAgreementHelper AgreementHelper;
        private bool breakpointResume;

        /// <summary>
        /// 刚开始下载文件的时候
        /// </summary>
        public event RRQMTransferFileEventHandler BeforeDownloadFile;

        /// <summary>
        /// 刚开始上传文件的时候
        /// </summary>
        public event RRQMFileEventHandler BeforeUploadFile;

        /// <summary>
        /// 当文件接收完成时
        /// </summary>
        public event RRQMFileFinishedEventHandler DownloadFileFinished;

        /// <summary>
        /// 传输文件错误
        /// </summary>
        public event RRQMTransferFileMessageEventHandler TransferFileError;

        /// <summary>
        /// 当文件上传完成时
        /// </summary>
        public event RRQMFileFinishedEventHandler UploadFileFinished;

        /// <summary>
        /// 当接收到系统信息的时候
        /// </summary>
        public event RRQMMessageEventHandler ReceiveSystemMes;

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="addressFamily"></param>
        /// <param name="endPoint"></param>
        public override void Connect(AddressFamily addressFamily, EndPoint endPoint)
        {
            base.Connect(addressFamily, endPoint);
            AgreementHelper = new RRQMAgreementHelper(this);
            SynchronizeTransferSetting();
        }
        private void SynchronizeTransferSetting()
        {
            ByteBlock byteBlock = this.SendWait(1020, this.timeout);
            if (byteBlock == null)
            {
                return;
            }
            byteBlock.Position = 0;
            TransferSetting transferSetting = SerializeConvert.BinaryDeserialize<TransferSetting>(byteBlock);
            this.breakpointResume = transferSetting.breakpointResume;
            this.BufferLength = transferSetting.bufferLength;
            byteBlock.SetHolding(false);
        }

        private void BeforeDownloadFileMethod(object sender, TransferFileEventArgs e)
        {
            BeforeDownloadFile?.Invoke(sender, e);
        }

        private void BeforeUploadFileMethod(object sender, FileEventArgs e)
        {
            BeforeUploadFile?.Invoke(sender, e);
        }

        private void DownloadFileFinishedMethod(object sender, FileFinishedArgs e)
        {
            DownloadFileFinished?.Invoke(sender, e);
        }

        private void TransferFileErrorMethod(object sender, TransferFileMessageArgs e)
        {
            TransferFileError?.Invoke(sender, e);
        }

        private void UploadFileFinishedMethod(object sender, FileFinishedArgs e)
        {
            UploadFileFinished?.Invoke(sender, e);
        }

        private void ReceiveSystemMesMethod(object sender, MesEventArgs e)
        {
            ReceiveSystemMes?.Invoke(sender, e);
        }

        /// <summary>
        /// 请求下载文件
        /// </summary>
        /// <param name="url">请求参数</param>
        /// <param name="waitTime">请求等待时间</param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMTransferingException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void DownloadFile(FileUrl url, int waitTime)
        {
            lock (locker)
            {
                if (!this.Online)
                {
                    throw new RRQMException("未连接服务器");
                }
                else if (this.TransferType != TransferType.None)
                {
                    throw new RRQMTransferingException("已有传输任务在进行中");
                }

                byte[] datas = SerializeConvert.BinarySerialize(url);
                ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
                byteBlock.Write(datas);
                try
                {
                    ByteBlock returnByteBlock = this.SendWait(1001, waitTime, byteBlock);
                    if (returnByteBlock == null)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                    returnByteBlock.Position = 0;
                    FileWaitResult waitResult = SerializeConvert.BinaryDeserialize<FileWaitResult>(returnByteBlock);
                    returnByteBlock.SetHolding(false);
                    if (waitResult.Status == 2)
                    {
                        throw new RRQMTransferErrorException(waitResult.Message);
                    }

                    StartDownloadFile((ProgressBlockCollection)waitResult.Data, url.Restart);
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// 请求下载文件
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="host">IP及端口</param>
        /// <param name="verifyToken">验证令箭</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="receiveDir">存放目录</param>
        /// <param name="finishedCallBack">完成时回调</param>
        /// <returns></returns>
        public static FileClient RequestDownloadFile(FileUrl url, string host, string verifyToken = null, int waitTime = 10, string receiveDir = null, RRQMFileFinishedEventHandler finishedCallBack = null)
        {
            FileClient fileClient = new FileClient();
            fileClient.VerifyToken = verifyToken;
            try
            {
                IPHost iPHost = IPHost.CreatIPHost(host);
                fileClient.Connect(iPHost.AddressFamily, iPHost.EndPoint);

                if (finishedCallBack != null)
                {
                    fileClient.DownloadFileFinished += finishedCallBack;
                }
                fileClient.DownloadFileFinished += (object sender, FileFinishedArgs e) => { fileClient.Dispose(); };
                fileClient.BeforeDownloadFile += (object sender, TransferFileEventArgs e) =>
                {
                    e.TargetPath = Path.Combine(receiveDir == null ? "" : receiveDir, e.FileInfo.FileName);
                };
                fileClient.DownloadFile(url, waitTime);
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
        /// <param name="url">URL</param>
        /// <param name="host">IP及端口</param>
        /// <param name="verifyToken">验证令箭</param>
        /// <param name="finishedCallBack">完成时回调</param>
        /// <returns></returns>
        public static FileClient RequestUploadFile(FileUrl url, string host, string verifyToken = null, RRQMFileFinishedEventHandler finishedCallBack = null)
        {
            FileClient fileClient = new FileClient();
            fileClient.VerifyToken = verifyToken;
            try
            {
                IPHost iPHost = IPHost.CreatIPHost(host);
                fileClient.Connect(iPHost.AddressFamily, iPHost.EndPoint);
                fileClient.UploadFile(url);
                if (finishedCallBack != null)
                {
                    fileClient.UploadFileFinished += finishedCallBack;
                }
                fileClient.UploadFileFinished += (object sender, FileFinishedArgs e) => { fileClient.Dispose(); };
                return fileClient;
            }
            catch (Exception ex)
            {
                fileClient.Dispose();
                throw new RRQMException(ex.Message);
            }

        }
       
        /// <summary>
        /// 请求删除文件
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void RequestDelete(FileUrl url)
        {
            lock (locker)
            {
                if (!this.Online)
                {
                    throw new RRQMException("未连接服务器");
                }
                byte[] datas = SerializeConvert.BinarySerialize(url);
                ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
                byteBlock.Write(datas);
                try
                {
                    ByteBlock returnByteBlock = this.SendWait(1021, this.timeout, byteBlock);
                    if (returnByteBlock == null || returnByteBlock.Length == 0)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                    byte[] returnData = returnByteBlock.ToArray();
                    returnByteBlock.SetHolding(false);
                    if (returnData[0] == 1)
                    {
                        return;
                    }
                    else if (returnData[0] == 2)
                    {
                        throw new FileNotFoundException("未找到该文件路径");
                    }
                    else if (returnData[0] == 3)
                    {
                        throw new RRQMException("服务器拒绝操作");
                    }
                    else if (returnData[0] == 4)
                    {
                        throw new RRQMException(Encoding.UTF8.GetString(returnData, 1, returnData.Length - 1));
                    }
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// 请求获取文件信息
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="RRQMException"></exception>
        public FileInfo RequestFileInfo(FileUrl url)
        {
            lock (locker)
            {
                if (!this.Online)
                {
                    throw new RRQMException("未连接服务器");
                }
                byte[] datas = SerializeConvert.BinarySerialize(url);
                ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
                byteBlock.Write(datas);
                try
                {
                    ByteBlock returnByteBlock = this.SendWait(1022, this.timeout, byteBlock);
                    if (returnByteBlock == null || returnByteBlock.Length == 0)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                    byte[] returnData = returnByteBlock.ToArray();
                    returnByteBlock.SetHolding(false);
                    if (returnData[0] == 1)
                    {
                        FileInfo fileInfo = SerializeConvert.BinaryDeserialize<FileInfo>(returnData,1,returnData.Length);
                        return fileInfo;
                    }
                    else if (returnData[0] == 2)
                    {
                        throw new FileNotFoundException("未找到该文件路径");
                    }
                    else if (returnData[0] == 3)
                    {
                        throw new RRQMException("服务器拒绝操作");
                    }
                    else if (returnData[0] == 4)
                    {
                        throw new RRQMException(Encoding.UTF8.GetString(returnData, 1, returnData.Length - 1));
                    }
                }
                finally
                {
                    byteBlock.Dispose();
                }

                return null;
            }
        }

        /// <summary>
        /// 发送系统消息
        /// </summary>
        /// <param name="Text">文本</param>
        ///<exception cref="RRQMException"></exception>
        public void SendSystemMessage(string Text)
        {
            try
            {
                byte[] datas = Encoding.UTF8.GetBytes(Text);
                ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);

                try
                {
                    byteBlock.Write(datas);
                    ByteBlock returnByteBlock = this.SendWait(1000, this.timeout, byteBlock);
                    if (returnByteBlock != null)
                    {
                        returnByteBlock.SetHolding(false);
                    }
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new RRQMException(e.Message);
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMTransferingException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void UploadFile(FileUrl url)
        {
            lock (locker)
            {
                if (!this.Online)
                {
                    throw new RRQMNotConnectedException("未连接到服务器");
                }
                else if (this.TransferType != TransferType.None)
                {
                    throw new RRQMTransferingException("已有传输任务在进行中");
                }
                UrlFileInfo urlFileInfo = new UrlFileInfo();

                using (FileStream stream = File.OpenRead(url.FilePath))
                {
                    urlFileInfo.Restart = url.Restart;
                    urlFileInfo.FilePath = url.FilePath;
                    urlFileInfo.Flag = url.Flag;
                    if (breakpointResume)
                    {
                        urlFileInfo.FileHash = FileControler.GetStreamHash(stream);
                    }
                    urlFileInfo.FileLength = stream.Length;
                    urlFileInfo.FileName = Path.GetFileName(url.FilePath);
                }
                this.UploadFile(urlFileInfo);
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="urlFileInfo"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMTransferingException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void UploadFile(UrlFileInfo urlFileInfo)
        {
            lock (locker)
            {
                if (!this.Online)
                {
                    throw new RRQMNotConnectedException("未连接到服务器");
                }
                else if (this.TransferType != TransferType.None)
                {
                    throw new RRQMTransferingException("已有传输任务在进行中");
                }
                RequestUploadFileBlock requsetBlocks = FileBaseTool.GetRequestProgressBlockCollection(urlFileInfo, urlFileInfo.Restart);
                byte[] datas = SerializeConvert.BinarySerialize(requsetBlocks);
                ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length);
                byteBlock.Write(datas);

                try
                {
                    ByteBlock resultByteBlock = this.SendWait(1010, this.timeout, byteBlock);
                    if (resultByteBlock == null)
                    {
                        throw new RRQMTimeoutException("等待结果超时");
                    }
                    resultByteBlock.Position = 0;
                    FileWaitResult waitResult = SerializeConvert.BinaryDeserialize<FileWaitResult>(resultByteBlock);
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
                        this.TransferType = TransferType.Upload;

                        TransferFileEventArgs args = new TransferFileEventArgs();
                        args.FileInfo = requsetBlocks.FileInfo;
                        args.TargetPath = requsetBlocks.FileInfo.FilePath;
                        BeforeUploadFileMethod(this, args);//触发接收文件事件
                        requsetBlocks.FileInfo.FilePath = args.TargetPath;
                        this.uploadFileBlocks = requsetBlocks;

                        Task.Run(() =>
                        {
                            UploadFileFinished_s();
                        });
                    }
                    else
                    {
                        ProgressBlockCollection blocks = (ProgressBlockCollection)waitResult.Data;
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

        /// <summary>
        /// 暂停下载
        /// </summary>
        public void PauseDownload()
        {
            this.isPauseDownload = true;
        }

        /// <summary>
        /// 暂停上传
        /// </summary>
        public void PauseUpload()
        {
            this.isPauseUpload = true;
        }

        /// <summary>
        /// 继续当前下载
        /// </summary>
        ///<exception cref="RRQMNotTransferException"></exception>
        public void ResumeDownload()
        {
            if (waitHandleDownload == null)
            {
                throw new RRQMNotTransferException("没有可以继续的任务");
            }
            else
            {
                this.isPauseDownload = false;
                waitHandleDownload.Set();
            }
        }

        /// <summary>
        /// 继续上传
        /// </summary>
        ///<exception cref="RRQMNotTransferException"></exception>
        ///<exception cref="RRQMException"></exception>
        public void ResumeUpload()
        {
            if (waitHandleUpload == null)
            {
                throw new RRQMNotTransferException("没有可以继续的任务");
            }
            else
            {
                this.isPauseUpload = false;
                waitHandleUpload.Set();
            }
        }

        /// <summary>
        /// 停止当前下载
        /// </summary>
        ///<exception cref="RRQMException"></exception>
        public void StopDownload()
        {
            OutDownload(true);
        }

        /// <summary>
        /// 停止上传
        /// </summary>
        public void StopUpload()
        {
            try
            {
                ByteBlock byteBlock = this.SendWait(1013, this.timeout);
                if (byteBlock != null && byteBlock.Buffer[0] == 1)
                {
                    byteBlock.SetHolding(false);
                    OutUpload();
                }
                else
                {
                    throw new RRQMException("未能成功停止上传，请重试");
                }
            }
            catch (Exception)
            {
                throw new RRQMException("未能成功停止上传，请重试");
            }
        }

        private void StartDownloadFile(ProgressBlockCollection blocks, bool restart)
        {
            if (this.TransferType != TransferType.None)
            {
                return;
            }
            this.TransferType = TransferType.Download;

            TransferFileEventArgs args = new TransferFileEventArgs();
            args.FileInfo = blocks.FileInfo;

            args.TargetPath = Path.Combine(receiveDirectory, blocks.FileInfo.FileName);

            BeforeDownloadFileMethod(this, args);//触发接收文件事件
            blocks.FileInfo.FilePath = args.TargetPath;

            downloadFileStream = FileBaseTool.GetNewFileStream(ref blocks, restart);
            downloadFileBlocks = blocks;
            this.isPauseDownload = false;

            this.SynchronizeTransferSetting();

            thread_Download = new Thread(this.DownloadFileBlock);
            thread_Download.IsBackground = true;
            thread_Download.Name = "文件下载线程";
            thread_Download.Start();
        }

        private void StartUploadFile(ProgressBlockCollection blocks)
        {
            this.TransferType = TransferType.Upload;

            TransferFileEventArgs args = new TransferFileEventArgs();
            args.FileInfo = blocks.FileInfo;
            args.TargetPath = blocks.FileInfo.FilePath;
            BeforeUploadFileMethod(this, args);//触发接收文件事件
            blocks.FileInfo.FilePath = args.TargetPath;
            this.uploadFileBlocks = blocks;
            this.isPauseUpload = false;

            this.SynchronizeTransferSetting();

            thread_Upload = new Thread(this.UploadFileBlock);
            thread_Upload.IsBackground = true;
            thread_Upload.Name = "文件上传线程";
            thread_Upload.Start();
        }

        private void DownloadFileBlock(object o)
        {
            waitHandleDownload = new AutoResetEvent(false);
            foreach (FileProgressBlock fileBlock in this.downloadFileBlocks)
            {
                if (!fileBlock.Finished)
                {
                    this.downloadPosition = fileBlock.StreamPosition;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
                        if (disposable)
                        {
                            OutDownload(true);
                            return;
                        }
                        if (this.isPauseDownload)
                        {
                            waitHandleDownload.WaitOne();
                        }

                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        byteBlock.Write(BitConverter.GetBytes(this.downloadPosition));

                        long requestLength = surplusLength > (this.BufferLength - 1) ? (this.BufferLength - 1) : surplusLength;
                        byteBlock.Write(BitConverter.GetBytes(requestLength));

                        try
                        {
                            ByteBlock returnByteBlock = this.SendWait(1002, this.timeout, byteBlock);
                            if (returnByteBlock == null || returnByteBlock.Buffer[0] == 2)
                            {
                                reTryCount++;
                                Logger.Debug(LogType.Message, this, $"下载文件错误，正在尝试第{reTryCount}次重试");
                                if (reTryCount > 10)
                                {
                                    this.OutDownload(true);
                                    TransferFileMessageArgs args = new TransferFileMessageArgs();
                                    args.FileInfo = this.downloadFileBlocks.FileInfo;
                                    args.TransferType = TransferType.Download;
                                    args.Message = "下载文件错误";
                                    TransferFileErrorMethod(this, args);
                                    return;
                                }
                            }
                            else
                            {
                                reTryCount = 0;

                                while (true)
                                {
                                    if (this.TransferType != TransferType.Download)
                                    {
                                        return;
                                    }
                                    string mes;
                                    if (FileBaseTool.WriteFile(downloadFileStream, out mes, this.downloadPosition, returnByteBlock.Buffer, 1, (int)returnByteBlock.Position - 1))
                                    {
                                        tempReceiveLength += requestLength;
                                        this.downloadPosition += requestLength;
                                        surplusLength -= requestLength;
                                        if (returnByteBlock.Buffer[0] == 3)
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
                                            args.FileInfo = this.DownloadFileInfo;
                                            args.TransferType = TransferType.Download;
                                            args.Message = "文件写入错误：" + mes;
                                            this.OutDownload(true);
                                            TransferFileErrorMethod(this, args);
                                            return;
                                        }
                                    }
                                }
                            }

                            if (returnByteBlock != null)
                            {
                                returnByteBlock.SetHolding(false);
                            }
                        }
                        finally
                        {
                            byteBlock.Dispose();
                        }
                    }
                    fileBlock.Finished = true;
                    FileBaseTool.SaveProgressBlockCollection(downloadFileStream, this.downloadFileBlocks);
                }
            }

            Task.Run(() => { this.DownloadFileFinished_s(); });
        }

        private void UploadFileBlock()
        {
            waitHandleUpload = new AutoResetEvent(false);
            foreach (FileProgressBlock fileBlock in this.uploadFileBlocks)
            {
                if (!fileBlock.Finished)
                {
                    this.uploadPosition = fileBlock.StreamPosition;
                    long surplusLength = fileBlock.UnitLength;
                    int reTryCount = 0;
                    while (surplusLength > 0)
                    {
                        if (disposable)
                        {
                            OutUpload();
                            return;
                        }
                        if (this.isPauseUpload)
                        {
                            waitHandleUpload.WaitOne();
                        }
                        byte[] positionBytes = BitConverter.GetBytes(this.uploadPosition);
                        long submitLength = surplusLength > (this.BufferLength - 21) ? (this.BufferLength - 21) : surplusLength;
                        byte[] submitLengthBytes = BitConverter.GetBytes(submitLength);
                        ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
                        if (this.uploadPosition + submitLength == fileBlock.StreamPosition + fileBlock.UnitLength)
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
                            if (this.TransferType != TransferType.Upload)
                            {
                                return;
                            }
                            if (FileBaseTool.ReadFileBytes(this.uploadFileBlocks.FileInfo.FilePath, this.uploadPosition, byteBlock, 21, (int)submitLength))
                            {
                                ByteBlock returnByteBlock = this.SendWait(1011, this.timeout, byteBlock);
                                if (returnByteBlock != null && returnByteBlock.Buffer[0] != 2)
                                {
                                    reTryCount = 0;
                                    this.tempSendLength += submitLength;
                                    this.uploadPosition += submitLength;
                                    surplusLength -= submitLength;

                                    if (returnByteBlock.Buffer[0] == 3)
                                    {
                                        this.SynchronizeTransferSetting();
                                    }
                                }
                                else if (returnByteBlock != null && returnByteBlock.Buffer[0] != 4)
                                {
                                    return;
                                }
                                else
                                {
                                    reTryCount++;
                                    Logger.Debug(LogType.Message, this, $"上传文件错误，正在尝试第{reTryCount}次重试");
                                    if (reTryCount > 10)
                                    {
                                        TransferFileMessageArgs args = new TransferFileMessageArgs();
                                        args.FileInfo = this.uploadFileBlocks.FileInfo;
                                        args.TransferType = TransferType.Upload;
                                        args.Message = "未知错误";
                                        TransferFileErrorMethod(this, args);
                                    }
                                }

                                if (returnByteBlock != null)
                                {
                                    returnByteBlock.SetHolding(false);
                                }
                            }
                            else
                            {
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

        private void DownloadFileFinished_s()
        {

            try
            {
                ByteBlock resultByteBlock = this.SendWait(1004, this.timeout);

                if (resultByteBlock != null)
                {
                    if (resultByteBlock.Position == 1 && resultByteBlock.Buffer[0] == 1)
                    {
                        FileBaseTool.FileFinished(downloadFileStream);
                        FileFinishedArgs args = new FileFinishedArgs();
                        args.FileInfo = downloadFileBlocks.FileInfo;
                        TransferFileHashDictionary.AddFile(downloadFileBlocks.FileInfo);

                        OutDownload(false);
                        DownloadFileFinishedMethod(this, args);
                    }
                    resultByteBlock.SetHolding(false);
                }
            }
            catch (Exception e)
            {
                TransferFileMessageArgs args = new TransferFileMessageArgs();
                args.FileInfo = this.DownloadFileInfo;
                args.TransferType = TransferType.Download;
                args.Message = e.Message;
                TransferFileErrorMethod(this, args);
            }
        }

        private void UploadFileFinished_s()
        {
            int reTryCount = 0;
            while (reTryCount < 10)
            {
                ByteBlock byteBlock = this.SendWait(1012, this.timeout);
                if (byteBlock != null)
                {
                    if (byteBlock.Buffer[0] == 1)
                    {
                        TransferFileStreamDic.DisposeFileStream(this.UploadFileInfo.FilePath);
                        FileFinishedArgs args = new FileFinishedArgs();
                        args.FileInfo = uploadFileBlocks.FileInfo;
                        this.uploadFileBlocks = null;
                        OutUpload();
                        UploadFileFinishedMethod(this, args);
                        break;
                    }
                    byteBlock.SetHolding(false);
                }

                reTryCount++;
            }
        }

        private void OutDownload(bool abort)
        {
            if (this.waitHandleDownload != null)
            {
                this.waitHandleDownload.Dispose();
            }
            if (this.downloadFileStream != null)
            {
                this.downloadFileStream.Dispose();
            }
            this.downloadFileBlocks = null;
            this.TransferType = TransferType.None;

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
            if (this.waitHandleUpload != null)
            {
                this.waitHandleUpload.Dispose();
            }

            this.uploadFileBlocks = null;
            this.TransferType = TransferType.None;

        }

        /// <summary>
        /// 发送Byte数组，并等待返回
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public byte[] SendBytesWaitReturn(byte[] data, int offset, int length, int waitTime = 3)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length - offset);
            byteBlock.Write(data, offset, length);
            ByteBlock resultByteBlock = this.SendWait(1014, this.timeout, byteBlock);
            if (resultByteBlock != null)
            {
                if (resultByteBlock.Buffer[0] == 1)
                {
                    byte[] buffer = new byte[resultByteBlock.Position - 1];
                    resultByteBlock.Position = 1;
                    resultByteBlock.Read(buffer, 0, buffer.Length);
                    resultByteBlock.SetHolding(false);
                    return buffer;
                }
                else
                {
                    throw new RRQMException("未知错误");
                }

            }
            else
            {
                throw new RRQMException("未知错误");
            }
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock)
        {
            if (byteBlock.Length > 16)
            {
                if (BitConverter.ToInt32(byteBlock.Buffer, 0) == 1000)
                {
                    if (BitConverter.ToInt32(byteBlock.Buffer, 4) == 1000)
                    {
                        if (BitConverter.ToInt32(byteBlock.Buffer, 8) == 1000)
                        {
                            if (BitConverter.ToInt32(byteBlock.Buffer, 12) == 1000)
                            {
                                ReceiveSystemMesMethod(this, new MesEventArgs(Encoding.UTF8.GetString(byteBlock.Buffer, 16, (int)byteBlock.Length - 16)));
                            }
                        }
                    }
                }
            }

            if (this.waitDataSend != null)
            {
                byteBlock.SetHolding(true);
                this.waitDataSend.Set(byteBlock);
            }
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="RRQMNotConnectedException"></exception>
        /// <exception cref="RRQMOverlengthException"></exception>
        /// <exception cref="RRQMException"></exception>
        public override void Send(byte[] buffer, int offset, int length)
        {
            throw new RRQMException("不允许发送自由数据");
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
                        AgreementHelper.SocketSend(agreement);
                    }
                    else
                    {
                        AgreementHelper.SocketSend(agreement, byteBlock.Buffer, 0, (int)byteBlock.Position);
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

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.TransferType == TransferType.Download)
            {
                this.OutDownload(true);
            }
            else if (this.TransferType == TransferType.Upload)
            {
                this.OutUpload();
            }

            this.disposable = true;
            this.TransferType = TransferType.None;
            this.downloadProgress = 0;
            this.uploadProgress = 0;
            this.downloadSpeed = 0;
            this.uploadSpeed = 0;
        }
    }
}