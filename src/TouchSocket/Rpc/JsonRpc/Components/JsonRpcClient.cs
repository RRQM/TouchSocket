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
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Core.Run;
using TouchSocket.Core.XREF.Newtonsoft.Json;
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.JsonRpc
{
    /// <summary>
    /// JsonRpc客户端
    /// </summary>
    public class JsonRpcClient : DisposableObject, ITcpClient, IJsonRpcClient
    {
        private readonly WaitHandlePool<IWaitResult> m_waitHandle;
        private JRPT m_jrpt;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcClient()
        {
            this.m_waitHandle = new WaitHandlePool<IWaitResult>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int BufferLength => this.Client.BufferLength;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend => this.Client.CanSend;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSetDataHandlingAdapter => this.Client.CanSetDataHandlingAdapter;

        /// <summary>
        /// 内部客户端
        /// </summary>
        public ITcpClient Client { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TouchSocketConfig Config => this.Client.Config;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MessageEventHandler<ITcpClient> Connected { get => this.Client.Connected; set => this.Client.Connected = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ClientConnectingEventHandler<ITcpClient> Connecting { get => this.Client.Connecting; set => this.Client.Connecting = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => ((IClient)this.Client).Container;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter => this.Client.DataHandlingAdapter;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ClientDisconnectedEventHandler<ITcpClientBase> Disconnected { get => this.Client.Disconnected; set => this.Client.Disconnected = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string IP => this.Client.IP;

        /// <summary>
        /// 协议类型
        /// </summary>
        public JRPT JRPT => this.m_jrpt;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastReceivedTime => this.Client.LastReceivedTime;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastSendTime => this.Client.LastSendTime;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ILog Logger => this.Client.Logger;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Socket MainSocket => this.Client.MainSocket;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int MaxPackageSize => this.Client.MaxPackageSize;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get => this.Client.OnHandleRawBuffer; set => this.Client.OnHandleRawBuffer = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get => this.Client.OnHandleReceivedData; set => this.Client.OnHandleReceivedData = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Online => this.Client.Online;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager => this.Client.PluginsManager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Port => this.Client.Port;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Protocol Protocol { get => this.Client.Protocol; set => this.Client.Protocol = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType => this.Client.ReceiveType;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPHost RemoteIPHost => this.Client.RemoteIPHost;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<IRpcClient, bool> TryCanInvoke { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UsePlugin => this.Client.UsePlugin;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl => this.Client.UseSsl;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Close()
        {
            this.Client.Close();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Close(string msg)
        {
            this.Client.Close(msg);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITcpClient Connect(int timeout = 5000)
        {
            this.Client.OnHandleReceivedData = this.HandleReceivedData;
            this.Client.Connect(timeout);
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Task<ITcpClient> ConnectAsync(int timeout = 5000)
        {
            return Task.Run(() =>
            {
                return this.Connect(timeout);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            this.Client.DefaultSend(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            this.Client.DefaultSendAsync(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Stream GetStream()
        {
            return this.Client.GetStream();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object GetValue(DependencyProperty dp)
        {
            return this.Client.GetValue(dp);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public T GetValue<T>(DependencyProperty dp)
        {
            return this.Client.GetValue<T>(dp);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Send(byte[] buffer, int offset, int length)
        {
            this.Client.Send(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Send(IRequestInfo requestInfo)
        {
            this.Client.Send(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Send(IList<ArraySegment<byte>> transferBytes)
        {
            this.Client.Send(transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SendAsync(byte[] buffer, int offset, int length)
        {
            this.Client.SendAsync(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SendAsync(IRequestInfo requestInfo)
        {
            this.Client.SendAsync(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            this.Client.SendAsync(transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            this.Client.SetDataHandlingAdapter(adapter);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITcpClient Setup(TouchSocketConfig config)
        {
            this.m_jrpt = config.GetValue<JRPT>(JsonRpcConfigExtensions.JRPTProperty);
            switch (this.m_jrpt)
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ITcpClient Setup(string ipHost)
        {
            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(ipHost));
            return this.Setup(config);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DependencyObject SetValue(DependencyProperty dp, object value)
        {
            return this.Client.SetValue(dp, value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Shutdown(SocketShutdown how)
        {
            this.Client.Shutdown(how);
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        private bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            switch (this.m_jrpt)
            {
                case JRPT.Http:
                    {
                        HttpResponse httpResponse = (HttpResponse)requestInfo;
                        JsonResponseContext responseContext = (JsonResponseContext)JsonConvert.DeserializeObject(httpResponse.GetBody(), typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitResult waitContext = new JsonRpcWaitResult();
                            waitContext.Status = 1;
                            waitContext.Sign = long.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.Return = responseContext.result;
                            this.m_waitHandle.SetRun(waitContext);
                        }
                        break;
                    }
                case JRPT.Websocket:
                    {
                        if (requestInfo is WSDataFrame dataFrame)
                        {
                            JsonResponseContext responseContext = (JsonResponseContext)JsonConvert.DeserializeObject(dataFrame.ToText(), typeof(JsonResponseContext));
                            if (responseContext != null)
                            {
                                JsonRpcWaitResult waitContext = new JsonRpcWaitResult();
                                waitContext.Status = 1;
                                waitContext.Sign = long.Parse(responseContext.id);
                                waitContext.error = responseContext.error;
                                waitContext.Return = responseContext.result;
                                this.m_waitHandle.SetRun(waitContext);
                            }
                        }
                        break;
                    }
                case JRPT.Tcp:
                default:
                    {
                        string jsonString = byteBlock.ToString();
                        JsonResponseContext responseContext = (JsonResponseContext)JsonConvert.DeserializeObject(jsonString, typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitResult waitContext = new JsonRpcWaitResult();
                            waitContext.Status = 1;
                            waitContext.Sign = long.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.Return = responseContext.result;
                            this.m_waitHandle.SetRun(waitContext);
                        }
                        break;
                    }
            }
            return true;
        }

        #region RPC调用

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            JsonRpcWaitResult context = new JsonRpcWaitResult();
            WaitData<IWaitResult> waitData = this.m_waitHandle.GetWaitData(context);

            using (ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                JObject jobject = new JObject();
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                jobject.Add("method", JToken.FromObject(method));
                if (parameters == null)
                {
                    parameters = new object[0];
                }
                jobject.Add("params", JToken.FromObject(parameters));

                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    jobject.Add("id", JToken.FromObject(context.Sign.ToString()));
                }
                else
                {
                    jobject.Add("id", null);
                }
                switch (this.m_jrpt)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jobject.ToString(Formatting.None)));
                            break;
                        }
                    case JRPT.Http:
                        {
                            HttpRequest request = new HttpRequest();
                            request.Method = "POST";
                            request.SetUrl(this.RemoteIPHost.GetUrlPath());
                            request.FromJson(jobject.ToString(Formatting.None));
                            request.Build(byteBlock);
                            break;
                        }
                    case JRPT.Websocket:
                        {
                            ((WebSocketClient)this.Client).SendWithWS(jobject.ToString(Formatting.None));
                            break;
                        }
                        
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SendAsync(byteBlock);
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
                            JsonRpcWaitResult resultContext = (JsonRpcWaitResult)waitData.WaitResult;
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
                            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                            {
                                return (T)resultContext.Return.ToString().ParseToType(typeof(T));
                            }

                            return JsonConvert.DeserializeObject<T>(resultContext.Return.ToString());
                        }
                    default:
                        return default;
                }
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="Exception"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            JsonRpcWaitResult context = new JsonRpcWaitResult();
            WaitData<IWaitResult> waitData = this.m_waitHandle.GetWaitData(context);

            using (ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength))
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                JObject jobject = new JObject();
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                jobject.Add("method", JToken.FromObject(method));
                if (parameters == null)
                {
                    parameters = new object[0];
                }
                jobject.Add("params", JToken.FromObject(parameters));
                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    jobject.Add("id", JToken.FromObject(context.Sign.ToString()));
                }
                else
                {
                    jobject.Add("id", null);
                }
                switch (this.m_jrpt)
                {
                    case JRPT.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jobject.ToString(Formatting.None)));
                            break;
                        }
                    case JRPT.Websocket:
                        {
                            ((WebSocketClient)this.Client).SendWithWS(jobject.ToString(Formatting.None));
                            break;
                        }
                    case JRPT.Http:
                        {
                            HttpRequest request = new HttpRequest();
                            request.Method = "POST";
                            request.SetUrl(this.RemoteIPHost.GetUrlPath());
                            request.FromJson(jobject.ToString(Formatting.None));
                            request.Build(byteBlock);
                        }
                        break;
                }
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.SendAsync(byteBlock);
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
                            JsonRpcWaitResult resultContext = (JsonRpcWaitResult)waitData.WaitResult;
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

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="Exception"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke<T>(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcException"></exception>
        /// <exception cref="Exception"></exception>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
             {
                 this.Invoke(method, invokeOption, parameters);
             });
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(method, invokeOption, parameters);
            });
        }

        #endregion RPC调用
    }
}