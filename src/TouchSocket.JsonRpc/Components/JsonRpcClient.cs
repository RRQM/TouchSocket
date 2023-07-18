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
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpc客户端
    /// </summary>
    public class JsonRpcClient : DisposableObject, ITcpClient, IJsonRpcClient
    {
        private readonly WaitHandlePool<IWaitResult> m_waitHandle;

        /// <summary>
        /// JsonRpc客户端
        /// </summary>
        public JsonRpcClient()
        {
            this.m_waitHandle = new WaitHandlePool<IWaitResult>();
            this.Protocol = JsonRpcUtility.JsonRpc;
        }

        /// <inheritdoc/>
        public int BufferLength => this.Client.BufferLength;

        /// <inheritdoc/>
        public bool CanSend => this.Client.CanSend;

        /// <inheritdoc/>
        public bool CanSetDataHandlingAdapter => this.Client.CanSetDataHandlingAdapter;

        /// <summary>
        /// 内部客户端
        /// </summary>
        public ITcpClient Client { get; private set; }

        /// <inheritdoc/>
        public TouchSocketConfig Config => this.Client.Config;

        /// <inheritdoc/>
        public ConnectedEventHandler<ITcpClient> Connected { get => this.Client.Connected; set => this.Client.Connected = value; }

        /// <inheritdoc/>
        public ConnectingEventHandler<ITcpClient> Connecting { get => this.Client.Connecting; set => this.Client.Connecting = value; }

        /// <inheritdoc/>
        public IContainer Container => ((IClient)this.Client).Container;

        /// <inheritdoc/>
        public TcpDataHandlingAdapter DataHandlingAdapter => this.Client.DataHandlingAdapter;

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnected { get => this.Client.Disconnected; set => this.Client.Disconnected = value; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnecting { get => this.Client.Disconnecting; set => this.Client.Disconnecting = value; }

        /// <inheritdoc/>
        public string IP => this.Client.IP;

        /// <inheritdoc/>
        public bool IsClient => this.Client.IsClient;

        /// <summary>
        /// 协议类型
        /// </summary>
        public JRPT JRPT { get; private set; }

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.Client.LastReceivedTime;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.Client.LastSendTime;

        /// <inheritdoc/>
        public ILog Logger => this.Client.Logger;

        /// <inheritdoc/>
        public Socket MainSocket => this.Client.MainSocket;

        /// <inheritdoc/>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get => this.Client.OnHandleRawBuffer; set => this.Client.OnHandleRawBuffer = value; }

        /// <inheritdoc/>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get => this.Client.OnHandleReceivedData; set => this.Client.OnHandleReceivedData = value; }

        /// <inheritdoc/>
        public bool Online => this.Client.Online;

        /// <inheritdoc/>
        public IPluginsManager PluginsManager => this.Client.PluginsManager;

        /// <inheritdoc/>
        public int Port => this.Client.Port;

        /// <inheritdoc/>
        public Protocol Protocol { get ; set; }

        /// <inheritdoc/>
        public ReceiveType ReceiveType => this.Client.ReceiveType;

        /// <inheritdoc/>
        public IPHost RemoteIPHost => this.Client.RemoteIPHost;


        /// <inheritdoc/>
        public bool UseSsl => this.Client.UseSsl;

        /// <inheritdoc/>
        public void Close(string msg)
        {
            this.Client.Close(msg);
        }

        /// <inheritdoc/>
        public ITcpClient Connect(int timeout = 5000)
        {
            this.Client.OnHandleReceivedData = this.HandleReceivedData;
            this.Client.Connect(timeout);
            return this;
        }

        /// <inheritdoc/>
        public Task<ITcpClient> ConnectAsync(int timeout = 5000)
        {
            return Task.Run(() =>
            {
                return this.Connect(timeout);
            });
        }

        /// <inheritdoc/>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            this.Client.DefaultSend(buffer, offset, length);
        }

        /// <inheritdoc/>
        public Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            return this.Client.DefaultSendAsync(buffer, offset, length);
        }

        /// <inheritdoc/>
        public Stream GetStream()
        {
            return this.Client.GetStream();
        }

        /// <inheritdoc/>
        public TValue GetValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return this.Client.GetValue(dp);
        }

        /// <inheritdoc/>
        public bool HasValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return this.Client.HasValue(dp);
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }

                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest
                {
                    method = method,
                    @params = parameters,
                    id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null
                };
                switch (this.JRPT)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                            break;
                        }
                    case JRPT.Http:
                        {
                            var request = new HttpRequest();
                            request.Method = HttpMethod.Post;
                            request.SetUrl(this.RemoteIPHost.PathAndQuery);
                            request.FromJson(jsonRpcRequest.ToJson());
                            request.Build(byteBlock);
                            break;
                        }
                    case JRPT.Websocket:
                        {
                            ((WebSocketClient)this.Client).SendWithWS(jsonRpcRequest.ToJson());
                            break;
                        }
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                            this.m_waitHandle.Destroy(waitData);

                            if (resultContext.Status == 0)
                            {
                                throw new TimeoutException("等待结果超时");
                            }
                            if (resultContext.error != null)
                            {
                                throw new RpcException(resultContext.error.message);
                            }

                            if (resultContext.Return == null)
                            {
                                return default;
                            }
                            else
                            {
                                if (returnType.IsPrimitive || returnType == typeof(string))
                                {
                                    return resultContext.Return.ToString().ParseToType(returnType);
                                }
                                else
                                {
                                    return resultContext.Return.ToJson().FromJson(returnType);
                                }
                            }
                        }
                    default:
                        return default;
                }
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest()
                {
                    method = method,
                    @params = parameters
                };

                jsonRpcRequest.id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null;
                switch (this.JRPT)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                            break;
                        }
                    case JRPT.Websocket:
                        {
                            ((WebSocketClient)this.Client).SendWithWS(jsonRpcRequest.ToJson());
                            break;
                        }
                    case JRPT.Http:
                        {
                            var request = new HttpRequest();
                            request.Method = HttpMethod.Post;
                            request.SetUrl(this.RemoteIPHost.PathAndQuery);
                            request.FromJson(jsonRpcRequest.ToJson());
                            request.Build(byteBlock);
                        }
                        break;
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                            this.m_waitHandle.Destroy(waitData);

                            if (resultContext.Status == 0)
                            {
                                throw new TimeoutException("等待结果超时");
                            }
                            if (resultContext.error != null)
                            {
                                throw new RpcException(resultContext.error.message);
                            }
                            break;
                        }
                    default:
                        return;
                }
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType,string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke(returnType,method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest()
                {
                    method = method,
                    @params = parameters
                };

                jsonRpcRequest.id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null;
                switch (this.JRPT)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                            break;
                        }
                    case JRPT.Websocket:
                        {
                            ((WebSocketClient)this.Client).SendWithWS(jsonRpcRequest.ToJson());
                            break;
                        }
                    case JRPT.Http:
                        {
                            var request = new HttpRequest();
                            request.Method = HttpMethod.Post;
                            request.SetUrl(this.RemoteIPHost.PathAndQuery);
                            request.FromJson(jsonRpcRequest.ToJson());
                            request.Build(byteBlock);
                        }
                        break;
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            await waitData.WaitAsync(invokeOption.Timeout);
                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                            this.m_waitHandle.Destroy(waitData);

                            if (resultContext.Status == 0)
                            {
                                throw new TimeoutException("等待结果超时");
                            }
                            if (resultContext.error != null)
                            {
                                throw new RpcException(resultContext.error.message);
                            }
                            break;
                        }
                    default:
                        return;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest
                {
                    method = method,
                    @params = parameters,
                    id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null
                };
                switch (this.JRPT)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(SerializeConvert.JsonSerializeToBytes(jsonRpcRequest));
                            break;
                        }
                    case JRPT.Http:
                        {
                            var request = new HttpRequest();
                            request.Method = HttpMethod.Post;
                            request.SetUrl(this.RemoteIPHost.PathAndQuery);
                            request.FromJson(jsonRpcRequest.ToJson());
                            request.Build(byteBlock);
                            break;
                        }
                    case JRPT.Websocket:
                        {
                            ((WebSocketClient)this.Client).SendWithWS(jsonRpcRequest.ToJson());
                            break;
                        }
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            await waitData.WaitAsync(invokeOption.Timeout);
                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                            this.m_waitHandle.Destroy(waitData);

                            if (resultContext.Status == 0)
                            {
                                throw new TimeoutException("等待结果超时");
                            }
                            if (resultContext.error != null)
                            {
                                throw new RpcException(resultContext.error.message);
                            }

                            if (resultContext.Return == null)
                            {
                                return default;
                            }
                            else
                            {
                                if (returnType.IsPrimitive || returnType == typeof(string))
                                {
                                    return resultContext.Return.ToString().ParseToType(returnType);
                                }
                                else
                                {
                                    return resultContext.Return.ToJson().FromJson(returnType);
                                }
                            }
                        }
                    default:
                        return default;
                }
            }
        }

        /// <inheritdoc/>
        public DependencyObject RemoveValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return this.Client.RemoveValue(dp);
        }

        /// <inheritdoc/>
        public void Send(byte[] buffer, int offset, int length)
        {
            this.Client.Send(buffer, offset, length);
        }

        /// <inheritdoc/>
        public void Send(IRequestInfo requestInfo)
        {
            this.Client.Send(requestInfo);
        }

        /// <inheritdoc/>
        public void Send(IList<ArraySegment<byte>> transferBytes)
        {
            this.Client.Send(transferBytes);
        }

        /// <inheritdoc/>
        public Task SendAsync(byte[] buffer, int offset, int length)
        {
            return this.Client.SendAsync(buffer, offset, length);
        }

        /// <inheritdoc/>
        public Task SendAsync(IRequestInfo requestInfo)
        {
            return this.Client.SendAsync(requestInfo);
        }

        /// <inheritdoc/>
        public Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return this.Client.SendAsync(transferBytes);
        }

        /// <inheritdoc/>
        public int SetBufferLength(int value)
        {
            return this.Client.SetBufferLength(value);
        }

        /// <inheritdoc/>
        public void SetDataHandlingAdapter(TcpDataHandlingAdapter adapter)
        {
            this.Client.SetDataHandlingAdapter(adapter);
        }

        /// <inheritdoc/>
        public ITcpClient Setup(TouchSocketConfig config)
        {
            this.JRPT = config.GetValue(JsonRpcConfigExtensions.JRPTProperty);
            switch (this.JRPT)
            {
                case JRPT.Http:
                    this.Client ??= new HttpClient();
                    break;

                case JRPT.Websocket:
                    this.Client ??= new WebSocketClient();
                    break;

                case JRPT.Tcp:
                default:
                    this.Client ??= new Sockets.TcpClient();
                    break;
            }
            return this.Client.Setup(config);
        }

        /// <inheritdoc/>
        public ITcpClient Setup(string ipHost)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(ipHost));
            return this.Setup(config);
        }

        /// <inheritdoc/>
        public DependencyObject SetValue<TValue>(IDependencyProperty<TValue> dp, TValue value)
        {
            return this.Client.SetValue(dp, value);
        }

        /// <inheritdoc/>
        public bool TryGetValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            return this.Client.TryGetValue(dp, out value);
        }

        /// <inheritdoc/>
        public bool TryRemoveValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            return this.Client.TryRemoveValue(dp, out value);
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        private bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            string jsonString = null;
            switch (this.JRPT)
            {
                case JRPT.Http:
                    {
                        var httpResponse = (HttpResponse)requestInfo;
                        jsonString = httpResponse.GetBody();
                        break;
                    }
                case JRPT.Websocket:
                    {
                        if (requestInfo is WSDataFrame dataFrame && dataFrame.Opcode == WSDataType.Text)
                        {
                            jsonString = dataFrame.ToText();
                        }
                        break;
                    }
                case JRPT.Tcp:
                default:
                    {
                        if (byteBlock == null)
                        {
                            if (requestInfo is IJsonRpcRequestInfo jsonRpcRequest)
                            {
                                jsonString = jsonRpcRequest.GetJsonRpcString();
                            }
                        }
                        else
                        {
                            jsonString = byteBlock.ToString();
                        }
                        break;
                    }
            }

            if (string.IsNullOrEmpty(jsonString))
            {
                return true;
            }

            try
            {
                if (jsonString.Contains("error") || jsonString.Contains("result"))
                {
                    var responseContext = jsonString.FromJson<JsonResponseContext>();
                    if (responseContext != null && !responseContext.id.IsNullOrEmpty())
                    {
                        var waitContext = new JsonRpcWaitResult
                        {
                            Status = 1,
                            Sign = long.Parse(responseContext.id),
                            error = responseContext.error,
                            Return = responseContext.result
                        };
                        this.m_waitHandle.SetRun(waitContext);
                    }
                }
            }
            catch
            {
            }
            return true;
        }
    }
}