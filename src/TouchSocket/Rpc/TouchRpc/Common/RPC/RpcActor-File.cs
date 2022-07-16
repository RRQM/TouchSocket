//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.IO;
using TouchSocket.Core.Run;
using TouchSocket.Core.Serialization;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    public partial class RpcActor
    {
        private ConcurrentDictionary<int, FileOperationEventArgs> m_eventArgs;

        private ResponseType m_responseType;

        private string m_rootPath = string.Empty;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ResponseType ResponseType
        {
            get => this.m_responseType;
            set => this.m_responseType = value;
        }

        /// <summary>
        /// 根路径
        /// </summary>
        public string RootPath
        {
            get => this.m_rootPath;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                this.m_rootPath = value;
            }
        }

        /// <summary>
        /// 从对点拉取文件
        /// </summary>
        public Result PullFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            if (fileRequest is null)
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileRequest)));
            }

            if (fileOperator is null)
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileOperator)));
            }

            if (string.IsNullOrEmpty(fileRequest.SavePath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(fileRequest.SavePath), fileRequest.SavePath)));
            }

            WaitFileInfo waitFileInfo = new WaitFileInfo();
            waitFileInfo.Metadata = metadata;
            waitFileInfo.FileRequest = fileRequest;

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitFileInfo);

            ByteBlock byteBlock = new ByteBlock().WriteObject(waitFileInfo, SerializationType.Json);
            try
            {
                this.SocketSend(TouchRpcUtility.P_500_PullFile_Request, byteBlock);
                waitData.SetCancellationToken(fileOperator.Token);

                waitData.Wait(fileOperator.Timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitFileInfo waitFileResult = (WaitFileInfo)waitData.WaitResult;
                            if (waitFileResult.Status == 1)
                            {
                                return this.PreviewPullFile(null, fileOperator, waitFileResult);
                            }
                            else if (waitFileResult.Status == 2)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetDescription(waitFileResult.Message)));
                            }
                            else if (waitFileResult.Status == 3)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(waitFileResult.FileRequest.Path), waitFileResult.FileRequest.Path)));
                            }
                            else if (waitFileResult.Status == 4)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteFileNotExists.GetDescription(waitFileResult.FileRequest.Path)));
                            }
                            else if (waitFileResult.Status == 6)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteNotSupported.GetDescription()));
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
        /// 从对点拉取文件
        /// </summary>
        public Result PullFile(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    return rpcActor.PullFile(fileRequest, fileOperator, metadata);
                }
                throw new ClientNotFindException(ResType.ClientNotFind.GetDescription());
            }

            if (string.IsNullOrEmpty(targetID) || targetID == this.ID)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, $"{nameof(targetID)}不能为空，或者等于自身ID"));
            }

            if (fileRequest is null)
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileRequest)));
            }

            if (fileOperator is null)
            {
                return new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileOperator)));
            }

            if (string.IsNullOrEmpty(fileRequest.SavePath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(fileRequest.SavePath), fileRequest.SavePath)));
            }

            WaitFileInfo waitFileInfo = new WaitFileInfo();
            waitFileInfo.Metadata = metadata;
            waitFileInfo.FileRequest = fileRequest;
            waitFileInfo.ClientID = targetID;

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitFileInfo);

            ByteBlock byteBlock = new ByteBlock().WriteObject(waitFileInfo, SerializationType.Json);
            try
            {
                this.SocketSend(TouchRpcUtility.P_503_PullFile2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                waitData.SetCancellationToken(fileOperator.Token);

                waitData.Wait(60 * 1000);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitFileInfo waitFileResult = (WaitFileInfo)waitData.WaitResult;
                            if (waitFileResult.Status == 1)
                            {
                                return this.PreviewPullFile(targetID, fileOperator, waitFileResult);
                            }
                            else if (waitFileResult.Status == 2)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetDescription(waitFileResult.Message)));
                            }
                            else if (waitFileResult.Status == 3)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(waitFileResult.FileRequest.Path), waitFileResult.FileRequest.Path)));
                            }
                            else if (waitFileResult.Status == 4)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteFileNotExists.GetDescription(waitFileResult.FileRequest.Path)));
                            }
                            else if (waitFileResult.Status == 6)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteNotSupported.GetDescription()));
                            }
                            else if (waitFileResult.Status == 7)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ClientNotFind.GetDescription(targetID)));
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
        /// <param name="clientID"></param>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<Result> PullFileAsync(string clientID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return await Task.Run(() =>
            {
                return this.PullFile(clientID, fileRequest, fileOperator, metadata);
            });
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
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileRequest))));
            }

            if (fileOperator is null)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileOperator))));
            }

            if (string.IsNullOrEmpty(fileRequest.SavePath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(fileRequest.SavePath), fileRequest.SavePath)));
            }
            string fullPath;
            if (Path.IsPathRooted(fileRequest.Path))
            {
                fullPath = fileRequest.Path;
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(this.m_rootPath, fileRequest.Path));
            }
            if (!File.Exists(fullPath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(fullPath), fullPath)));
            }

            TouchRpcFileInfo fileInfo;
            try
            {
                fileInfo = FileTool.GetFileInfo(fullPath, fileRequest.Flags.HasFlag(TransferFlags.BreakpointResume));
            }
            catch (Exception ex)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
            }

            WaitFileInfo waitFileInfo = new WaitFileInfo();
            waitFileInfo.FileInfo = fileInfo;
            waitFileInfo.Metadata = metadata;
            waitFileInfo.FileRequest = fileRequest;

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitFileInfo);

            ByteBlock byteBlock = new ByteBlock().WriteObject(waitFileInfo, SerializationType.Json);
            LoopAction loopAction = null;
            try
            {
                this.SocketSend(TouchRpcUtility.P_502_PushFile_Request, byteBlock);
                waitData.SetCancellationToken(fileOperator.Token);

                waitData.Wait(fileOperator.Timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitTransfer waitTransferResult = (WaitTransfer)waitData.WaitResult;
                            if (waitTransferResult.Status == 1)
                            {
                                return this.PreviewPushFile(fileOperator, waitTransferResult);
                            }
                            else if (waitTransferResult.Status == 2)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetDescription(waitTransferResult.Message)));
                            }
                            else if (waitTransferResult.Status == 3)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.FileExists.GetDescription(waitTransferResult.Path)));
                            }
                            else if (waitTransferResult.Status == 4)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetDescription(waitTransferResult.Path)));
                            }
                            else if (waitTransferResult.Status == 6)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteNotSupported.GetDescription()));
                            }
                            else
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, waitTransferResult.Message));
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
        /// 将文件推送到对点
        /// </summary>
        public Result PushFile(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    return rpcActor.PushFile(fileRequest, fileOperator, metadata);
                }
                throw new ClientNotFindException(ResType.ClientNotFind.GetDescription());
            }

            if (string.IsNullOrEmpty(targetID) || targetID == this.ID)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, $"{nameof(targetID)}不能为空，或者等于自身ID"));
            }

            if (fileRequest is null)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileRequest))));
            }

            if (fileOperator is null)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ArgumentNull.GetDescription(nameof(fileOperator))));
            }

            if (string.IsNullOrEmpty(fileRequest.SavePath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(fileRequest.SavePath), fileRequest.SavePath)));
            }

            string fullPath;
            if (Path.IsPathRooted(fileRequest.Path))
            {
                fullPath = fileRequest.Path;
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(this.m_rootPath, fileRequest.Path));
            }

            if (!File.Exists(fullPath))
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.PathInvalid.GetDescription(nameof(fullPath), fullPath)));
            }

            TouchRpcFileInfo fileInfo;
            try
            {
                fileInfo = FileTool.GetFileInfo(fullPath, fileRequest.Flags.HasFlag(TransferFlags.BreakpointResume));
            }
            catch (Exception ex)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
            }

            WaitFileInfo waitFileInfo = new WaitFileInfo()
            {
                FileInfo = fileInfo,
                Metadata = metadata,
                FileRequest = fileRequest,
                ClientID = targetID
            };
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitFileInfo);

            ByteBlock byteBlock = new ByteBlock().WriteObject(waitFileInfo, SerializationType.Json);
            try
            {
                this.SocketSend(TouchRpcUtility.P_507_PushFile2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                waitData.SetCancellationToken(fileOperator.Token);

                waitData.Wait(60 * 1000);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitTransfer waitResult = (WaitTransfer)waitData.WaitResult;
                            if (waitResult.Status == 1)
                            {
                                return this.PreviewPushFile(fileOperator, waitResult);
                            }
                            else if (waitResult.Status == 2)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetDescription(waitResult.Message)));
                            }
                            else if (waitResult.Status == 3)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.FileExists.GetDescription(waitResult.Path)));
                            }
                            else if (waitResult.Status == 4)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetDescription()));
                            }
                            else if (waitResult.Status == 6)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.RemoteNotSupported.GetDescription()));
                            }
                            else if (waitResult.Status == 7)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.ClientNotFind.GetDescription(targetID)));
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
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> PushFileAsync(string clientID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return Task.Run(() =>
            {
                return this.PushFile(clientID, fileRequest, fileOperator, metadata);
            });
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

        private void BeginPullFile(short responseOrder, WaitTransfer waitTransfer)
        {
            //2.加载流异常
            //3.事件操作器异常
            //4.通道建立异常

            Task.Run(() =>
            {
                FileTransferStatusEventArgs e;
                if (this.m_eventArgs.TryRemove(waitTransfer.EventHashCode, out FileOperationEventArgs args))
                {
                    try
                    {
                        using (FileStorageReader reader = FilePool.GetReader(waitTransfer.Path))
                        {
                            if (this.TrySubscribeChannel(waitTransfer.ChannelID, out Channel channel))
                            {
                                ByteBlock byteBlock = BytePool.GetByteBlock(TouchRpcUtility.TransferPackage);
                                FileOperator fileOperator = args.FileOperator;
                                try
                                {
                                    waitTransfer.Status = 1;
                                    this.SendDefaultObject(responseOrder, waitTransfer);
                                    long position = waitTransfer.Position;
                                    reader.Position = position;
                                    fileOperator.SetFileCompletedLength(position);
                                    while (true)
                                    {
                                        if (fileOperator.Token.IsCancellationRequested)
                                        {
                                            channel.Cancel("主动取消");
                                            fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                                            break;
                                        }
                                        int r = reader.Read(byteBlock.Buffer, 0,
                                            (int)Math.Min(TouchRpcUtility.TransferPackage, fileOperator.MaxSpeed / 10.0));
                                        if (r == 0)
                                        {
                                            channel.Complete();
                                            break;
                                        }
                                        else
                                        {
                                            position += r;
                                            if (channel.TryWrite(byteBlock.Buffer, 0, r))
                                            {
                                                fileOperator.AddFileFlow(r, reader.FileStorage.FileInfo.Length);
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
                                    byteBlock.Dispose();

                                    e = new FileTransferStatusEventArgs(TransferType.Pull, args.FileRequest, args.Metadata,
                                       fileOperator.SetFileResult(new Result(channel.Status.ToResultCode())), null);
                                    this.OnFileTransfered?.Invoke(this, e);
                                }

                                return;
                            }
                            else
                            {
                                waitTransfer.Status = 4;
                                e = new FileTransferStatusEventArgs(TransferType.Pull,
                                    args.FileRequest, args.Metadata, new Result(ResultCode.Error, StringResStore.GetDescription(ResType.SetChannelFail)), null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        waitTransfer.Status = 2;
                        waitTransfer.Message = ex.Message;
                        e = new FileTransferStatusEventArgs(TransferType.Pull,
                             args.FileRequest, args.Metadata, new Result(ResultCode.Error, StringResStore.GetDescription(ResType.LoadStreamFail)), null);
                    }
                }
                else
                {
                    e = new FileTransferStatusEventArgs(TransferType.Pull, null, null,
                        new Result(ResultCode.Overtime, StringResStore.GetDescription(ResType.GetEventArgsFail)), null);
                    waitTransfer.Status = 3;
                }

                this.SendDefaultObject(responseOrder, waitTransfer);

                this.OnFileTransfered?.Invoke(this, e);
            });
        }

        private Result PreviewPullFile(string clientID, FileOperator fileOperator, WaitFileInfo waitFileInfo)
        {
            FileRequest fileRequest = waitFileInfo.FileRequest;
            TouchRpcFileInfo fileInfo = waitFileInfo.FileInfo;
            if (!Path.IsPathRooted(fileRequest.SavePath))
            {
                fileRequest.SavePath = Path.GetFullPath(Path.Combine(this.m_rootPath, fileRequest.SavePath));
            }
            TouchRpcFileStream stream;
            try
            {
                stream = TouchRpcFileStream.Create(fileRequest.SavePath, ref fileInfo, fileRequest.Flags.HasFlag(TransferFlags.BreakpointResume));
            }
            catch (Exception ex)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ex.Message));
            }

            Channel channel;
            if (clientID == null)
            {
                channel = this.CreateChannel();
            }
            else
            {
                channel = this.CreateChannel(clientID);
            }
            WaitTransfer waitTransfer = new WaitTransfer()
            {
                EventHashCode = waitFileInfo.EventHashCode,
                ChannelID = channel.ID,
                Path = fileInfo.FilePath,
                Position = fileInfo.Position,
                ClientID = clientID
            };

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitTransfer);
            ByteBlock byteBlock = new ByteBlock().WriteObject(waitTransfer, SerializationType.Json);
            try
            {
                if (clientID == null)
                {
                    this.SocketSend(TouchRpcUtility.P_501_BeginPullFile_Request, byteBlock);
                }
                else
                {
                    this.SocketSend(TouchRpcUtility.P_505_BeginPullFile2C_Request, byteBlock.Buffer, 0, byteBlock.Len);
                }
                waitData.SetCancellationToken(fileOperator.Token);

                switch (waitData.Wait(fileOperator.Timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitTransfer waitTransferResult = (WaitTransfer)waitData.WaitResult;
                            if (waitTransferResult.Status == 1)
                            {
                                fileOperator.SetFileCompletedLength(fileInfo.Position);
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
                                        fileInfo.Position = stream.FileWriter.Position;
                                        fileOperator.AddFileFlow(len, fileInfo.FileLength);
                                    }
                                    block.SetHolding(false);
                                }

                                if (channel.Status == ChannelStatus.Completed)
                                {
                                    stream.FinishStream();
                                    return fileOperator.SetFileResult(new Result(ResultCode.Success));
                                }
                                else
                                {
                                    return fileOperator.SetFileResult(new Result(channel.Status.ToResultCode()));
                                }
                            }
                            else if (waitTransferResult.Status == 2)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetDescription(fileInfo.FilePath)));
                            }
                            else if (waitTransferResult.Status == 3)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.GetEventArgsFail.GetDescription()));
                            }
                            else if (waitTransferResult.Status == 4)
                            {
                                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.SetChannelFail.GetDescription()));
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
                stream.SafeDispose();
                byteBlock.Dispose();
                channel.Dispose();
            }
        }

        private Result PreviewPushFile(FileOperator fileOperator, WaitTransfer waitTransfer)
        {
            try
            {
                using (FileStorageReader reader = FilePool.GetReader(waitTransfer.Path))
                {
                    if (this.TrySubscribeChannel(waitTransfer.ChannelID, out Channel channel))
                    {
                        ByteBlock byteBlock = BytePool.GetByteBlock(TouchRpcUtility.TransferPackage);
                        try
                        {
                            long position = waitTransfer.Position;
                            reader.Position = position;
                            fileOperator.SetFileCompletedLength(waitTransfer.Position);

                            while (true)
                            {
                                if (fileOperator.Token.IsCancellationRequested)
                                {
                                    channel.Cancel("主动取消");
                                    fileOperator.SetFileResult(new Result(ResultCode.Canceled));
                                    break;
                                }
                                int r = reader.Read(byteBlock.Buffer, 0, (int)Math.Min(TouchRpcUtility.TransferPackage, fileOperator.MaxSpeed / 10.0));
                                if (r == 0)
                                {
                                    channel.Complete();
                                    break;
                                }
                                else
                                {
                                    position += r;
                                    if (channel.TryWrite(byteBlock.Buffer, 0, r))
                                    {
                                        fileOperator.AddFileFlow(r, reader.FileStorage.FileInfo.Length);
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
                            byteBlock.Dispose();
                        }
                    }
                    else
                    {
                        return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.SetChannelFail.GetDescription()));
                    }
                }
            }
            catch (Exception ex)
            {
                return fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetDescription(waitTransfer.Path, ex.Message)));
            }
        }

        private WaitFileInfo RequestPullFile(WaitFileInfo waitFileInfo)
        {
            //2.不允许
            //3.路径无效
            //4.文件不存在
            //5.其他

            Metadata metadata = waitFileInfo.Metadata;
            FileRequest fileRequest = waitFileInfo.FileRequest;
            TouchRpcFileInfo fileInfo = waitFileInfo.FileInfo;

            if (this.m_responseType == ResponseType.None || this.m_responseType == ResponseType.Push)
            {
                waitFileInfo.Status = 6;
                return waitFileInfo;
            }

            if (!Path.IsPathRooted(fileRequest.Path))
            {
                fileRequest.Path = Path.GetFullPath(Path.Combine(this.m_rootPath, fileRequest.Path));
            }

            FileOperationEventArgs args = new FileOperationEventArgs(TransferType.Pull, fileRequest, new FileOperator(), metadata, null);
            this.OnFileTransfering?.Invoke(this, args);
            if (string.IsNullOrEmpty(fileRequest.Path))
            {
                waitFileInfo.Status = 3;
                return waitFileInfo;
            }

            if (args.IsPermitOperation)
            {
                if (File.Exists(fileRequest.Path))
                {
                    //合法传输
                    waitFileInfo.FileInfo = FileTool.GetFileInfo(fileRequest.Path, fileRequest.Flags.HasFlag(TransferFlags.BreakpointResume));
                    waitFileInfo.Status = 1;
                    waitFileInfo.EventHashCode = args.GetHashCode();

                    this.m_eventArgs.TryAdd(args.GetHashCode(), args);
                    EasyAction.DelayRun(1000 * 60, args, (a) =>
                    {
                        if (this.m_eventArgs.TryRemove(a.GetHashCode(), out FileOperationEventArgs eventArgs))
                        {
                            FileTransferStatusEventArgs e = new FileTransferStatusEventArgs(
                                 TransferType.Pull,
                                 eventArgs.FileRequest,
                                 eventArgs.Metadata,
                                 new Result(ResultCode.Overtime, StringResStore.GetDescription(ResType.NoResponse)), null);
                            this.OnFileTransfered?.Invoke(this, e);
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
                waitFileInfo.Message = args.Message;
                waitFileInfo.Status = 2;
                return waitFileInfo;
            }
        }

        private void RequestPushFile(short responseOrder, WaitFileInfo waitRemoteFileInfo)
        {
            //2.不允许
            //3.文件已存在
            //4.加载流异常
            //5.其他

            Task.Run(() =>
            {
                if (this.m_responseType == ResponseType.None || this.m_responseType == ResponseType.Pull)
                {
                    this.SendDefaultObject(responseOrder, new WaitTransfer() { Sign = waitRemoteFileInfo.Sign, Status = 6 });
                    return;
                }
                FileRequest fileRequest = waitRemoteFileInfo.FileRequest;
                Metadata metadata = waitRemoteFileInfo.Metadata;
                TouchRpcFileInfo fileInfo = waitRemoteFileInfo.FileInfo;

                if (!Path.IsPathRooted(fileRequest.SavePath))
                {
                    fileRequest.SavePath = Path.GetFullPath(Path.Combine(this.m_rootPath, fileRequest.SavePath));
                }

                FileOperator fileOperator = new FileOperator();
                FileOperationEventArgs args = new FileOperationEventArgs(TransferType.Push, fileRequest,
                    fileOperator, metadata, fileInfo);
                this.OnFileTransfering?.Invoke(this, args);

                WaitTransfer waitTransfer = new WaitTransfer()
                {
                    Message = args.Message,
                    Sign = waitRemoteFileInfo.Sign,
                    Path = args.FileRequest.Path,
                    ClientID = waitRemoteFileInfo.ClientID
                };

                if (args.IsPermitOperation)
                {
                    try
                    {
                        using (TouchRpcFileStream stream = TouchRpcFileStream.Create(fileRequest.SavePath, ref fileInfo, fileRequest.Flags.HasFlag(TransferFlags.BreakpointResume)))
                        {
                            Channel channel = null;
                            try
                            {
                                if (responseOrder == TouchRpcUtility.P_1502_PushFile_Response)
                                {
                                    channel = this.CreateChannel();
                                }
                                else
                                {
                                    channel = this.CreateChannel(waitTransfer.ClientID);
                                }

                                waitTransfer.ChannelID = channel.ID;
                                waitTransfer.Position = fileInfo.Position;
                                waitTransfer.Status = 1;
                                this.SendDefaultObject(responseOrder, waitTransfer);

                                fileOperator.SetFileCompletedLength(fileInfo.Position);
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
                                        fileInfo.Position = stream.FileWriter.Position;
                                        fileOperator.AddFileFlow(len, fileInfo.FileLength);
                                    }
                                    block.SetHolding(false);
                                }

                                if (channel.Status == ChannelStatus.Completed)
                                {
                                    stream.FinishStream();
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
                                if (channel != null)
                                {
                                    channel.Cancel(ex.Message);
                                }
                            }
                            finally
                            {
                                if (channel != null)
                                {
                                    channel.Dispose();
                                }
                            }
                            this.OnFileTransfered?.Invoke(this, new FileTransferStatusEventArgs(args.TransferType, args.FileRequest, args.Metadata, fileOperator.Result, args.FileInfo));
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        waitTransfer.Status = 4;
                        waitTransfer.Message = ex.Message;
                        fileOperator.SetFileResult(new Result(ResultCode.Error, ResType.LoadStreamFail.GetDescription(ex.Message)));
                        this.OnFileTransfered?.Invoke(this, new FileTransferStatusEventArgs(args.TransferType, args.FileRequest, args.Metadata, fileOperator.Result, args.FileInfo));
                    }
                }
                else
                {
                    waitTransfer.Message = args.Message;
                    waitTransfer.Status = 2;
                    this.OnFileTransfered?.Invoke(this, new FileTransferStatusEventArgs(args.TransferType, args.FileRequest, args.Metadata, fileOperator.Result, args.FileInfo));
                }

                this.SendDefaultObject(responseOrder, waitTransfer);
            });
        }

        private void SendDefaultObject(short protocol, object obj)
        {
            ByteBlock byteBlock = new ByteBlock();
            byteBlock.WriteObject(obj, SerializationType.Json);
            try
            {
                this.SocketSend(protocol, byteBlock);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}