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
using RRQMCore.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 已接收的客户端
    /// </summary>
    public  class FileSocketClient : TcpSocketClient, IFileService, IFileClient
    {
        #region 属性

        private long maxDownloadSpeed = 1024 * 1024;

        private long maxUploadSpeed = 1024 * 1024;

        /// <summary>
        /// 发送文件信息
        /// </summary>
        public FileInfo DownloadFileInfo { get { return downloadFileBlocks == null ? null : downloadFileBlocks.FileInfo; } }

        /// <summary>
        /// 获取下载进度
        /// </summary>
        public float DownloadProgress
        {
            get
            {
                if (downloadFileBlocks != null)
                {
                    return downloadFileBlocks.FileInfo.FileLength != 0 ? (float)sendPosition / downloadFileBlocks.FileInfo.FileLength : 0;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 获取上传进度
        /// </summary>
        public float UploadProgress
        {
            get
            {
                if (uploadFileBlocks != null)
                {
                    return uploadFileBlocks.FileInfo.FileLength != 0 ? (float)receivePosition / uploadFileBlocks.FileInfo.FileLength : 0;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 每秒最大下载速度（Byte）,不可小于1024
        /// </summary>
        public long MaxDownloadSpeed
        {
            get { return maxDownloadSpeed; }
            set
            {
                if (value < 1024)
                {
                    value = 1024;
                }
                maxDownloadSpeed = value;
                MaxSpeedChanged(maxDownloadSpeed);
            }
        }

        /// <summary>
        /// 每秒最大上传速度（Byte）,不可小于1024
        /// </summary>
        public long MaxUploadSpeed
        {
            get { return maxUploadSpeed; }
            set
            {
                if (value < 1024)
                {
                    value = 1024;
                }
                maxUploadSpeed = value;
                MaxSpeedChanged(maxUploadSpeed);
            }
        }

        /// <summary>
        /// 获取传输类型
        /// </summary>
        public TransferType TransferType { get; private set; }

        /// <summary>
        /// 接收文件信息
        /// </summary>
        public FileInfo UploadFileInfo { get { return uploadFileBlocks == null ? null : uploadFileBlocks.FileInfo; } }

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

        /// <summary>
        /// 获取下载速度
        /// </summary>
        public long DownloadSpeed
        {
            get
            {
                return this.sendDataLength;
            }
        }

        /// <summary>
        /// 获取上传速度
        /// </summary>
        public long UploadSpeed
        {
            get
            {
                return this.receivedDataLength;
            }
        }

        #endregion 属性

        #region 字段
        internal RRQMAgreementHelper AgreementHelper;
        internal bool breakpointResume;
        private bool bufferLengthChanged;
        private long receivedDataLength;
        private long sendDataLength;
        private long sendPosition;
        private long receivePosition;
        private Stopwatch stopwatch = new Stopwatch();
        private long timeTick;
        private ProgressBlockCollection uploadFileBlocks;
        private ProgressBlockCollection downloadFileBlocks;
        private RRQMStream uploadFileStream;


        #endregion 字段

        #region 事件

        /// <summary>
        /// 刚开始接受文件的时候
        /// </summary>
        internal  RRQMTransferFileEventHandler BeforeReceiveFile;

        /// <summary>
        /// 开始发送文件
        /// </summary>
        internal RRQMTransferFileEventHandler BeforeSendFile;

        /// <summary>
        /// 当文件接收完成
        /// </summary>
        internal RRQMFileFinishedEventHandler ReceiveFileFinished;

        /// <summary>
        /// 当接收到系统信息的时候
        /// </summary>
        internal RRQMMessageEventHandler ReceiveSystemMes;

        /// <summary>
        /// 当文件发送完
        /// </summary>
        internal RRQMFileFinishedEventHandler SendFileFinished;

        /// <summary>
        /// 收到字节数组并返回
        /// </summary>
        internal RRQMBytesEventHandler ReceivedBytesThenReturn;

        /// <summary>
        /// 请求删除文件
        /// </summary>
        internal RRQMFileOperationEventHandler RequestDeleteFile;

        /// <summary>
        /// 请求文件信息
        /// </summary>
        internal RRQMFileOperationEventHandler RequestFileInfo;

        #endregion 事件
        private void MaxSpeedChanged(long speed)
        {
            if (speed < 1024 * 1024)
            {
                this.BufferLength = 1024 * 10;
            }
            else if (speed < 1024 * 1024 * 10)
            {
                this.BufferLength = 1024 * 64;
            }
            else if (speed < 1024 * 1024 * 50)
            {
                this.BufferLength = 1024 * 512;
            }
            else if (speed < 1024 * 1024 * 100)
            {
                this.BufferLength = 1024 * 1024;
            }
            else if (speed < 1024 * 1024 * 200)
            {
                this.BufferLength = 1024 * 1024 * 5;
            }
            else
            {
                this.BufferLength = 1024 * 1024 * 10;
            }
        }

        internal  override void WaitReceive()
        {
            if (this.GetNowTick() - timeTick > 0)
            {
                //时间过了一秒
                this.timeTick = GetNowTick();
                this.receivedDataLength = 0;
                this.sendDataLength = 0;
                stopwatch.Restart();
            }
            else
            {
                //在这一秒中
                switch (this.TransferType)
                {
                    case TransferType.Upload:
                        if (this.receivedDataLength > this.maxUploadSpeed)
                        {
                            //上传饱和
                            stopwatch.Stop();
                            int sleepTime = 1000 - (int)stopwatch.ElapsedMilliseconds <= 0 ? 0 : 1000 - (int)stopwatch.ElapsedMilliseconds;
                            Thread.Sleep(sleepTime);
                        }
                        break;

                    case TransferType.Download:
                        if (this.sendDataLength > this.maxDownloadSpeed)
                        {
                            //下载饱和
                            stopwatch.Stop();
                            int sleepTime = 1000 - (int)stopwatch.ElapsedMilliseconds <= 0 ? 0 : 1000 - (int)stopwatch.ElapsedMilliseconds;
                            Thread.Sleep(sleepTime);
                        }
                        break;
                }
            }
        }

        #region 协议函数

        private void RequestDownload(ByteBlock byteBlock, FileUrl url)
        {
            FileWaitResult waitResult = new FileWaitResult();
            if (!File.Exists(url.FilePath))
            {
                waitResult.Message = string.Format("文件：“{0}”不存在", url.FileName);
                waitResult.Status = 2;
                this.TransferType = TransferType.None;
            }
            else
            {
                FileInfo fileInfo;

                if (!TransferFileHashDictionary.GetFileInfo(url.FilePath, out fileInfo, breakpointResume))
                {
                    fileInfo = new FileInfo();
                    using (FileStream stream = File.OpenRead(url.FilePath))
                    {
                        fileInfo.FilePath = url.FilePath;
                        fileInfo.FileLength = stream.Length;
                        fileInfo.FileName = Path.GetFileName(url.FilePath);
                        if (this.breakpointResume)
                        {
                            fileInfo.FileHash = FileControler.GetStreamHash(stream);
                        }
                        TransferFileHashDictionary.AddFile(fileInfo);
                    }
                }

                TransferFileEventArgs args = new TransferFileEventArgs();
                args.FileInfo = fileInfo;
                args.FileInfo.Flag = url.Flag;
                args.IsPermitTransfer = true;
                args.TargetPath = args.FileInfo.FilePath;
                BeforeSendFile?.Invoke(this, args);

                this.TransferType = TransferType.Download;
                if (!args.IsPermitTransfer)
                {
                    waitResult.Message = string.Format("服务器拒绝下载--文件：“{0}”", fileInfo.FileName);
                    waitResult.Status = 2;
                    this.TransferType = TransferType.None;
                }
                else
                {
                    waitResult.Message = null;
                    waitResult.Status = 1;
                    downloadFileBlocks = FileBaseTool.GetProgressBlockCollection(fileInfo);
                    waitResult.PBCollectionTemp = PBCollectionTemp.GetFromProgressBlockCollection(downloadFileBlocks);
                }
            }

            byteBlock.Write(SerializeConvert.RRQMBinarySerialize(waitResult,true));
        }

        private void RequestUpload(ByteBlock byteBlock, PBCollectionTemp requestBlocks,bool restart)
        {
            FileWaitResult waitResult = new FileWaitResult();
            TransferFileEventArgs args = new TransferFileEventArgs();
            args.FileInfo = requestBlocks.FileInfo;
            args.TargetPath = requestBlocks.FileInfo.FileName;
            args.IsPermitTransfer = true;
            BeforeReceiveFile?.Invoke(this, args);//触发 接收文件事件
            requestBlocks.FileInfo.FilePath = args.TargetPath;

            if (!args.IsPermitTransfer)
            {
                waitResult.Status = 2;
                waitResult.Message = "服务器拒绝下载";
            }
            else if (FileControler.FileIsOpen(requestBlocks.FileInfo.FilePath))
            {
                waitResult.Status = 2;
                waitResult.Message = "该文件已被打开，或许正在由其他客户端上传";
            }
            else
            {
                this.TransferType = TransferType.Upload;

                restart = this.breakpointResume ? restart : true;
                if (!restart)
                {
                    FileInfo fileInfo;
                    if (TransferFileHashDictionary.GetFileInfoFromHash(requestBlocks.FileInfo.FileHash, out fileInfo))
                    {
                        try
                        {
                            if (fileInfo.FilePath != requestBlocks.FileInfo.FilePath)
                            {
                                File.Copy(fileInfo.FilePath, requestBlocks.FileInfo.FilePath);
                            }
                            uploadFileBlocks = FileBaseTool.GetProgressBlockCollection(fileInfo);
                            foreach (var item in uploadFileBlocks)
                            {
                                item.Finished = true;
                            }
                            waitResult.Status = 3;
                            waitResult.Message = null;

                            byteBlock.Write(SerializeConvert.RRQMBinarySerialize(waitResult,true));
                            return;
                        }
                        catch
                        {
                        }
                    }
                }
                try
                {
                    ProgressBlockCollection blocks = requestBlocks.ToPBCollection();
                    uploadFileStream = FileBaseTool.GetNewFileStream(ref blocks, restart);
                    blocks.FileInfo.FilePath = requestBlocks.FileInfo.FilePath;
                    this.uploadFileBlocks = blocks;
                    waitResult.Status = 1;
                    waitResult.Message = null;
                    waitResult.PBCollectionTemp =PBCollectionTemp.GetFromProgressBlockCollection(blocks) ;
                }
                catch (Exception ex)
                {
                    waitResult.Status = 2;
                    waitResult.Message = ex.Message;
                    waitResult.PBCollectionTemp = null;
                }
            }

            byteBlock.Write(SerializeConvert.RRQMBinarySerialize(waitResult,true));
        }

        private void DownloadBlockData(ByteBlock byteBlock, byte[] buffer)
        {
            if (this.TransferType != TransferType.Download)
            {
                byteBlock.Write(0);
                return;
            }
            try
            {
                long position = BitConverter.ToInt64(buffer, 4);
                long requestLength = BitConverter.ToInt64(buffer, 12);
                if (FileBaseTool.ReadFileBytes(downloadFileBlocks.FileInfo.FilePath, position, byteBlock, 1, (int)requestLength))
                {
                    Speed.downloadSpeed += requestLength;
                    this.sendPosition = position + requestLength;
                    this.sendDataLength += requestLength;
                    if (this.bufferLengthChanged)
                    {
                        byteBlock.Buffer[0] = 3;
                    }
                    else
                    {
                        byteBlock.Buffer[0] = 1;
                    }
                }
                else
                {
                    byteBlock.Buffer[0] = 2;
                }
            }
            catch
            {
            }

        }

        private void DownloadFinished(ByteBlock byteBlock)
        {
            TransferFileStreamDic.DisposeFileStream(this.DownloadFileInfo.FilePath);
            byteBlock.Write(1);
            FileFinishedArgs args = new FileFinishedArgs();
            args.FileInfo = this.downloadFileBlocks.FileInfo;
            this.TransferType = TransferType.None;
            SendFileFinished?.Invoke(this, args);
            this.downloadFileBlocks = null;
        }

        private void UploadBlockData(ByteBlock byteBlock, ByteBlock receivedbyteBlock)
        {
            if (this.TransferType!= TransferType.Upload)
            {
                byteBlock.Write(4);
                return;
            }
            byte status = receivedbyteBlock.Buffer[4];
            int index = BitConverter.ToInt32(receivedbyteBlock.Buffer, 5);
            long position = BitConverter.ToInt64(receivedbyteBlock.Buffer, 9);
            long submitLength = BitConverter.ToInt64(receivedbyteBlock.Buffer, 17);

            string mes;
            if (FileBaseTool.WriteFile(this.uploadFileStream, out mes, position, receivedbyteBlock.Buffer, 25, (int)submitLength))
            {
                this.receivePosition = position + submitLength;
                this.receivedDataLength += submitLength;
                Speed.uploadSpeed += submitLength;

                if (this.bufferLengthChanged)
                {
                    byteBlock.Write(3);
                }
                else
                {
                    byteBlock.Write(1);
                }

                if (status == 1)
                {
                    FileProgressBlock fileProgress = this.uploadFileBlocks.FirstOrDefault(a => a.Index == index);
                    fileProgress.Finished = true;
                    FileBaseTool.SaveProgressBlockCollection(this.uploadFileStream, this.uploadFileBlocks);
                }
            }
            else
            {
                byteBlock.Write(2);
                Logger.Debug(LogType.Error, this, "上传文件写入错误：" + mes);
            }
        }

        private void UploadFinished(ByteBlock byteBlock)
        {
            TransferFileHashDictionary.AddFile(this.UploadFileInfo);
            if (this.uploadFileStream != null)
            {
                FileBaseTool.FileFinished(this.uploadFileStream);
                this.uploadFileStream = null;
            }
            if (this.uploadFileBlocks != null)
            {
                TransferFileHashDictionary.AddFile(this.uploadFileBlocks.FileInfo);
                FileFinishedArgs args = new FileFinishedArgs();
                args.FileInfo = this.uploadFileBlocks.FileInfo;
                ReceiveFileFinished?.Invoke(this, args);

                this.uploadFileBlocks = null;
            }

            byteBlock.Write(1);
        }

        private void StopUpload(ByteBlock byteBlock)
        {
            FileBaseTool.SaveProgressBlockCollection(this.uploadFileStream, this.uploadFileBlocks);
            this.uploadFileStream.Close();
            this.uploadFileStream.Dispose();
            this.uploadFileStream = null;
            byteBlock.Write(1);
        }

        private void ReturnBytes(ByteBlock byteBlock, ByteBlock receivedByteBlock, int length)
        {
            BytesEventArgs args = new BytesEventArgs();
            byte[] buffer = new byte[length];
            byteBlock.Position = 4;
            receivedByteBlock.Read(buffer, 0, buffer.Length);
            args.ReceivedDataBytes = buffer;
            ReceivedBytesThenReturn?.Invoke(this, args);
            byteBlock.Write(1);
            if (args.ReturnDataBytes != null)
            {
                byteBlock.Write(args.ReturnDataBytes);
            }
        }

        private void RDeleteFile(ByteBlock byteBlock,FileUrl url)
        {
            if (!File.Exists(url.FilePath))
            {
                byteBlock.Write(2);
                return;
            }
            OperationFileEventArgs args = new OperationFileEventArgs();
            args.FileInfo = new FileInfo();
            args.FileInfo.FilePath = url.FilePath;
            args.FileInfo.Flag = url.Flag;
            this.RequestDeleteFile?.Invoke(this, args);
            if (!args.IsPermitOperation)
            {
                byteBlock.Write(3);
                return;
            }
            try
            {
                File.Delete(args.FileInfo.FilePath);
                byteBlock.Write(1);
            }
            catch (Exception ex)
            {
                byteBlock.Write(4);
                byteBlock.Write(Encoding.UTF8.GetBytes(ex.Message));
            }
            
        } 
        
        private void RFileInfo(ByteBlock byteBlock,FileUrl url)
        {
            if (!File.Exists(url.FilePath))
            {
                byteBlock.Write(2);
                return;
            }
            OperationFileEventArgs args = new OperationFileEventArgs();
            args.FileInfo = new FileInfo();
            args.FileInfo.FilePath = url.FilePath;
            args.FileInfo.Flag = url.Flag;
            this.RequestFileInfo?.Invoke(this, args);
            if (!args.IsPermitOperation)
            {
                byteBlock.Write(3);
                return;
            }
            try
            {
                FileInfo fileInfo = new FileInfo();
                using (Stream stream=File.Open(args.FileInfo.FilePath,FileMode.Open))
                {
                    fileInfo.FileLength = stream.Length;
                    fileInfo.FileName = Path.GetFileName(args.FileInfo.FilePath);
                    fileInfo.FilePath = args.FileInfo.FilePath;
                }
                byteBlock.Write(1);
                byteBlock.Write(SerializeConvert.RRQMBinarySerialize(fileInfo,true));
            }
            catch (Exception ex)
            {
                byteBlock.Write(4);
                byteBlock.Write(Encoding.UTF8.GetBytes(ex.Message));
            }
            
        }
        private void SystemMessage(string mes)
        {
            ReceiveSystemMes?.Invoke(this, new MesEventArgs(mes));
        }

        #endregion 协议函数

        /// <summary>
        /// 当BufferLength改变值的时候
        /// </summary>
        protected override void OnBufferLengthChanged()
        {
            bufferLengthChanged = true;
        }

        /// <summary>
        /// 获取当前时间帧
        /// </summary>
        /// <returns></returns>
        private long GetNowTick()
        {
            long tick = (long)(DateTime.Now.Ticks / 10000000.0);
            return tick;
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

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="mes"></param>
        /// <exception cref="RRQMException"></exception>
        public void SendSystemMes(string mes)
        {
            if (mes == null || mes == string.Empty)
            {
                throw new RRQMException("消息不可为空");
            }
            byte[] datas = Encoding.UTF8.GetBytes(mes);
            ByteBlock byteBlock = this.BytePool.GetByteBlock(datas.Length + 12);
            try
            {
                byteBlock.Write(BitConverter.GetBytes(1000));
                byteBlock.Write(BitConverter.GetBytes(1000));
                byteBlock.Write(BitConverter.GetBytes(1000));
                byteBlock.Write(datas);
                AgreementHelper.SocketSend(1000, byteBlock);
            }
            catch (Exception ex)
            {
                throw new RRQMException(ex.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 处理已接收到的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
            int agreement = BitConverter.ToInt32(buffer, 0);
            ByteBlock returnByteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            switch (agreement)
            {
                case 1000:
                    {
                        try
                        {
                            string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 4, r - 4);
                            SystemMessage(mes);
                            returnByteBlock.Write(1);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }
                case 1001:
                    {
                        try
                        {
                            FileUrl url = SerializeConvert.RRQMBinaryDeserialize<FileUrl>(buffer, 4);
                            RequestDownload(returnByteBlock, url);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }

                        break;
                    }

                case 1002:
                    {
                        try
                        {
                            DownloadBlockData(returnByteBlock, buffer);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }
                case 1003://停止下载
                    {
                        try
                        {
                            this.downloadFileBlocks = null;
                            this.TransferType = TransferType.None;
                            returnByteBlock.Write(1);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }
                case 1004:
                    {
                        try
                        {
                            DownloadFinished(returnByteBlock);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }

                case 1010:
                    {
                        try
                        {
                            bool restart = BitConverter.ToBoolean(byteBlock.Buffer,4);
                            PBCollectionTemp blocks = SerializeConvert.RRQMBinaryDeserialize<PBCollectionTemp>(byteBlock.Buffer, 5);
                            RequestUpload(returnByteBlock, blocks, restart);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }

                        break;
                    }

                case 1011:
                    {
                        try
                        {
                            UploadBlockData(returnByteBlock, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }

                        break;
                    }

                case 1012:
                    {
                        try
                        {
                            UploadFinished(returnByteBlock);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }
                case 1013:
                    {
                        try
                        {
                            StopUpload(returnByteBlock);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }

                        break;
                    }

                case 1014:
                    {
                        try
                        {
                            ReturnBytes(returnByteBlock, byteBlock, r - 4);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }

                case 1020:
                    {
                        try
                        {
                            if (this.TransferType == TransferType.Download)
                            {
                                this.MaxSpeedChanged(this.maxDownloadSpeed);
                            }
                            else
                            {
                                this.MaxSpeedChanged(this.maxUploadSpeed);
                            }
                            TransferSetting transferSetting = new TransferSetting();
                            transferSetting.breakpointResume = this.breakpointResume;
                            transferSetting.bufferLength = this.BufferLength;
                            transferSetting.Serialize(returnByteBlock);
                            this.bufferLengthChanged = false;
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }

                case 1021:
                    {
                        try
                        {
                            FileUrl url = SerializeConvert.RRQMBinaryDeserialize<FileUrl>(byteBlock.Buffer,4);
                            this.RDeleteFile(returnByteBlock,url);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    } 
                
                case 1022:
                    {
                        try
                        {
                            FileUrl url = SerializeConvert.RRQMBinaryDeserialize<FileUrl>(byteBlock.Buffer,4);
                            this.RFileInfo(returnByteBlock,url);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
                        }
                        break;
                    }
            }

            try
            {
                this.AgreementHelper.SocketSend(returnByteBlock);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, ex.Message, ex.StackTrace);
            }
            finally
            {
                returnByteBlock.Dispose();
            }
        }


        /// <summary>
        ///
        /// </summary>
        public override void Recreate()
        {
            base.Recreate();
            this.maxDownloadSpeed = 1024 * 1024;
            this.maxUploadSpeed = 1024 * 1024;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (uploadFileStream != null)
            {
                uploadFileStream.Dispose();
            }
        }
    }
}