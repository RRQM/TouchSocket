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
using RRQMCore.ByteManager;
using RRQMCore.Log;
using RRQMCore.Serialization;
using RRQMSocket.RPC.RRQMRPC;
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
    public class FileSocketClient : RpcSocketClient, IFileService, IFileClient
    {
        /// <summary>
        /// 传输文件之前
        /// </summary>
        internal RRQMFileOperationEventHandler BeforeFileTransfer;

        /// <summary>
        /// 当文件传输完成时
        /// </summary>
        internal RRQMTransferFileMessageEventHandler FinishedFileTransfer;

        private long dataTransferLength;

        private long maxDownloadSpeed = 1024 * 1024;

        private long maxUploadSpeed = 1024 * 1024;

        private long position;

        private float progress;

        private string rrqmPath;

        private long speed;

        private Stopwatch stopwatch = new Stopwatch();

        private long tempLength;

        private long timeTick;

        private TransferStatus transferStatus;

        private UrlFileInfo transferUrlFileInfo;

        static FileSocketClient()
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
            for (short i = 121; i < 200; i++)
            {
                AddUsedProtocol(i, "保留协议");
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
                this.maxDownloadSpeed = value;
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
                this.maxUploadSpeed = value;
            }
        }

        /// <summary>
        /// 获取当前传输文件信息
        /// </summary>
        public UrlFileInfo TransferFileInfo { get => this.transferUrlFileInfo; }

        /// <summary>
        /// 获取当前传输进度
        /// </summary>
        public float TransferProgress
        {
            get
            {
                if (transferUrlFileInfo == null)
                {
                    return 0;
                }
                this.progress = transferUrlFileInfo.FileLength > 0 ? (float)position / transferUrlFileInfo.FileLength : 0;//计算下载完成进度
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
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.ResetVariable();
        }


        /// <summary>
        /// 文件辅助类处理其他协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void FileSocketClientHandleDefaultData(short? procotol, ByteBlock byteBlock)
        {
            this.OnHandleDefaultData(procotol, byteBlock);
        }

        /// <summary>
        /// 封装协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected sealed override void RPCHandleDefaultData(short? procotol, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;

            switch (procotol)
            {
                case 110:
                    {
                        try
                        {
                            //this.P110_SynchronizeTransferSetting();
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 112:
                    {
                        try
                        {
                            UrlFileInfo fileInfo = SerializeConvert.RRQMBinaryDeserialize<UrlFileInfo>(buffer, 2);
                            P112_RequestDownload(fileInfo);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 113:
                    {
                        try
                        {
                            P113_DownloadBlockData(byteBlock);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 114:
                    {
                        try
                        {
                            P114_StopDownload();
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 115:
                    {
                        try
                        {
                            P115_DownloadFinished();
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 116:
                    {
                        try
                        {
                            UrlFileInfo urlFileInfo = SerializeConvert.RRQMBinaryDeserialize<UrlFileInfo>(buffer, 2);
                            P116_RequestUpload(urlFileInfo);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 117:
                    {
                        try
                        {
                            P117_UploadBlockData(buffer);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 118:
                    {
                        try
                        {
                            P118_StopUpload();
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case 119:
                    {
                        try
                        {
                            P119_UploadFinished();
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }

                        break;
                    }
                default:
                    {
                        this.FileSocketClientHandleDefaultData(procotol, byteBlock);
                        break;
                    }
            }
        }

        /// <summary>
        /// 继承
        /// </summary>
        protected override void WaitReceive()
        {
            if (this.GetNowTick() - timeTick > 0)
            {
                //时间过了一秒
                this.timeTick = GetNowTick();
                this.dataTransferLength = 0;
                this.dataTransferLength = 0;
                stopwatch.Restart();
            }
            else
            {
                //在这一秒中
                switch (this.transferStatus)
                {
                    case TransferStatus.Upload:
                        if (this.dataTransferLength > this.maxUploadSpeed)
                        {
                            //上传饱和
                            stopwatch.Stop();
                            int sleepTime = 1000 - (int)stopwatch.ElapsedMilliseconds <= 0 ? 0 : 1000 - (int)stopwatch.ElapsedMilliseconds;
                            Thread.Sleep(sleepTime);
                        }
                        break;

                    case TransferStatus.Download:
                        if (this.dataTransferLength > this.maxDownloadSpeed)
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

        /// <summary>
        /// 获取当前时间帧
        /// </summary>
        /// <returns></returns>
        private long GetNowTick()
        {
            return DateTime.Now.Ticks / 10000000;
        }

        private FileOperationEventArgs OnBeforeFileTransfer(UrlFileInfo urlFileInfo)
        {
            FileOperationEventArgs args = new FileOperationEventArgs();
            args.UrlFileInfo = urlFileInfo;
            args.TransferType = urlFileInfo.TransferType;
            args.IsPermitOperation = true;
            if (urlFileInfo.TransferType == TransferType.Download)
            {
                //下载
                urlFileInfo.FilePath = Path.GetFullPath(urlFileInfo.FilePath);
            }

            try
            {
                this.BeforeFileTransfer?.Invoke(this, args);//触发 接收文件事件
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(BeforeFileTransfer)}中发生异常", ex);
            }

            if (urlFileInfo.TransferType == TransferType.Upload)
            {
                urlFileInfo.SaveFullPath = Path.GetFullPath(string.IsNullOrEmpty(urlFileInfo.SaveFullPath) ? urlFileInfo.FileName : urlFileInfo.SaveFullPath);
                this.rrqmPath = urlFileInfo.SaveFullPath + ".rrqm";
            }
            this.transferUrlFileInfo = urlFileInfo;
            return args;
        }

        private void OnFinishedFileTransfer(TransferType transferType)
        {
            if (!string.IsNullOrEmpty(this.transferUrlFileInfo.FileHash))
            {
                TransferFileHashDictionary.AddFile(this.transferUrlFileInfo);
            }
            TransferFileMessageArgs args = new TransferFileMessageArgs();
            args.UrlFileInfo = this.transferUrlFileInfo;
            args.TransferType = transferType;

            if (transferType == TransferType.Download)
            {
                FileStreamPool.DisposeReadStream(this.transferUrlFileInfo.FilePath);
            }
            else
            {
                FileStreamPool.DisposeWriteStream(this.rrqmPath, true);
                this.rrqmPath = null;
            }
            this.transferStatus = TransferStatus.None;
            this.transferUrlFileInfo = null;
            try
            {
                this.FinishedFileTransfer?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(FinishedFileTransfer)}中发生异常", ex);
            }
            this.ResetVariable();
        }

        private void P112_RequestDownload(UrlFileInfo urlFileInfo)
        {
            FileOperationEventArgs args = OnBeforeFileTransfer(urlFileInfo);
            FileWaitResult waitResult = new FileWaitResult();
            if (!args.IsPermitOperation)
            {
                waitResult.Message = string.IsNullOrEmpty(args.Message) ? "服务器拒绝下载" : args.Message;
                waitResult.Status = 2;
                this.transferStatus = TransferStatus.None;
            }
            else if (!File.Exists(urlFileInfo.FilePath))
            {
                waitResult.Message = $"路径{urlFileInfo.FilePath}文件不存在";
                waitResult.Status = 2;
                this.transferStatus = TransferStatus.None;
            }
            else
            {
                UrlFileInfo fileInfo;

                if (!TransferFileHashDictionary.GetFileInfo(urlFileInfo.FilePath, out fileInfo, urlFileInfo.Flags.HasFlag(TransferFlags.BreakpointResume)))
                {
                    fileInfo = TransferFileHashDictionary.AddFile(urlFileInfo.FilePath, urlFileInfo.Flags.HasFlag(TransferFlags.BreakpointResume));
                }
                urlFileInfo.CopyFrom(fileInfo);
                if (FileStreamPool.LoadReadStream(ref urlFileInfo, out string mes))
                {
                    this.transferStatus = TransferStatus.Download;
                    waitResult.Message = null;
                    waitResult.Status = 1;
                    this.transferUrlFileInfo = urlFileInfo;
                    waitResult.PBCollectionTemp = new PBCollectionTemp();
                    waitResult.PBCollectionTemp.UrlFileInfo = urlFileInfo;
                }
                else
                {
                    waitResult.Message = mes;
                    waitResult.Status = 2;
                }
            }

            this.SendDefaultObject(waitResult);
        }

        private void P113_DownloadBlockData(ByteBlock receivedByteBlock)
        {
            receivedByteBlock.Pos = 2;

            long position = receivedByteBlock.ReadInt64();
            int requestLength = receivedByteBlock.ReadInt32();
            ByteBlock byteBlock = this.BytePool.GetByteBlock(requestLength + 7);
            if (this.transferStatus != TransferStatus.Download)
            {
                byteBlock.Buffer[6] = 0;
            }

            string mes;
            if (FileStreamPool.ReadFile(transferUrlFileInfo.FilePath, out mes, position, byteBlock, 7, requestLength))
            {
                Speed.downloadSpeed += requestLength;
                this.position = position + requestLength;
                this.tempLength += requestLength;
                this.dataTransferLength += requestLength;
                byteBlock.Buffer[6] = 1;
            }
            else
            {
                byteBlock.Position = 6;
                byteBlock.Write((byte)2);
                byteBlock.Write(Encoding.UTF8.GetBytes(string.IsNullOrEmpty(mes) ? "未知错误" : mes));
            }

            try
            {
                if (this.Online)
                {
                    this.InternalSend(111, byteBlock.Buffer, 0, byteBlock.Len, true);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void P114_StopDownload()
        {
            this.transferUrlFileInfo = null;
            this.transferStatus = TransferStatus.None;
            this.InternalSend(111, new byte[] { 1 }, 0, 1);
        }

        private void P115_DownloadFinished()
        {
            if (this.transferStatus == TransferStatus.None)
            {
                this.InternalSend(111, new byte[] { 1 }, 0, 1);
                return;
            }
            this.InternalSend(111, new byte[] { 1 }, 0, 1);
            OnFinishedFileTransfer(TransferType.Download);
        }

        private void P116_RequestUpload(UrlFileInfo urlFileInfo)
        {
            FileOperationEventArgs args = OnBeforeFileTransfer(urlFileInfo);

            FileWaitResult waitResult = new FileWaitResult();
            if (!args.IsPermitOperation)
            {
                waitResult.Status = 2;
                waitResult.Message = string.IsNullOrEmpty(args.Message) ? "服务器拒绝上传" : args.Message;
            }
            else
            {
                this.transferStatus = TransferStatus.Upload;
                this.transferUrlFileInfo = urlFileInfo;
                if (urlFileInfo.Flags.HasFlag(TransferFlags.QuickTransfer) && TransferFileHashDictionary.GetFileInfoFromHash(urlFileInfo.FileHash, out UrlFileInfo oldUrlFileInfo))
                {
                    try
                    {
                        if (urlFileInfo.FilePath != oldUrlFileInfo.FilePath)
                        {
                            File.Copy(oldUrlFileInfo.FilePath, urlFileInfo.FilePath, true);
                        }

                        waitResult.Status = 3;
                        waitResult.Message = null;
                        this.SendDefaultObject(waitResult);
                        return;
                    }
                    catch (Exception ex)
                    {
                        waitResult.Status = 2;
                        waitResult.Message = ex.Message;
                        this.Logger.Debug(LogType.Error, this, "在处理快速上传时发生异常。", ex);
                    }
                }

                try
                {
                    ProgressBlockCollection blocks = ProgressBlockCollection.CreateProgressBlockCollection(urlFileInfo);

                    if (FileStreamPool.LoadWriteStream(ref blocks, false, out string mes))
                    {
                        waitResult.Status = 1;
                        waitResult.Message = null;
                        waitResult.PBCollectionTemp = PBCollectionTemp.GetFromProgressBlockCollection(blocks);
                    }
                    else
                    {
                        waitResult.Status = 2;
                        waitResult.Message = mes;
                    }
                }
                catch (Exception ex)
                {
                    waitResult.Status = 2;
                    waitResult.Message = ex.Message;
                    waitResult.PBCollectionTemp = null;
                }
            }

            this.SendDefaultObject(waitResult);
        }

        private void P117_UploadBlockData(byte[] buffer)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            try
            {
                if (this.transferStatus != TransferStatus.Upload)
                {
                    byteBlock.Write((byte)4);
                }
                byte status = buffer[2];
                int index = BitConverter.ToInt32(buffer, 3);
                long position = BitConverter.ToInt64(buffer, 7);
                int submitLength = BitConverter.ToInt32(buffer, 15);

                string mes;
                if (FileStreamPool.WriteFile(this.rrqmPath, out mes, out RRQMStream stream, position, buffer, 19, submitLength))
                {
                    this.position = position + submitLength;
                    this.tempLength += submitLength;
                    this.dataTransferLength += submitLength;
                    Speed.uploadSpeed += submitLength;
                    byteBlock.Write((byte)1);
                    if (status == 1)
                    {
                        FileBlock fileProgress = stream.Blocks.FirstOrDefault(a => a.Index == index);
                        fileProgress.RequestStatus = RequestStatus.Finished;
                        stream.SaveProgressBlockCollection();
                    }
                }
                else
                {
                    byteBlock.Write((byte)2);
                    byteBlock.Write(Encoding.UTF8.GetBytes(mes));
                    Logger.Debug(LogType.Error, this, $"上传文件写入错误，信息：{mes}");
                }
                this.InternalSend(111, byteBlock);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, "上传文件错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void P118_StopUpload()
        {
            try
            {
                FileStreamPool.SaveProgressBlockCollection(this.rrqmPath);
                FileStreamPool.DisposeWriteStream(this.rrqmPath, false);
                this.ResetVariable();
                this.InternalSend(111, new byte[] { 1 }, 0, 1);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, "客户端请求停止上传时发生异常", ex);
            }
        }

        private void P119_UploadFinished()
        {
            if (this.transferStatus == TransferStatus.None)
            {
                this.InternalSend(111, new byte[] { 1 }, 0, 1);
                return;
            }
            this.InternalSend(111, new byte[] { 1 }, 0, 1);
            OnFinishedFileTransfer(TransferType.Upload);
        }

        private void ResetVariable()
        {
            this.transferStatus = TransferStatus.None;
            this.transferUrlFileInfo = null;
            this.progress = 0;
            this.speed = 0;
            if (!string.IsNullOrEmpty(this.rrqmPath))
            {
                FileStreamPool.DisposeWriteStream(this.rrqmPath, false);
                this.rrqmPath = null;
            }
        }

        private void SendDefaultObject(object obj)
        {
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            byteBlock.Write(SerializeConvert.RRQMBinarySerialize(obj, true));
            try
            {
                this.InternalSend(111, byteBlock);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}