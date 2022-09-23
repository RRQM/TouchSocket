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
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.XREF.Newtonsoft.Json;
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Http.WebSockets.Plugins;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.JsonRpc
{
    /// <summary>
    /// JsonRpcParser解析器插件
    /// </summary>
    public class JsonRpcParserPlugin : WebSocketPluginBase, IRpcParser
    {
        private readonly ActionMap m_actionMap;
        private readonly WaitCallback m_invokeWaitCallback;
        private string m_jsonRpcUrl = "/jsonrpc";
        private RpcStore m_rpcStore;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParserPlugin([DependencyParamterInject(true)] RpcStore rpcStore)
        {
            this.m_actionMap = new ActionMap();
            rpcStore?.AddRpcParser(this.GetType().Name, this);
            this.m_invokeWaitCallback = this.InvokeWaitCallback;
        }

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap => this.m_actionMap;

        /// <summary>
        /// 自动转换协议
        /// </summary>
        public bool AutoSwitch { get; set; } = true;

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        public string JsonRpcUrl
        {
            get => this.m_jsonRpcUrl;
            set => this.m_jsonRpcUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcStore RpcStore => this.m_rpcStore;

        /// <summary>
        /// 不需要自动转化协议。
        /// <para>仅当服务器是TCP时生效。此时如果携带协议为TcpJsonRpc时才会解释为jsonRpc。</para>
        /// </summary>
        /// <returns></returns>
        public JsonRpcParserPlugin NoSwitchProtocol()
        {
            this.AutoSwitch = false;
            return this;
        }

        #region RPC解析器

        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.m_actionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.m_actionMap.Remove(attribute.GetInvokenKey(methodInstance));
                }
            }
        }

        void IRpcParser.SetRpcStore(RpcStore rpcService)
        {
            this.m_rpcStore = rpcService;
        }

        #endregion RPC解析器

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        /// <param name="jsonRpcUrl"></param>
        /// <returns></returns>
        public JsonRpcParserPlugin SetJsonRpcUrl(string jsonRpcUrl)
        {
            this.JsonRpcUrl = jsonRpcUrl;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            if (this.AutoSwitch && client.Protocol == Protocol.TCP)
            {
                client.SwitchProtocolToTcpJsonRpc();
            }
            base.OnConnecting(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Text)
            {
                string jsonRpcStr = e.DataFrame.ToText();
                if (jsonRpcStr.Contains("jsonrpc"))
                {
                    e.Handled = true;
                    ThreadPool.QueueUserWorkItem(this.m_invokeWaitCallback, new JsonRpcCallContext()
                    {
                        Caller = client,
                        JRPT = JRPT.Websocket,
                        JsonString = jsonRpcStr
                    });
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.m_jsonRpcUrl == "/" || e.Context.Request.UrlEquals(this.m_jsonRpcUrl))
            {
                e.Handled = true;
                ThreadPool.QueueUserWorkItem(this.m_invokeWaitCallback, new JsonRpcCallContext()
                {
                    Caller = client,
                    JRPT = JRPT.Http,
                    JsonString = e.Context.Request.GetBody(),
                    HttpContext = e.Context
                });
            }
            base.OnPost(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (client.Protocol == JsonRpcConfigExtensions.TcpJsonRpc)
            {
                string jsonRpcStr = e.ByteBlock.ToString();
                if (jsonRpcStr.Contains("jsonrpc"))
                {
                    e.Handled = true;
                    ThreadPool.QueueUserWorkItem(this.m_invokeWaitCallback, new JsonRpcCallContext()
                    {
                        Caller = client,
                        JRPT = JRPT.Tcp,
                        JsonString = e.ByteBlock.ToString()
                    });
                }
            }
            base.OnReceivedData(client, e);
        }

        private void BuildRequestContext(ref JsonRpcCallContext callContext)
        {
            JsonRpcPackage jsonRpcPackage = JsonConvert.DeserializeObject<JsonRpcPackage>(callContext.JsonString);
            callContext.JsonRpcPackage = jsonRpcPackage;
            if (jsonRpcPackage.id != null)
            {
                jsonRpcPackage.needResponse = true;
            }

            if (this.m_actionMap.TryGetMethodInstance(jsonRpcPackage.method, out MethodInstance methodInstance))
            {
                callContext.MethodInstance = methodInstance;
                if (jsonRpcPackage.@params == null)
                {
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (methodInstance.ParameterNames.Length > 1)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        else
                        {
                            jsonRpcPackage.parameters = new object[] { callContext };
                        }
                    }
                    else
                    {
                        if (methodInstance.ParameterNames.Length != 0)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                    }
                }
                if (jsonRpcPackage.@params.GetType() != typeof(JArray))
                {
                    JObject obj = (JObject)jsonRpcPackage.@params;
                    jsonRpcPackage.parameters = new object[methodInstance.ParameterNames.Length];
                    //内联
                    int i = 0;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        jsonRpcPackage.parameters[0] = callContext;
                        i = 1;
                    }
                    for (; i < methodInstance.ParameterNames.Length; i++)
                    {
                        if (obj.TryGetValue(methodInstance.ParameterNames[i], out JToken jToken))
                        {
                            Type type = methodInstance.ParameterTypes[i];
                            jsonRpcPackage.parameters[i] = jToken.ToObject(type);
                        }
                        else if (methodInstance.Parameters[i].HasDefaultValue)
                        {
                            jsonRpcPackage.parameters[i] = methodInstance.Parameters[i].DefaultValue;
                        }
                        else
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                    }
                }
                else
                {
                    JArray array = (JArray)jsonRpcPackage.@params;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (array.Count != methodInstance.ParameterNames.Length - 1)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcPackage.parameters = new object[methodInstance.ParameterNames.Length];

                        jsonRpcPackage.parameters[0] = callContext;
                        for (int i = 0; i < array.Count; i++)
                        {
                            jsonRpcPackage.parameters[i + 1] = jsonRpcPackage.@params[i].ToObject(methodInstance.ParameterTypes[i + 1]);
                        }
                    }
                    else
                    {
                        if (array.Count != methodInstance.ParameterNames.Length)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcPackage.parameters = new object[methodInstance.ParameterNames.Length];

                        for (int i = 0; i < array.Count; i++)
                        {
                            jsonRpcPackage.parameters[i] = jsonRpcPackage.@params[i].ToObject(methodInstance.ParameterTypes[i]);
                        }
                    }
                }
            }
        }

        private void InvokeWaitCallback(object context)
        {
            JsonRpcCallContext callContext = (JsonRpcCallContext)context;
            InvokeResult invokeResult = new InvokeResult();

            try
            {
                this.BuildRequestContext(ref callContext);
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
            }

            if (callContext.MethodInstance != null)
            {
                if (!callContext.MethodInstance.IsEnable)
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
            }
            else
            {
                invokeResult.Status = InvokeStatus.UnFound;
            }

            if (invokeResult.Status == InvokeStatus.Ready)
            {
                IRpcServer rpcServer = callContext.MethodInstance.ServerFactory.Create(callContext, callContext.JsonRpcPackage.parameters);
                if (rpcServer is ITransientRpcServer transientRpcServer)
                {
                    transientRpcServer.CallContext = callContext;
                }

                invokeResult = this.m_rpcStore.Execute(rpcServer, callContext.JsonRpcPackage.parameters, callContext);
            }

            if (!callContext.JsonRpcPackage.needResponse)
            {
                return;
            }
            error error = null;
            switch (invokeResult.Status)
            {
                case InvokeStatus.Success:
                    {
                        break;
                    }
                case InvokeStatus.UnFound:
                    {
                        error = new error();
                        error.code = -32601;
                        error.message = "函数未找到";
                        break;
                    }
                case InvokeStatus.UnEnable:
                    {
                        error = new error();
                        error.code = -32601;
                        error.message = "函数已被禁用";
                        break;
                    }
                case InvokeStatus.InvocationException:
                    {
                        error = new error();
                        error.code = -32603;
                        error.message = "函数内部异常";
                        break;
                    }
                case InvokeStatus.Exception:
                    {
                        error = new error();
                        error.code = -32602;
                        error.message = invokeResult.Message;
                        break;
                    }
                default:
                    return;
            }

            this.Response(callContext, invokeResult.Result, error);
        }

        private void Response(JsonRpcCallContext callContext, object result, error error)
        {
            using (ByteBlock responseByteBlock = new ByteBlock())
            {
                JObject jobject = new JObject();
                if (error == null)
                {
                    //成功
                    jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                    if (result != null)
                    {
                        jobject.Add("result", JToken.FromObject(result));
                    }
                    else
                    {
                        jobject.Add("result", null);
                    }

                    jobject.Add("id", callContext.JsonRpcPackage.id == null ? null : JToken.FromObject(callContext.JsonRpcPackage.id));
                }
                else
                {
                    jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                    jobject.Add("error", JToken.FromObject(error));
                    jobject.Add("id", callContext.JsonRpcPackage.id == null ? null : JToken.FromObject(callContext.JsonRpcPackage.id));
                }

                ITcpClientBase client = (ITcpClientBase)callContext.Caller;
                if (callContext.JRPT == JRPT.Http)
                {
                    HttpResponse httpResponse = new HttpResponse();
                    httpResponse.FromJson(jobject.ToString(Formatting.None));
                    httpResponse.Build(responseByteBlock);
                    client.DefaultSend(responseByteBlock);
                }
                else if (callContext.JRPT == JRPT.Websocket)
                {
                    ((HttpSocketClient)client).SendWithWS(jobject.ToString(Formatting.None));
                }
                else
                {
                    responseByteBlock.Write(jobject.ToString(Formatting.None).ToUTF8Bytes());
                    client.Send(responseByteBlock);
                }
            }
        }
    }
}