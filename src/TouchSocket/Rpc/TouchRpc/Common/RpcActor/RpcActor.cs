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
using System.Linq;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Data.Security;
using TouchSocket.Core.Log;
using TouchSocket.Core.Run;
using TouchSocket.Core.Serialization;
using TouchSocket.Resources;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// RpcActor
    /// </summary>
    public partial class RpcActor : DisposableObject, IRpcActor, IInternalRpc
    {
        #region 委托

        /// <summary>
        /// 获取调用函数的委托
        /// </summary>
        public Func<string, MethodInstance> GetInvokeMethod { get; set; }

        /// <summary>
        /// 请求关闭
        /// </summary>
        public Action<RpcActor, string> OnClose { get; set; }

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        public Action<RpcActor, FileTransferStatusEventArgs> OnFileTransfered { get; set; }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        public Action<RpcActor, FileOperationEventArgs> OnFileTransfering { get; set; }

        /// <summary>
        /// 查找其他RpcActor
        /// </summary>
        public Func<string, RpcActor> OnFindRpcActor { get; set; }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        public Action<RpcActor, VerifyOptionEventArgs> OnHandshaked { get; set; }

        /// <summary>
        /// 握手
        /// </summary>
        public Action<RpcActor, VerifyOptionEventArgs> OnHandshaking { get; set; }

        /// <summary>
        /// 接收到数据
        /// </summary>
        public Action<RpcActor, short, ByteBlock> OnReceived { get; set; }

        /// <summary>
        /// 重设ID
        /// </summary>
        public Action<RpcActor, WaitSetID> OnResetID { get; set; }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        public Action<RpcActor, StreamStatusEventArgs> OnStreamTransfered { get; set; }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        public Action<RpcActor, StreamOperationEventArgs> OnStreamTransfering { get; set; }

        /// <summary>
        /// 发送数据接口
        /// </summary>
        public Action<RpcActor, bool, ArraySegment<byte>[]> OutputSend { get; set; }

        #endregion 委托

        #region 变量

        private readonly bool m_isService;
        private readonly WaitHandlePool<IWaitResult> m_waitHandlePool;
        private string m_id;
        private bool m_isHandshaked;
        private ILog m_logger;

        #endregion 变量

        #region 属性

        /// <summary>
        /// 本节点ID
        /// </summary>
        public string ID { get => this.m_id; set => this.m_id = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsHandshaked => this.m_isHandshaked;

        /// <summary>
        /// 是否为服务器组件
        /// </summary>
        public bool IsService => this.m_isService;

        /// <summary>
        /// 日志
        /// </summary>
        public ILog Logger { get => this.m_logger; set => this.m_logger = value; }

        /// <summary>
        ///  获取可用于同步对<see cref="RpcActor"/>的访问的对象。
        /// </summary>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<IRpcClient, bool> TryCanInvoke { get; set; }

        /// <summary>
        /// 等待返回池
        /// </summary>
        public WaitHandlePool<IWaitResult> WaitHandlePool => this.m_waitHandlePool;

        #endregion 属性

        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcActor(bool isService)
        {
            this.m_eventArgs = new ConcurrentDictionary<int, object>();
            this.m_waitHandlePool = new WaitHandlePool<IWaitResult>();
            this.m_userChannels = new ConcurrentDictionary<int, Channel>();
            this.m_contextDic = new ConcurrentDictionary<long, TouchRpcCallContext>();
            this.m_isService = isService;
            this.m_waitCallback_InvokeThis = this.InvokeThis;
            this.m_waitCallback_InvokeClientByID = this.InvokeClientByID;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="message"></param>
        public void Close(string message)
        {
            if (this.m_isHandshaked)
            {
                this.m_isHandshaked = false;
                foreach (var item in this.m_userChannels.Values)
                {
                    item.SafeDispose();
                }
                var keys = this.m_contextDic.Keys.ToArray();
                foreach (var item in keys)
                {
                    if (this.m_contextDic.TryRemove(item, out TouchRpcCallContext rpcCallContext))
                    {
                        rpcCallContext.TryCancel();
                    }
                }
                this.WaitHandlePool.CancelAll();
                this.OnClose?.Invoke(this, message);
            }
        }

        /// <summary>
        /// 建立对点
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Handshake(string verifyToken, CancellationToken token = default, int timeout = 5000, Metadata metadata = null)
        {
            if (this.m_isHandshaked)
            {
                return;
            }
            WaitVerify waitVerify = new WaitVerify()
            {
                Token = verifyToken,
                Metadata = metadata
            };
            WaitData<IWaitResult> waitData = this.m_waitHandlePool.GetWaitData(waitVerify);
            waitData.SetCancellationToken(token);

            using (ByteBlock byteBlock = new ByteBlock())
            {
                this.SocketSend(TouchRpcUtility.P_0_Handshake_Request, byteBlock.WriteObject(waitVerify));
            }

            try
            {
                switch (waitData.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitVerify verifyResult = (WaitVerify)waitData.WaitResult;
                            if (verifyResult.Status == 1)
                            {
                                this.m_id = verifyResult.ID;
                                this.m_isHandshaked = true;
                                this.OnHandshaked?.Invoke(this, new VerifyOptionEventArgs(verifyToken, verifyResult.Metadata) { Message = "成功建立" });
                                verifyResult.Handle = true;
                                return;
                            }
                            else if (verifyResult.Status == 3)
                            {
                                verifyResult.Handle = true;
                                this.Close("连接数量已达到服务器设定最大值");
                                throw new Exception("连接数量已达到服务器设定最大值");
                            }
                            else if (verifyResult.Status == 4)
                            {
                                verifyResult.Handle = true;
                                this.Close("服务器拒绝连接");
                                throw new Exception("服务器拒绝连接");
                            }
                            else
                            {
                                verifyResult.Handle = true;
                                this.Close(verifyResult.Message);
                                throw new TokenVerifyException(verifyResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        this.Close("连接超时");
                        throw new TimeoutException("连接超时");
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        this.Close(null);
                        return;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <summary>
        /// 处理接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        public void InputReceivedData(ByteBlock byteBlock)
        {
            short protocol = TouchSocketBitConverter.Default.ToInt16(byteBlock.Buffer, 0);
            switch (protocol)
            {
                #region 0-99

                case TouchRpcUtility.P_0_Handshake_Request:
                    {
                        try
                        {
                            WaitVerify waitVerify = byteBlock.Seek(2).ReadObject<WaitVerify>();
                            VerifyOptionEventArgs args = new VerifyOptionEventArgs(waitVerify.Token, waitVerify.Metadata);
                            this.OnHandshaking?.Invoke(this, args);

                            if (args.Operation.HasFlag(Operation.Permit))
                            {
                                waitVerify.ID = this.ID;
                                waitVerify.Status = 1;
                                byteBlock.Reset();
                                byteBlock.WriteObject(waitVerify);
                                this.SocketSend(TouchRpcUtility.P_1000_Handshake_Response, byteBlock);
                                this.m_isHandshaked = true;
                                args.Message = "成功建立";
                                this.OnHandshaked?.Invoke(this, args);
                            }
                            else
                            {
                                waitVerify.Status = 2;
                                waitVerify.Message = args.Message;
                                byteBlock.Reset();
                                byteBlock.WriteObject(waitVerify);
                                this.SocketSend(TouchRpcUtility.P_1000_Handshake_Response, byteBlock);
                                this.Close(args.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Close(ex.Message);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1000_Handshake_Response:
                    {
                        WaitVerify waitVerify = byteBlock.Seek(2).ReadObject<WaitVerify>();
                        this.m_waitHandlePool.SetRun(waitVerify);
                        SpinWait.SpinUntil(() =>
                        {
                            return waitVerify.Handle;
                        }, 3000);
                        break;
                    }
                case TouchRpcUtility.P_1_ResetID_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitSetID waitSetID = byteBlock.ReadObject<WaitSetID>();
                            try
                            {
                                this.OnResetID?.Invoke(this, waitSetID);
                                this.m_id = waitSetID.NewID;
                                waitSetID.Status = 1;
                            }
                            catch (System.Exception ex)
                            {
                                waitSetID.Status = 2;
                                waitSetID.Message = ex.Message;
                            }
                            byteBlock.Reset();
                            this.SocketSend(TouchRpcUtility.P_1001_ResetID_Response, byteBlock.WriteObject(waitSetID));
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1001_ResetID_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitSetID waitSetID = byteBlock.ReadObject<WaitSetID>();
                            if (waitSetID.Status == 1)
                            {
                                this.m_id = waitSetID.NewID;
                            }
                            this.WaitHandlePool.SetRun(waitSetID);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_2_Ping_Request://心跳
                    {
                        try
                        {
                            this.SocketSend(TouchRpcUtility.P_1002_Ping_Response, byteBlock.Buffer, 2, byteBlock.Len - 2);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1002_Ping_Response://心跳
                    {
                        try
                        {
                            this.WaitHandlePool.SetRun(TouchSocketBitConverter.Default.ToInt64(byteBlock.Buffer, 2));
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_3_Ping2C_Request://心跳
                    {
                        break;
                    }
                case TouchRpcUtility.P_1003_Ping2C_Response://心跳
                    {
                        break;
                    }

                #endregion 0-99

                #region 100-199

                case TouchRpcUtility.P_100_CreateChannel_Request:
                    {
                        try
                        {
                            int id = TouchSocketBitConverter.Default.ToInt32(byteBlock.Buffer, 2);
                            this.RequestCreateChannel(id, null);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_101_DataOrder:
                case TouchRpcUtility.P_102_CompleteOrder:
                case TouchRpcUtility.P_103_CancelOrder:
                case TouchRpcUtility.P_104_DisposeOrder:
                case TouchRpcUtility.P_105_HoldOnOrder:
                case TouchRpcUtility.P_106_QueueChangedOrder:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            int id = byteBlock.ReadInt32();
                            this.ReceivedChannelData(id, protocol, byteBlock);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_110_CreateChannel_2C_Request://create channel to client
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitCreateChannel waitCreateChannel = byteBlock.ReadObject<WaitCreateChannel>();

                            var targetRpcActor = this.OnFindRpcActor?.Invoke(waitCreateChannel.ClientID);
                            if (targetRpcActor == null)
                            {
                                waitCreateChannel.Status = 2;
                                waitCreateChannel.Message = "没有找到对应的客户端ID";
                            }
                            else
                            {
                                int id;
                                if (waitCreateChannel.RandomID)
                                {
                                    do
                                    {
                                        id = new Random(DateTime.Now.Millisecond).Next(int.MinValue, int.MaxValue);
                                    } while (this.ChannelExisted(id) || targetRpcActor.ChannelExisted(id));
                                }
                                else
                                {
                                    id = waitCreateChannel.ChannelID;
                                }
                                if (this.ChannelExisted(id) || targetRpcActor.ChannelExisted(id))
                                {
                                    waitCreateChannel.Status = 2;
                                    waitCreateChannel.Message = "ID已被占用";
                                }
                                else
                                {
                                    using (ByteBlock @byte = new ByteBlock())
                                    {
                                        targetRpcActor.SocketSend(TouchRpcUtility.P_120_CreateChannel_FC, @byte.Write(this.m_id).Write(id));
                                    }
                                    waitCreateChannel.ChannelID = id;
                                    waitCreateChannel.Status = 1;
                                }
                            }
                            using (ByteBlock block = new ByteBlock())
                            {
                                this.SocketSend(TouchRpcUtility.P_1110_CreateChannel_2C_Response, block.WriteObject(waitCreateChannel));
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_111_DataOrder_2C:
                case TouchRpcUtility.P_112_CompleteOrder_2C:
                case TouchRpcUtility.P_113_CancelOrder_2C:
                case TouchRpcUtility.P_114_DisposeOrder_2C:
                case TouchRpcUtility.P_115_HoldOnOrder_2C:
                case TouchRpcUtility.P_116_QueueChangedOrder_2C:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            string clientID = byteBlock.ReadString();
                            int channelid = byteBlock.ReadInt32();

                            using (ByteBlock block = new ByteBlock(byteBlock.Len))
                            {
                                if (this.OnFindRpcActor?.Invoke(clientID) is RpcActor rpcActor)
                                {
                                    block.Write(channelid);
                                    if (protocol == TouchRpcUtility.P_111_DataOrder_2C)
                                    {
                                        if (byteBlock.TryReadBytesPackageInfo(out int pos, out int len))
                                        {
                                            block.WriteBytesPackage(byteBlock.Buffer, pos, len);
                                        }
                                    }
                                    else if (protocol != TouchRpcUtility.P_114_DisposeOrder_2C)
                                    {
                                        block.Write(byteBlock.ReadString());
                                    }
                                    rpcActor.SocketSend((short)(protocol + 10), block);
                                }
                                else
                                {
                                    this.SocketSend(TouchRpcUtility.P_103_CancelOrder, block.Write("没有找到该ID对应的客户端"));
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1110_CreateChannel_2C_Response://create channel to client return
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            this.WaitHandlePool.SetRun(byteBlock.ReadObject<WaitCreateChannel>());
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_120_CreateChannel_FC:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            string clientID = byteBlock.ReadString();
                            int id = byteBlock.ReadInt32();
                            this.RequestCreateChannel(id, clientID);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }

                        break;
                    }

                #endregion 100-199

                #region 200-299

                case TouchRpcUtility.P_200_Invoke_Request:/*函数调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            TouchRpcPackage context = TouchRpcPackage.Deserialize(byteBlock);

                            ThreadPool.QueueUserWorkItem(this.m_waitCallback_InvokeThis, context);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1200_Invoke_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            TouchRpcPackage result = TouchRpcPackage.Deserialize(byteBlock);
                            this.WaitHandlePool.SetRun(result.Sign, result);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_201_Invoke2C_Request:/*ID调用客户端*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            TouchRpcPackage context = TouchRpcPackage.Deserialize(byteBlock);
                            ThreadPool.QueueUserWorkItem(this.m_waitCallback_InvokeClientByID, context);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1201_Invoke2C_Response:/*ID函数调用*/
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            TouchRpcPackage result = TouchRpcPackage.Deserialize(byteBlock);
                            this.WaitHandlePool.SetRun(result.Sign, result);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_204_CancelInvoke:
                    {
                        try
                        {
                            long sign = TouchSocketBitConverter.Default.ToInt64(byteBlock.Buffer, 2);
                            if (this.m_contextDic.TryGetValue(sign, out TouchRpcCallContext context))
                            {
                                context.TokenSource.Cancel();
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }

                #endregion 200-299

                #region 400-499

                case TouchRpcUtility.P_400_SendStreamToSocketClient_Request://StreamStatusToThis
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            this.P_8_RequestStreamToThis(byteBlock.ReadObject<WaitStream>());
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1400_SendStreamToSocketClient_Response://StreamStatusToThis
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitStream waitStream = byteBlock.ReadObject<WaitStream>();
                            this.WaitHandlePool.SetRun(waitStream);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_401_SendStreamToClient://StreamToThis
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            this.P_9_RequestStreamToThis(byteBlock.ReadObject<WaitStream>());
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }

                #endregion 400-499

                #region 500-599

                case TouchRpcUtility.P_500_PullFile_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();

                            EasyAction.TaskRun(waitFileInfo, (w) =>
                            {
                                this.SendFastObject(TouchRpcUtility.P_1500_PullFile_Response, this.RequestPullFile(w));
                            });
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1500_PullFile_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFile = byteBlock.ReadObject<WaitFileInfo>();
                            this.WaitHandlePool.SetRun(waitFile);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }

                        break;
                    }
                case TouchRpcUtility.P_501_BeginPullFile_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            this.BeginPullFile(TouchRpcUtility.P_1501_BeginPullFile_Response, waitTransfer);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1501_BeginPullFile_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            this.WaitHandlePool.SetRun(waitTransfer);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_502_PushFile_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();
                            this.RequestPushFile(TouchRpcUtility.P_1502_PushFile_Response, waitFileInfo);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1502_PushFile_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            this.WaitHandlePool.SetRun(waitTransfer);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }

                        break;
                    }
                case TouchRpcUtility.P_503_PullFile2C_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();

                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.OnFindRpcActor.Invoke(waitFileInfo.ClientID) is RpcActor rpcActor)
                                {
                                    waitFileInfo.ClientID = this.ID;
                                    rpcActor.SocketSend(TouchRpcUtility.P_504_PullFileFC_Request, block.WriteObject(waitFileInfo));
                                }
                                else
                                {
                                    waitFileInfo.Status = 7;
                                    this.SocketSend(TouchRpcUtility.P_1503_PullFile2C_Response, block.WriteObject(waitFileInfo));
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1503_PullFile2C_Response:
                    {
                        byteBlock.Pos = 2;
                        WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();
                        this.WaitHandlePool.SetRun(waitFileInfo);
                        break;
                    }
                case TouchRpcUtility.P_504_PullFileFC_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();

                            EasyAction.TaskRun(waitFileInfo, (w) =>
                            {
                                this.SendFastObject(TouchRpcUtility.P_1504_PullFileFC_Response, this.RequestPullFile(w));
                            });
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1504_PullFileFC_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();

                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.OnFindRpcActor?.Invoke(waitFileInfo.ClientID) is RpcActor rpcActor)
                                {
                                    waitFileInfo.ClientID = this.ID;
                                    rpcActor.SocketSend(TouchRpcUtility.P_1503_PullFile2C_Response, block.WriteObject(waitFileInfo));
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_505_BeginPullFile2C_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.OnFindRpcActor?.Invoke(waitTransfer.ClientID) is RpcActor rpcActor)
                                {
                                    waitTransfer.ClientID = this.ID;
                                    rpcActor.SocketSend(TouchRpcUtility.P_506_BeginPullFileFC_Request, block.WriteObject(waitTransfer));
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1505_BeginPullFile2C_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            this.WaitHandlePool.SetRun(waitTransfer);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_506_BeginPullFileFC_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            this.BeginPullFile(TouchRpcUtility.P_1506_BeginPullFileFC_Response, waitTransfer);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1506_BeginPullFileFC_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.OnFindRpcActor?.Invoke(waitTransfer.ClientID) is RpcActor rpcActor)
                                {
                                    rpcActor.SocketSend(TouchRpcUtility.P_1505_BeginPullFile2C_Response, block.WriteObject(waitTransfer));
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_507_PushFile2C_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();

                            using (ByteBlock block = new ByteBlock())
                            {
                                if (this.OnFindRpcActor?.Invoke(waitFileInfo.ClientID) is RpcActor rpcActor)
                                {
                                    waitFileInfo.ClientID = this.ID;
                                    rpcActor.SocketSend(TouchRpcUtility.P_508_PushFileFC_Request, block.WriteObject(waitFileInfo));
                                }
                                else
                                {
                                    this.SocketSend(TouchRpcUtility.P_1507_PushFile2C_Response, block.WriteObject(new WaitTransfer() { Sign = waitFileInfo.Sign, Status = 7 }));
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1507_PushFile2C_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();
                            this.WaitHandlePool.SetRun(waitTransfer);
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_508_PushFileFC_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitFileInfo waitFileInfo = byteBlock.ReadObject<WaitFileInfo>();
                            EasyAction.TaskRun(waitFileInfo, (w) =>
                            {
                                this.RequestPushFile(TouchRpcUtility.P_1508_PushFileFC_Response, w);
                            });
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_1508_PushFileFC_Response:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitTransfer waitTransfer = byteBlock.ReadObject<WaitTransfer>();

                            if (this.OnFindRpcActor?.Invoke(waitTransfer.ClientID) is RpcActor rpcActor)
                            {
                                using (ByteBlock block = new ByteBlock())
                                {
                                    waitTransfer.ClientID = this.ID;
                                    rpcActor.SocketSend(TouchRpcUtility.P_1507_PushFile2C_Response, block.WriteObject(waitTransfer));
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                case TouchRpcUtility.P_509_PushFileAck_Request:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitResult waitResult = byteBlock.ReadObject<WaitResult>();
                            this.m_eventArgs.TryAdd((int)waitResult.Sign, waitResult);

                            EasyAction.DelayRun(10000, waitResult, (a) =>
                             {
                                 this.m_eventArgs.TryRemove((int)a.Sign, out _);
                             });
                        }
                        catch (System.Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
                #endregion 500-599

                default:
                    {
                        if (protocol < 0)
                        {
                            return;
                        }
                        try
                        {
                            this.OnReceived?.Invoke(this, protocol, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Log(LogType.Error, this, $"在protocol={protocol}中发生错误。", ex);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Ping
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Ping(int timeout = 5000)
        {
            try
            {
                WaitResult waitResult = new WaitResult();
                var wait = this.WaitHandlePool.GetWaitData(new WaitResult());

                this.SocketSend(TouchRpcUtility.P_2_Ping_Request, TouchSocketBitConverter.Default.GetBytes(wait.WaitResult.Sign));
                switch (wait.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return true;
                        }
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Overtime:
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ping
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Ping(string clientId, int timeout = 5000)
        {
            //try
            //{
            //    WaitPing2C waitPing = new WaitPing2C()
            //    {
            //        Address = new RouteAddress()
            //        {
            //            SServiceId = this.ServiceId,
            //            SClientId = this.m_id,
            //            TServiceId = serviceId,
            //            TClientId = clientId,
            //            IsSocketClient=this.m_isService
            //        }
            //    };
            //    var wait = this.WaitHandlePool.GetWaitData(waitPing);
            //    using (ByteBlock byteBlock = new ByteBlock())
            //    {
            //        waitPing.Package(byteBlock);
            //        this.SocketSend(TouchRpcUtility.P_3_Ping2C_Request, byteBlock);
            //    }

            //    switch (wait.Wait(timeout))
            //    {
            //        case WaitDataStatus.SetRunning:
            //            {
            //                return true;
            //            }
            //        case WaitDataStatus.Default:
            //        case WaitDataStatus.Overtime:
            //        case WaitDataStatus.Canceled:
            //        case WaitDataStatus.Disposed:
            //        default:
            //            return false;
            //    }
            //}
            //catch
            //{
            //    return false;
            //}
            return false;
        }

        /// <summary>
        /// 重新设置ID,并且同步到对端
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="Exception"></exception>
        public void ResetID(string id, CancellationToken cancellationToken = default)
        {
            WaitSetID waitSetID = new WaitSetID();
            waitSetID.OldID = this.ID;
            waitSetID.NewID = id;

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitSetID);
            waitData.SetCancellationToken(cancellationToken);

            ByteBlock byteBlock = new ByteBlock();
            byteBlock.WriteObject(waitSetID);

            this.SocketSend(TouchRpcUtility.P_1_ResetID_Request, byteBlock.Buffer, 0, byteBlock.Len);

            switch (waitData.Wait(5000))
            {
                case WaitDataStatus.SetRunning:
                    {
                        if (waitData.WaitResult.Status != 1)
                        {
                            throw new Exception(waitData.WaitResult.Message);
                        }
                        break;
                    }
                case WaitDataStatus.Overtime:
                    throw new TimeoutException(TouchSocketRes.Overtime.GetDescription());
                case WaitDataStatus.Canceled:
                    break;

                case WaitDataStatus.Disposed:
                default:
                    throw new Exception(TouchSocketRes.UnknownError.GetDescription());
            }
        }

        #region 重写

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Caller = null;
            this.OnClose = null;
            this.OnFileTransfered = null;
            this.OnFileTransfering = null;
            this.OnFindRpcActor = null;
            this.OnHandshaked = null;
            this.OnHandshaking = null;
            this.OnReceived = null;
            this.OnResetID = null;
            this.OnStreamTransfered = null;
            this.OnStreamTransfering = null;
            this.OutputSend = null;
            this.WaitHandlePool.SafeDispose();
            this.Close(nameof(Dispose));
            base.Dispose(disposing);
        }

        #endregion 重写

        #region 协议同步发送

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="protocol"></param>
        public void Send(short protocol)
        {
            this.Send(protocol, new byte[0], 0, 0);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        public void Send(short protocol, byte[] buffer)
        {
            this.Send(protocol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(short protocol, byte[] buffer, int offset, int length)
        {
            if (protocol > 0)
            {
                this.SocketSend(protocol, buffer, offset, length);
            }
            else
            {
                throw new Exception("小于0的协议为内部协议。");
            }
        }

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        public void Send(short protocol, ByteBlock byteBlock)
        {
            this.Send(protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 协议同步发送

        #region 协议异步发送

        /// <summary>
        /// 异步发送协议状态
        /// </summary>
        /// <param name="protocol"></param>
        public void SendAsync(short protocol)
        {
            this.Send(protocol, new byte[0], 0, 0);
        }

        /// <summary>
        /// 异步发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        public void SendAsync(short protocol, byte[] buffer)
        {
            this.Send(protocol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 异步发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendAsync(short protocol, byte[] buffer, int offset, int length)
        {
            if (protocol > 0)
            {
                this.SocketSend(protocol, buffer, offset, length);
            }
            else
            {
                throw new Exception("小于0的协议为内部协议。");
            }
        }

        /// <summary>
        /// 异步发送协议流
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataByteBlock"></param>
        public void SendAsync(short protocol, ByteBlock dataByteBlock)
        {
            this.Send(protocol, dataByteBlock.Buffer, 0, dataByteBlock.Len);
        }

        #endregion 协议异步发送

        #region Socket同步直发

        private void SocketSend(short protocol, byte[] dataBuffer)
        {
            this.SocketSend(protocol, dataBuffer, 0, dataBuffer.Length);
        }

        private void SocketSend(short protocol, ByteBlock byteBlock)
        {
            this.SocketSend(protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        private void SocketSend(short protocol)
        {
            this.SocketSend(protocol, new byte[0], 0, 0);
        }

        private void SocketSend(short protocol, byte[] dataBuffer, int offset, int length)
        {
            ArraySegment<byte>[] transferBytes = new ArraySegment<byte>[]
            {
            new ArraySegment<byte>(TouchSocketBitConverter.Default.GetBytes(protocol)),
            new ArraySegment<byte>(dataBuffer,offset,length)
            };
            this.OutputSend?.Invoke(this, false, transferBytes);
        }

        private void SocketSend(byte[] dataBuffer)
        {
            this.SocketSend(dataBuffer, 0, dataBuffer.Length);
        }

        private void SocketSend(ByteBlock byteBlock)
        {
            this.SocketSend(byteBlock.Buffer, 0, byteBlock.Len);
        }

        private void SocketSend(byte[] dataBuffer, int offset, int length)
        {
            ArraySegment<byte>[] transferBytes = new ArraySegment<byte>[]
            {
            new ArraySegment<byte>(dataBuffer,offset,length)
            };
            this.OutputSend?.Invoke(this, false, transferBytes);
        }

        #endregion Socket同步直发

        #region Socket异步直发

        private void SocketSendAsync(short protocol, byte[] dataBuffer)
        {
            this.SocketSendAsync(protocol, dataBuffer, 0, dataBuffer.Length);
        }

        private void SocketSendAsync(short protocol)
        {
            this.SocketSendAsync(protocol, new byte[0], 0, 0);
        }

        private void SocketSendAsync(short protocol, byte[] dataBuffer, int offset, int length)
        {
            ArraySegment<byte>[] transferBytes = new ArraySegment<byte>[]
             {
            new ArraySegment<byte>(TouchSocketBitConverter.Default.GetBytes(protocol)),
            new ArraySegment<byte>(dataBuffer,offset,length)
             };
            this.OutputSend?.Invoke(this, true, transferBytes);
        }

        #endregion Socket异步直发

        #region Socket直发接口

        void IInternalRpc.SocketSend(short protocol, byte[] dataBuffer)
        {
            this.SocketSend(protocol, dataBuffer);
        }

        void IInternalRpc.SocketSend(short protocol, ByteBlock byteBlock)
        {
            this.SocketSend(protocol, byteBlock);
        }

        void IInternalRpc.SocketSend(short protocol)
        {
            this.SocketSend(protocol);
        }

        void IInternalRpc.SocketSend(short protocol, byte[] dataBuffer, int offset, int length)
        {
            this.SocketSend(protocol, dataBuffer, offset, length);
        }

        void IInternalRpc.SocketSendAsync(short protocol, byte[] dataBuffer)
        {
            this.SocketSendAsync(protocol, dataBuffer);
        }

        void IInternalRpc.SocketSendAsync(short protocol)
        {
            this.SocketSendAsync(protocol);
        }

        void IInternalRpc.SocketSendAsync(short protocol, byte[] dataBuffer, int offset, int length)
        {
            this.SocketSendAsync(protocol, dataBuffer, offset, length);
        }

        #endregion Socket直发接口
    }
}