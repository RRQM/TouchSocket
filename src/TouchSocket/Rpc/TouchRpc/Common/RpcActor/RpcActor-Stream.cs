//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Log;
using TouchSocket.Core.Run;
using TouchSocket.Resources;

namespace TouchSocket.Rpc.TouchRpc
{
    public partial class RpcActor
    {
        /// <summary>
        /// 发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Result SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata = default)
        {
            WaitStream waitStream = new WaitStream();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitStream);
            waitStream.Metadata = metadata;
            long size = stream.Length - stream.Position;
            waitStream.Size = size;
            waitStream.StreamType = stream.GetType().FullName;
            ByteBlock byteBlock = BytePool.GetByteBlock(TouchRpcUtility.TransferPackage)
                .WriteObject(waitStream);
            try
            {
                this.SocketSend(TouchRpcUtility.P_400_SendStreamToSocketClient_Request, byteBlock);

                waitData.SetCancellationToken(streamOperator.Token);

                waitData.Wait(streamOperator.Timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitStream waitStreamResult = (WaitStream)waitData.WaitResult;
                            if (waitStreamResult.Status == 1)
                            {
                                if (this.TrySubscribeChannel(waitStreamResult.ChannelID, out Channel channel))
                                {
                                    while (true)
                                    {
                                        if (streamOperator.Token.IsCancellationRequested)
                                        {
                                            channel.Cancel();
                                            return streamOperator.SetStreamResult(new Result(ResultCode.Canceled));
                                        }
                                        int r = stream.Read(byteBlock.Buffer, 0,
                                            (int)Math.Min(TouchRpcUtility.TransferPackage, streamOperator.MaxSpeed / 10.0));
                                        if (r <= 0)
                                        {
                                            channel.Complete();
                                            return streamOperator.SetStreamResult(new Result(ResultCode.Success));
                                        }

                                        channel.Write(byteBlock.Buffer, 0, r);
                                        streamOperator.AddStreamFlow(r, size);
                                    }
                                }
                                else
                                {
                                    return streamOperator.SetStreamResult(new Result(ResultCode.Error, TouchSocketRes.SetChannelFail.GetDescription()));
                                }
                            }
                            else if (waitStreamResult.Status == 2)
                            {
                                return streamOperator.SetStreamResult(new Result(ResultCode.Error, TouchSocketRes.RemoteRefuse.GetDescription(waitStreamResult.Message)));
                            }
                            else if (waitStreamResult.Status == 3)
                            {
                                return streamOperator.SetStreamResult(new Result(ResultCode.Error, TouchSocketRes.Exception.GetDescription(waitStreamResult.Message)));
                            }
                            else
                            {
                                return streamOperator.SetStreamResult(new Result(ResultCode.Error));
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return streamOperator.SetStreamResult(new Result(ResultCode.Overtime));
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return streamOperator.SetStreamResult(new Result(ResultCode.Canceled));
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return streamOperator.SetStreamResult(new Result(ResultCode.Error));
                        }
                }
            }
            catch (Exception ex)
            {
                return streamOperator.SetStreamResult(new Result(ResultCode.Error, ex.Message));
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata = default)
        {
            return Task.Run(() =>
            {
                return this.SendStream(stream, streamOperator, metadata);
            });
        }
        private void P_8_RequestStreamToThis(WaitStream waitStream)
        {
            StreamOperator streamOperator = new StreamOperator();
            StreamInfo streamInfo = new StreamInfo(waitStream.Size, waitStream.StreamType);
            StreamOperationEventArgs args = new StreamOperationEventArgs(streamOperator, waitStream.Metadata, streamInfo);

            try
            {
                this.OnStreamTransfering?.Invoke(this, args);
                if (args.Operation.HasFlag(Operation.Permit))
                {
                    if (args.Bucket != null)
                    {
                        waitStream.Status = 1;
                        Channel channel = this.CreateChannel();
                        waitStream.ChannelID = channel.ID;
                        this.TaskTransferingStream(streamOperator, args, channel, waitStream.Size);
                    }
                    else
                    {
                        streamOperator.SetStreamResult(new Result(ResultCode.Error, TouchSocketRes.StreamBucketNull.GetDescription()));
                        waitStream.Status = 3;
                        waitStream.Message = "未设置流容器";

                        this.OnStreamTransfered?.Invoke(this, new StreamStatusEventArgs(new Result(ResultCode.Error, waitStream.Message), waitStream.Metadata, streamInfo)
                        { Message = args.Message });
                    }
                }
                else
                {
                    streamOperator.SetStreamResult(new Result(ResultCode.Error, args.Message));
                    waitStream.Status = 2;
                    waitStream.Message = args.Message;

                    this.OnStreamTransfered?.Invoke(this, new StreamStatusEventArgs(new Result(ResultCode.Error, TouchSocketRes.RemoteRefuse.GetDescription(waitStream.Message)), waitStream.Metadata, streamInfo)
                    { Message = args.Message });
                }
            }
            catch (Exception ex)
            {
                streamOperator.SetStreamResult(new Result(ResultCode.Error, ex.Message));
                waitStream.Status = 3;
                waitStream.Message = ex.Message;

                this.Logger.Log(LogType.Error, this, $"在{nameof(P_8_RequestStreamToThis)}中发生错误。", ex);
            }

            waitStream.Metadata = null;
            using (ByteBlock byteBlock = new ByteBlock().WriteObject(waitStream))
            {
                this.SocketSend(TouchRpcUtility.P_1400_SendStreamToSocketClient_Response, byteBlock);
            }
        }

        private void P_9_RequestStreamToThis(WaitStream waitStream)
        {
            StreamOperator streamOperator = new StreamOperator();
            StreamInfo streamInfo = new StreamInfo(waitStream.Size, waitStream.StreamType);
            StreamOperationEventArgs args = new StreamOperationEventArgs(streamOperator, waitStream.Metadata, streamInfo);

            try
            {
                this.OnStreamTransfering?.Invoke(this, args);

                if (args.Operation.HasFlag(Operation.Permit))
                {
                    if (args.Bucket != null)
                    {
                        waitStream.Status = 1;
                        Channel channel = this.CreateChannel();
                        waitStream.ChannelID = channel.ID;
                        this.TaskTransferingStream(streamOperator, args, channel, waitStream.Size);
                    }
                    else
                    {
                        streamOperator.SetStreamResult(new Result(ResultCode.Error, TouchSocketRes.StreamBucketNull.GetDescription()));
                        waitStream.Status = 3;
                        waitStream.Message = "未设置流容器";

                        this.OnStreamTransfered?.Invoke(this, new StreamStatusEventArgs(new Result(ResultCode.Error, waitStream.Message), waitStream.Metadata, streamInfo)
                        { Message = args.Message });
                    }
                }
                else
                {
                    streamOperator.SetStreamResult(new Result(ResultCode.Error, args.Message));
                    waitStream.Status = 2;
                    waitStream.Message = args.Message;

                    this.OnStreamTransfered?.Invoke(this, new StreamStatusEventArgs(new Result(ResultCode.Error, TouchSocketRes.RemoteRefuse.GetDescription(waitStream.Message)), waitStream.Metadata, streamInfo)
                    { Message = args.Message });
                }
            }
            catch (Exception ex)
            {
                streamOperator.SetStreamResult(new Result(ResultCode.Error, ex.Message));
                waitStream.Status = 3;
                waitStream.Message = ex.Message;

                this.Logger.Log(LogType.Error, this, $"在{nameof(P_9_RequestStreamToThis)}中发生错误。", ex);
            }

            waitStream.Metadata = null;
            using (ByteBlock byteBlock = new ByteBlock().WriteObject(waitStream))
            {
                this.SocketSend(TouchRpcUtility.P_401_SendStreamToClient, byteBlock);
            }
        }

        private void TaskTransferingStream(StreamOperator streamOperator, StreamOperationEventArgs args, Channel channel, long size)
        {
            Task.Run(() =>
            {
                Stream stream = args.Bucket;
                while (channel.MoveNext())
                {
                    if (streamOperator.Token.IsCancellationRequested)
                    {
                        channel.Cancel();
                        streamOperator.SetStreamResult(new Result(ResultCode.Canceled));
                        break;
                    }
                    ByteBlock block = channel.GetCurrentByteBlock();
                    block.Pos = 6;

                    if (block.TryReadBytesPackageInfo(out int pos, out int len))
                    {
                        stream.Write(block.Buffer, pos, len);
                        streamOperator.AddStreamFlow(len, size);
                    }
                    block.SetHolding(false);
                }
                this.OnStreamTransfered?.Invoke(this, new StreamStatusEventArgs(streamOperator.SetStreamResult(new Result(channel.Status.ToResultCode())),
                    args.Metadata, args.StreamInfo)
                { Bucket = stream, Message = args.Message });
            });
        }
    }
}