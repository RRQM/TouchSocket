//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using RRQMSocket.Extensions;
using RRQMSocket.RPC.RRQMRPC;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 已接收的客户端
    /// </summary>
    public class FileSocketClient : RpcSocketClient, IFileClientBase
    {
        private ConcurrentDictionary<int, FileOperationEventArgs> eventArgs;

        private ResponseType responseType;

        private string rootPath = string.Empty;
        internal RRQMFileOperationEventHandler<FileSocketClient> internalFileTransfering;
        internal RRQMTransferFileEventHandler<FileSocketClient> internalFileTransfered;

        /// <summary>
        /// 构造函数
        /// </summary>
        public FileSocketClient()
        {
            this.eventArgs = new ConcurrentDictionary<int, FileOperationEventArgs>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ResponseType ResponseType
        {
            get => this.responseType;
            set => this.responseType = value;
        }

        /// <summary>
        /// 根路径
        /// </summary>
        public string RootPath
        {
            get => this.rootPath;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                this.rootPath = value;
            }
        }

        /// <summary>
        /// 从对点拉取文件
        /// </summary>
        public Result PullFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            if (fileRequest is null)
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetResString(nameof(fileRequest)));
            }

            if (fileOperator is null)
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetResString(nameof(fileOperator)));
            }

            if (string.IsNullOrEmpty(fileRequest.SavePath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetResString(nameof(fileRequest.SavePath), fileRequest.SavePath)));
            }

            if (!fileRequest.Overwrite)
            {
                if (File.Exists(fileRequest.SavePath))
                {
                    return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.FileExists.GetResString(fileRequest.SavePath)));
                }
            }

            WaitFileInfo waitFileInfo = new WaitFileInfo();
            waitFileInfo.PackageSize = fileOperator.PackageSize;
            waitFileInfo.Metadata = metadata;
            waitFileInfo.FileRequest = fileRequest;

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitFileInfo);

            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength).WriteObject(waitFileInfo, SerializationType.Json);
            try
            {
                this.InternalSend(203, byteBlock.Buffer, 0, byteBlock.Len);
                waitData.SetCancellationToken(fileOperator.Token);

                waitData.Wait(60 * 1000);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitFileInfo waitFileResult = (WaitFileInfo)waitData.WaitResult;
                            if (waitFileResult.Status == 1)
                            {
                                return this.OnPreviewPullFile(fileOperator, waitFileResult);
                            }
                            else if (waitFileResult.Status == 2)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetResString()));
                            }
                            else if (waitFileResult.Status == 3)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetResString(nameof(waitFileResult.FileRequest.Path), waitFileResult.FileRequest.Path)));
                            }
                            else if (waitFileResult.Status == 4)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteFileNotExists.GetResString(waitFileResult.FileRequest.Path)));
                            }
                            else if (waitFileResult.Status == 6)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteNotSupported.GetResString()));
                            }
                            else
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, waitFileResult.Message));
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return fileOperator.SetFileResult(new Result(ResultCode.Overtime));
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return fileOperator.SetFileResult(new Result(ResultCode.Error));
                        }
                }
            }
            catch (Exception ex)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<Result> PullFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return await Task.Run(() =>
            {
                return this.PullFile(fileRequest, fileOperator, metadata);
            });
        }

        /// <summary>
        /// 将文件推送到对点
        /// </summary>
        public Result PushFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            if (fileRequest is null)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ArgumentNull.GetResString(nameof(fileRequest))));
            }

            if (fileOperator is null)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ArgumentNull.GetResString(nameof(fileOperator))));
            }

            if (string.IsNullOrEmpty(fileRequest.SavePath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetResString(nameof(fileRequest.SavePath), fileRequest.SavePath)));
            }
            string fullPath;
            if (Path.IsPathRooted(fileRequest.Path))
            {
                fullPath = fileRequest.Path;
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(this.rootPath, fileRequest.Path));
            }
            if (!File.Exists(fullPath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetResString(nameof(fullPath), fullPath)));
            }

            RRQMFileInfo fileInfo;
            try
            {
                fileInfo = FileTool.GetFileInfo(fullPath, fileRequest.FileCheckerType);
            }
            catch (Exception ex)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
            }

            WaitFileInfo waitFileInfo = new WaitFileInfo();
            waitFileInfo.FileInfo = fileInfo;
            waitFileInfo.PackageSize = fileOperator.PackageSize;
            waitFileInfo.Metadata = metadata;
            waitFileInfo.FileRequest = fileRequest;

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitFileInfo);

            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength).WriteObject(waitFileInfo, SerializationType.Json);
            LoopAction loopAction = null;
            try
            {
                this.InternalSend(205, byteBlock.Buffer, 0, byteBlock.Len);
                waitData.SetCancellationToken(fileOperator.Token);

                waitData.Wait(60 * 1000);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitTransfer waitResult = (WaitTransfer)waitData.WaitResult;
                            if (waitResult.Status == 1)
                            {
                                return this.OnPreviewPushFile(fileOperator, waitResult);
                            }
                            else if (waitResult.Status == 2)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetResString()));
                            }
                            else if (waitResult.Status == 3)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.FileExists.GetResString(waitResult.Path)));
                            }
                            else if (waitResult.Status == 4)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetResString(waitResult.Path)));
                            }
                            else if (waitResult.Status == 6)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteNotSupported.GetResString()));
                            }
                            else
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, waitResult.Message));
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return fileOperator.SetFileResult(new Result(ResultCode.Overtime));
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return fileOperator.SetFileResult(new Result(ResultCode.Error));
                        }
                }
            }
            catch (Exception ex)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Overtime, ex.Message));
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                if (loopAction != null)
                {
                    loopAction.Dispose();
                }
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> PushFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return Task.Run(() =>
            {
                return this.PushFile(fileRequest, fileOperator, metadata);
            });
        }

        /// <summary>
        /// 文件终端处理其他协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void FileTransferHandleDefaultData(short protocol, ByteBlock byteBlock)
        {
            this.OnReceived(protocol, byteBlock);
        }

        /// <summary>
        /// 在传输之前
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileTransfering(FileOperationEventArgs e)
        {
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IFilePlugin>("OnFileTransfering",this,e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.internalFileTransfering?.Invoke(this, e);
        }

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileTransfered(FileTransferStatusEventArgs e)
        {
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IFilePlugin>("OnFileTransfered", this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.internalFileTransfered?.Invoke(this, e);
        }

        /// <summary>
        /// 封装协议
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected override sealed void HandleRpcDefaultData(short protocol, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;

            switch (protocol)
            {
                case FileUtility.P200:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>(SerializationType.Json);

                            EasyAction.TaskRun(waitFileInfo, (w) =>
                            {
                                this.SendDefaultObject(FileUtility.P200, this.P200_RequestPullFile(w));
                            });
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P201:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>(SerializationType.Json);
                            EasyAction.TaskRun(waitTransfer, (w) =>
                             {
                                 this.P201_BeginPullFile(w);
                             });
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P202:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>(SerializationType.Json);
                            EasyAction.TaskRun(waitFileInfo, (w) =>
                            {
                                this.P202_RequestPushFile(w);
                            });
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P203:
                    {
                        byteBlock.Pos = 2;
                        WaitFileInfo waitFile = byteBlock.ReadObject<WaitFileInfo>(SerializationType.Json);
                        this.WaitHandlePool.SetRun(waitFile);
                        break;
                    }
                case FileUtility.P204:
                    {
                        byteBlock.Pos = 2;
                        WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>(SerializationType.Json);
                        this.WaitHandlePool.SetRun(waitTransfer);
                        break;
                    }
                case FileUtility.P205:
                    {
                        byteBlock.Pos = 2;
                        WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>(SerializationType.Json);
                        this.WaitHandlePool.SetRun(waitTransfer);
                        break;
                    }
                case FileUtility.P206:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>(SerializationType.Json);

                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.Service.SocketClients.TryGetSocketClient(waitFileInfo.ClientID, out FileSocketClient client))
                                {
                                    waitFileInfo.ClientID = this.ID;
                                    client.InternalSend(FileUtility.P207, block.WriteObject(waitFileInfo, SerializationType.Json));
                                }
                                else
                                {
                                    this.InternalSend(FileUtility.P206, block.WriteObject(new WaitTransfer() { Sign = waitFileInfo.Sign, Status = 7 }, SerializationType.Json));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P207:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>(SerializationType.Json);

                            if (this.Service.SocketClients.TryGetSocketClient(waitTransfer.ClientID, out FileSocketClient client))
                            {
                                using (ByteBlock block = new ByteBlock())
                                {
                                    waitTransfer.ClientID = this.ID;
                                    client.InternalSend(FileUtility.P206, block.WriteObject(waitTransfer, SerializationType.Json));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P208:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>(SerializationType.Json);

                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.Service.SocketClients.TryGetSocketClient(waitFileInfo.ClientID, out FileSocketClient client))
                                {
                                    waitFileInfo.ClientID = this.ID;
                                    client.InternalSend(FileUtility.P209, block.WriteObject(waitFileInfo, SerializationType.Json));
                                }
                                else
                                {
                                    waitFileInfo.Status = 7;
                                    this.InternalSend(FileUtility.P208, block.WriteObject(waitFileInfo, SerializationType.Json));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P209:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>(SerializationType.Json);

                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.Service.SocketClients.TryGetSocketClient(waitFileInfo.ClientID, out FileSocketClient client))
                                {
                                    waitFileInfo.ClientID = this.ID;
                                    client.InternalSend(FileUtility.P208, block.WriteObject(waitFileInfo, SerializationType.Json));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P210:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>(SerializationType.Json);
                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.Service.SocketClients.TryGetSocketClient(waitTransfer.ClientID, out FileSocketClient client))
                                {
                                    waitTransfer.ClientID = this.ID;
                                    client.InternalSend(FileUtility.P211, block.WriteObject(waitTransfer, SerializationType.Json));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }
                case FileUtility.P211:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>(SerializationType.Json);
                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.Service.SocketClients.TryGetSocketClient(waitTransfer.ClientID, out FileSocketClient client))
                                {
                                    client.InternalSend(FileUtility.P210, block.WriteObject(waitTransfer, SerializationType.Json));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                        }
                        break;
                    }

                default:
                    {
                        this.FileTransferHandleDefaultData(protocol, byteBlock);
                        break;
                    }
            }
        }

        private Result OnPreviewPullFile(FileOperator fileOperator, WaitFileInfo waitFileInfo)
        {
            FileRequest fileRequest = waitFileInfo.FileRequest;
            string savePath;
            if (Path.IsPathRooted(fileRequest.SavePath))
            {
                savePath = fileRequest.SavePath + ".rrqm";
            }
            else
            {
                savePath = Path.GetFullPath(Path.Combine(this.rootPath, fileRequest.SavePath)) + ".rrqm";
            }
            RRQMFileInfo remoteFileInfo = waitFileInfo.FileInfo;

            if (RRQMStreamPool.LoadWriteStream(savePath, fileOperator, waitFileInfo.FileRequest, ref remoteFileInfo, out RRQMStream stream, out string mes))
            {
                Channel channel = this.CreateChannel();

                WaitTransfer waitTransfer = new WaitTransfer()
                {
                    PackageSize = waitFileInfo.PackageSize,
                    EventHashCode = waitFileInfo.EventHashCode,
                    ChannelID = channel.ID,
                    Path = remoteFileInfo.FilePath,
                    Position = remoteFileInfo.Posotion
                };

                WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitTransfer);
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength).WriteObject(waitTransfer, SerializationType.Json);
                try
                {
                    this.InternalSend(204, byteBlock.Buffer, 0, byteBlock.Len);
                    waitData.SetCancellationToken(fileOperator.Token);

                    waitData.Wait(60 * 1000);

                    switch (waitData.Status)
                    {
                        case WaitDataStatus.SetRunning:
                            {
                                WaitTransfer waitTransferResult = (WaitTransfer)waitData.WaitResult;
                                if (waitTransferResult.Status == 1)
                                {
                                    stream.Position = remoteFileInfo.Posotion;
                                    fileOperator.SetFileCompletedLength(remoteFileInfo.Posotion);
                                    while (channel.MoveNext())
                                    {
                                        if (fileOperator.Token.IsCancellationRequested)
                                        {
                                            channel.Cancel();
                                            fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                                            break;
                                        }
                                        ByteBlock block = channel.GetCurrentByteBlock();
                                        block.Pos = 6;

                                        if (block.TryReadBytesPackageInfo(out int pos, out int len))
                                        {
                                            stream.Write(block.Buffer, pos, len);
                                            stream.fileInfo.Posotion = stream.Position;
                                            fileOperator.AddFileFlow(len, remoteFileInfo.FileLength);
                                            stream.SaveProgress();
                                        }
                                        block.SetHolding(false);
                                    }

                                    if (channel.Status == ChannelStatus.Completed)
                                    {
                                        stream.FinishStream(fileRequest.Overwrite);
                                        return fileOperator.SetFileResult(new Result(ResultCode.Success));
                                    }
                                    else
                                    {
                                        return fileOperator.SetFileResult(new Result(channel.Status.ToResultCode()));
                                    }
                                }
                                else if (waitTransferResult.Status == 2)
                                {
                                    return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetResString(remoteFileInfo.FilePath)));
                                }
                                else if (waitTransferResult.Status == 3)
                                {
                                    return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.GetEventArgsFail.GetResString()));
                                }
                                else if (waitTransferResult.Status == 4)
                                {
                                    return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.SetChannelFail.GetResString()));
                                }
                                else
                                {
                                    return fileOperator.SetFileResult(new Result(ResultCode.Error));
                                }
                            }
                        case WaitDataStatus.Overtime:
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Overtime));
                            }
                        case WaitDataStatus.Canceled:
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                            }
                        case WaitDataStatus.Disposed:
                        default:
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error));
                            }
                    }
                }
                catch (Exception ex)
                {
                    return fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
                }
                finally
                {
                    this.WaitHandlePool.Destroy(waitData);
                    RRQMStreamPool.TryReleaseWriteStream(savePath, fileOperator);
                    byteBlock.Dispose();
                    channel.Dispose();
                }
            }
            else
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.CreateWriteStreamFail.GetResString(savePath, mes)));
            }
        }

        private Result OnPreviewPushFile(FileOperator fileOperator, WaitTransfer waitTransfer)
        {
            if (RRQMStreamPool.LoadReadStream(waitTransfer.Path, out RRQMFileInfo fileInfo, out string mes))
            {
                if (this.TrySubscribeChannel(waitTransfer.ChannelID, out Channel channel))
                {
                    ByteBlock byteBlock = BytePool.GetByteBlock(waitTransfer.PackageSize);
                    try
                    {
                        long position = waitTransfer.Position;
                        fileOperator.SetFileCompletedLength(waitTransfer.Position);
                        while (true)
                        {
                            if (fileOperator.Token.IsCancellationRequested)
                            {
                                channel.Cancel("主动取消");
                                fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                                break;
                            }
                            int r = RRQMStreamPool.ReadBytes(waitTransfer.Path, out mes, position, byteBlock.Buffer, 0, byteBlock.Capacity);
                            if (r == -1)
                            {
                                channel.Cancel(mes);
                                break;
                            }
                            else if (r == 0)
                            {
                                channel.Complete();
                                break;
                            }
                            else
                            {
                                position += r;
                                if (channel.TryWrite(byteBlock.Buffer, 0, r))
                                {
                                    fileOperator.AddFileFlow(r, fileInfo.FileLength);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (channel.Status == ChannelStatus.Cancel && !string.IsNullOrEmpty(channel.LastOperationMes))
                        {
                            return fileOperator.SetFileResult(new Result(ResultCode.Canceled, channel.LastOperationMes));
                        }
                        else
                        {
                            return fileOperator.SetFileResult(new Result(channel.Status.ToResultCode()));
                        }
                    }
                    catch (Exception ex)
                    {
                        return fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
                    }
                    finally
                    {
                        RRQMStreamPool.FinishedReadStream(waitTransfer.Path);
                        RRQMStreamPool.TryReleaseReadStream(waitTransfer.Path);
                        byteBlock.Dispose();
                    }
                }
                else
                {
                    return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.SetChannelFail.GetResString()));
                }
            }
            else
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetResString()));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnClose()
        {
            this.internalFileTransfering = null;
            this.internalFileTransfered = null;
            base.OnClose();
        }

        /// <summary>
        /// 客户端请求下拉文件
        /// </summary>
        private WaitFileInfo P200_RequestPullFile(WaitFileInfo waitFileInfo)
        {
            //2.不允许
            //3.路径无效
            //4.文件不存在
            //5.其他

            if (this.responseType == ResponseType.None || this.responseType == ResponseType.Push)
            {
                waitFileInfo.Status = 6;
                return waitFileInfo;
            }
            string fullPath;

            if (Path.IsPathRooted(waitFileInfo.FileRequest.Path))
            {
                fullPath = waitFileInfo.FileRequest.Path;
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(this.rootPath, waitFileInfo.FileRequest.Path));
            }
            waitFileInfo.FileRequest.Path = fullPath;

            FileOperationEventArgs args = new FileOperationEventArgs(TransferType.Pull, waitFileInfo.FileRequest, new FileOperator(), waitFileInfo.Metadata, null);
            this.OnFileTransfering(args);
            fullPath = waitFileInfo.FileRequest.Path;
            if (string.IsNullOrEmpty(fullPath))
            {
                waitFileInfo.Status = 3;
                return waitFileInfo;
            }

            if (args.IsPermitOperation)
            {
                if (File.Exists(fullPath))
                {
                    //合法传输
                    waitFileInfo.FileInfo = FileTool.GetFileInfo(fullPath, args.FileRequest.FileCheckerType);
                    waitFileInfo.Status = 1;
                    waitFileInfo.EventHashCode = args.GetHashCode();

                    this.eventArgs.TryAdd(args.GetHashCode(), args);
                    RRQMCore.Run.EasyAction.DelayRun(1000 * 60, () =>
                       {
                           if (this.eventArgs.TryRemove(args.GetHashCode(), out FileOperationEventArgs eventArgs))
                           {
                               FileTransferStatusEventArgs e = new FileTransferStatusEventArgs(
                                    TransferType.Pull,
                                    eventArgs.FileRequest,
                                    eventArgs.Metadata,
                                    new Result(ResultCode.Overtime, ResType.NoResponse.GetResString()), null);
                               this.OnFileTransfered(e);
                           }
                       });
                    return waitFileInfo;
                }
                else
                {
                    waitFileInfo.Status = 4;
                    return waitFileInfo;
                }
            }
            else
            {
                waitFileInfo.Status = 2;
                return waitFileInfo;
            }
        }

        private void P201_BeginPullFile(WaitTransfer waitTransfer)
        {
            //2.加载流异常
            //3.事件操作器异常
            //4.通道建立异常

            FileTransferStatusEventArgs e;
            if (this.eventArgs.TryRemove(waitTransfer.EventHashCode, out FileOperationEventArgs args))
            {
                if (RRQMStreamPool.LoadReadStream(waitTransfer.Path, out RRQMFileInfo fileInfo, out string mes))
                {
                    if (this.TrySubscribeChannel(waitTransfer.ChannelID, out Channel channel))
                    {
                        ByteBlock byteBlock = BytePool.GetByteBlock(waitTransfer.PackageSize);
                        FileOperator fileOperator = args.FileOperator;
                        try
                        {
                            waitTransfer.Status = 1;
                            this.SendDefaultObject(201, waitTransfer);
                            long position = waitTransfer.Position;
                            while (true)
                            {
                                if (fileOperator.Token.IsCancellationRequested)
                                {
                                    channel.Cancel("主动取消");
                                    fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                                    break;
                                }
                                int r = RRQMStreamPool.ReadBytes(waitTransfer.Path, out mes, position, byteBlock.Buffer, 0, byteBlock.Capacity);
                                if (r == -1)
                                {
                                    channel.Cancel(mes);
                                    break;
                                }
                                else if (r == 0)
                                {
                                    channel.Complete();
                                    break;
                                }
                                else
                                {
                                    position += r;
                                    if (channel.TryWrite(byteBlock.Buffer, 0, r))
                                    {
                                        fileOperator.AddFileFlow(r, fileInfo.FileLength);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            RRQMStreamPool.FinishedReadStream(waitTransfer.Path);
                            RRQMStreamPool.TryReleaseReadStream(waitTransfer.Path);
                            byteBlock.Dispose();

                            e = new FileTransferStatusEventArgs(TransferType.Pull, args.FileRequest, args.Metadata,
                               fileOperator.SetFileResult(new Result(channel.Status.ToResultCode())), null);
                            this.OnFileTransfered(e);
                        }

                        return;
                    }
                    else
                    {
                        waitTransfer.Status = 4;
                        e = new FileTransferStatusEventArgs(TransferType.Pull,
                            args.FileRequest, args.Metadata, new Result(ResultCode.Error, ResType.SetChannelFail.GetResString()), null);
                    }
                }
                else
                {
                    waitTransfer.Status = 2;
                    waitTransfer.Message = mes;
                    e = new FileTransferStatusEventArgs(TransferType.Pull,
                         args.FileRequest, args.Metadata, new Result(ResultCode.Error, ResType.LoadStreamFail.GetResString()), null);
                }
            }
            else
            {
                e = new FileTransferStatusEventArgs(TransferType.Pull, null, null,
                    new Result(ResultCode.Overtime, ResType.GetEventArgsFail.GetResString()), null);
                waitTransfer.Status = 3;
            }

            this.SendDefaultObject(201, waitTransfer);

            this.OnFileTransfered(e);
        }

        /// <summary>
        /// 客户端请求推送文件
        /// </summary>
        private void P202_RequestPushFile(WaitFileInfo waitRemoteFileInfo)
        {
            //2.不允许
            //3.文件已存在
            //4.加载流异常
            //5.其他

            if (this.responseType == ResponseType.None || this.responseType == ResponseType.Pull)
            {
                this.SendDefaultObject(202, new WaitTransfer() { Sign = waitRemoteFileInfo.Sign, Status = 6 });
                return;
            }

            string savePath;
            if (Path.IsPathRooted(waitRemoteFileInfo.FileRequest.SavePath))
            {
                savePath = waitRemoteFileInfo.FileRequest.SavePath;
            }
            else
            {
                savePath = Path.GetFullPath(Path.Combine(this.rootPath, waitRemoteFileInfo.FileRequest.SavePath));
            }
            waitRemoteFileInfo.FileRequest.SavePath = savePath;

            FileOperator fileOperator = new FileOperator() { PackageSize = waitRemoteFileInfo.PackageSize };
            FileOperationEventArgs args = new FileOperationEventArgs(TransferType.Push, waitRemoteFileInfo.FileRequest,
                fileOperator, waitRemoteFileInfo.Metadata, waitRemoteFileInfo.FileInfo);
            this.OnFileTransfering(args);

            savePath = waitRemoteFileInfo.FileRequest.SavePath;

            WaitTransfer waitTransfer = new WaitTransfer()
            {
                Message = args.Message,
                PackageSize = args.FileOperator.PackageSize,
                Sign = waitRemoteFileInfo.Sign,
                Path = args.FileRequest.Path,
            };

            if (args.IsPermitOperation)
            {
                if (!args.FileRequest.Overwrite)
                {
                    if (File.Exists(args.FileRequest.SavePath))
                    {
                        waitTransfer.Status = 3;
                        this.SendDefaultObject(202, waitTransfer);
                        fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.FileExists.GetResString(args.FileRequest.SavePath)));
                        return;
                    }
                }

                savePath = savePath + ".rrqm";

                RRQMFileInfo fileInfo = args.FileInfo;
                if (RRQMStreamPool.LoadWriteStream(savePath, args.FileOperator, args.FileRequest, ref fileInfo, out RRQMStream stream, out string mes))
                {
                    Channel channel = this.CreateChannel();
                    try
                    {
                        waitTransfer.ChannelID = channel.ID;
                        waitTransfer.Position = fileInfo.Posotion;
                        waitTransfer.Status = 1;
                        this.SendDefaultObject(202, waitTransfer);

                        stream.Position = fileInfo.Posotion;
                        fileOperator.SetFileCompletedLength(fileInfo.Posotion);
                        while (channel.MoveNext())
                        {
                            if (fileOperator.Token.IsCancellationRequested)
                            {
                                channel.Cancel();
                                fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                                break;
                            }
                            ByteBlock block = channel.GetCurrentByteBlock();
                            block.Pos = 6;

                            if (block.TryReadBytesPackageInfo(out int pos, out int len))
                            {
                                stream.Write(block.Buffer, pos, len);
                                stream.fileInfo.Posotion = stream.Position;
                                fileOperator.AddFileFlow(len, fileInfo.FileLength);
                                stream.SaveProgress();
                            }
                            block.SetHolding(false);
                        }

                        if (channel.Status == ChannelStatus.Completed)
                        {
                            stream.FinishStream(args.FileRequest.Overwrite);
                            fileOperator.SetFileResult(new Result(ResultCode.Success));
                        }
                        else
                        {
                            fileOperator.SetFileResult(new Result(channel.Status.ToResultCode()));
                        }
                    }
                    catch (Exception ex)
                    {
                        fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
                        channel.Cancel(ex.Message);
                    }
                    finally
                    {
                        channel.Dispose();
                        RRQMStreamPool.TryReleaseWriteStream(savePath, fileOperator);
                    }

                    this.OnFileTransfered(new FileTransferStatusEventArgs(args.TransferType, args.FileRequest, args.Metadata, fileOperator.Result, args.FileInfo));
                    return;
                }
                else
                {
                    waitTransfer.Status = 4;
                    waitTransfer.Message = mes;
                    fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetResString()));
                    this.OnFileTransfered(new FileTransferStatusEventArgs(args.TransferType, args.FileRequest, args.Metadata, fileOperator.Result, args.FileInfo));
                }
            }
            else
            {
                fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetResString(args.Message)));
                waitTransfer.Status = 2;
                this.OnFileTransfered(new FileTransferStatusEventArgs(args.TransferType, args.FileRequest, args.Metadata, fileOperator.Result, args.FileInfo));
            }

            this.SendDefaultObject(202, waitTransfer);
        }

        private void SendDefaultObject(short protocol, object obj)
        {
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            byteBlock.WriteObject(obj, SerializationType.Json);
            try
            {
                this.InternalSend(protocol, byteBlock);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}