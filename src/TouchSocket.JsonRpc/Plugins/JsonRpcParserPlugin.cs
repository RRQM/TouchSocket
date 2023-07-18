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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcParser解析器插件
    /// </summary>
    public class JsonRpcParserPlugin : PluginBase, IRpcParser, ITcpConnectingPlugin, IHttpPostPlugin, ITcpReceivedPlugin, IWebsocketReceivedPlugin
    {
        private string m_jsonRpcUrl = "/jsonrpc";

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParserPlugin(IContainer container)
        {
            if (container.IsRegistered(typeof(RpcStore)))
            {
                this.RpcStore = container.Resolve<RpcStore>();
            }
            else
            {
                this.RpcStore = new RpcStore(container);
            }
            this.ActionMap = new ActionMap(true);
            this.RpcStore.AddRpcParser(this);
        }

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap { get; private set; }

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
        public RpcStore RpcStore { get; private set; }

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
                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Remove(attribute.GetInvokenKey(methodInstance));
                }
            }
        }

        #endregion RPC解析器

        async Task IHttpPostPlugin<IHttpSocketClient>.OnHttpPost(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.m_jsonRpcUrl == "/" || e.Context.Request.UrlEquals(this.m_jsonRpcUrl))
            {
                e.Handled = true;
                this.ThisInvoke(new JsonRpcCallContext()
                {
                    Caller = client,
                    JRPT = JRPT.Http,
                    JsonString = e.Context.Request.GetBody(),
                    HttpContext = e.Context
                });
            }
            else
            {
                await e.InvokeNext();
            }
        }

        async Task ITcpConnectingPlugin<IClient>.OnTcpConnecting(IClient client, ConnectingEventArgs e)
        {
            if (this.AutoSwitch && client.Protocol == Protocol.TCP)
            {
                client.Protocol = JsonRpcUtility.JsonRpc;
            }

            await e.InvokeNext();
        }

        async Task ITcpReceivedPlugin<ITcpClientBase>.OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (client.Protocol == JsonRpcUtility.JsonRpc)
            {
                if (e.ByteBlock != null)
                {
                    var jsonRpcStr = e.ByteBlock.ToString();
                    if (jsonRpcStr.Contains("jsonrpc"))
                    {
                        e.Handled = true;
                        this.ThisInvoke(new JsonRpcCallContext()
                        {
                            Caller = client,
                            JRPT = JRPT.Tcp,
                            JsonString = e.ByteBlock.ToString()
                        });

                        return;
                    }
                }
            }

            await e.InvokeNext();
        }

        async Task IWebsocketReceivedPlugin<IHttpClientBase>.OnWebsocketReceived(IHttpClientBase client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Text)
            {
                var jsonRpcStr = e.DataFrame.ToText();
                if (jsonRpcStr.Contains("jsonrpc"))
                {
                    e.Handled = true;
                    this.ThisInvoke(new JsonRpcCallContext()
                    {
                        Caller = client,
                        JRPT = JRPT.Websocket,
                        JsonString = jsonRpcStr
                    });

                    return;
                }
            }
            else
            {
                await e.InvokeNext();
            }
        }

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

        private static void Response(JsonRpcCallContext callContext, object result, error error)
        {
            try
            {
                using (var responseByteBlock = new ByteBlock())
                {
                    object jobject;
                    if (error == null)
                    {
                        jobject = new JsonRpcSuccessResponse
                        {
                            result = result,
                            id = callContext.JsonRpcContext.id
                        };
                    }
                    else
                    {
                        jobject = new JsonRpcErrorResponse
                        {
                            error = error,
                            id = callContext.JsonRpcContext.id
                        };
                    }

                    var client = (ITcpClientBase)callContext.Caller;
                    if (callContext.JRPT == JRPT.Http)
                    {
                        var httpResponse = callContext.HttpContext.Response;
                        httpResponse.FromJson(jobject.ToJson());
                        httpResponse.Answer();
                    }
                    else if (callContext.JRPT == JRPT.Websocket)
                    {
                        ((HttpSocketClient)client).SendWithWS(jobject.ToJson());
                    }
                    else
                    {
                        responseByteBlock.Write(jobject.ToJson().ToUTF8Bytes());
                        client.Send(responseByteBlock);
                    }
                }
            }
            catch
            {
            }
        }

        private void BuildRequestContext(ref JsonRpcCallContext callContext)
        {
            var jsonRpcContext = SerializeConvert.JsonDeserializeFromString<JsonRpcContext>(callContext.JsonString);
            callContext.JsonRpcContext = jsonRpcContext;
            if (jsonRpcContext.id != null)
            {
                jsonRpcContext.needResponse = true;
            }

            if (this.ActionMap.TryGetMethodInstance(jsonRpcContext.method, out var methodInstance))
            {
                callContext.MethodInstance = methodInstance;
                if (jsonRpcContext.@params == null)
                {
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        jsonRpcContext.parameters = methodInstance.ParameterNames.Length > 1 ? throw new RpcException("调用参数计数不匹配") : (new object[] { callContext });
                    }
                    else
                    {
                        if (methodInstance.ParameterNames.Length != 0)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                    }
                }
                if (jsonRpcContext.@params is Dictionary<string, object> obj)
                {
                    jsonRpcContext.parameters = new object[methodInstance.ParameterNames.Length];
                    //内联
                    var i = 0;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        jsonRpcContext.parameters[0] = callContext;
                        i = 1;
                    }
                    for (; i < methodInstance.ParameterNames.Length; i++)
                    {
                        if (obj.TryGetValue(methodInstance.ParameterNames[i], out var jToken))
                        {
                            var type = methodInstance.ParameterTypes[i];
                            jsonRpcContext.parameters[i] = jToken.ToJson().FromJson(type);
                        }
                        else
                        {
                            if (methodInstance.Parameters[i].HasDefaultValue)
                            {
                                jsonRpcContext.parameters[i] = methodInstance.Parameters[i].DefaultValue;
                            }
                            else
                            {
                                throw new RpcException("调用参数计数不匹配");
                            }
                        }
                    }
                }
                else
                {
                    var array = (IList)jsonRpcContext.@params;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (array.Count != methodInstance.ParameterNames.Length - 1)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcContext.parameters = new object[methodInstance.ParameterNames.Length];

                        jsonRpcContext.parameters[0] = callContext;
                        for (var i = 0; i < array.Count; i++)
                        {
                            jsonRpcContext.parameters[i + 1] = array[i].ToJson().FromJson(methodInstance.ParameterTypes[i + 1]);
                        }
                    }
                    else
                    {
                        if (array.Count != methodInstance.ParameterNames.Length)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcContext.parameters = new object[methodInstance.ParameterNames.Length];

                        for (var i = 0; i < array.Count; i++)
                        {
                            jsonRpcContext.parameters[i] = array[i].ToJson().FromJson(methodInstance.ParameterTypes[i]);
                        }
                    }
                }
            }
        }

        private void ThisInvoke(JsonRpcCallContext callContext)
        {
            var invokeResult = new InvokeResult();

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
                var rpcServer = callContext.MethodInstance.ServerFactory.Create(callContext, callContext.JsonRpcContext.parameters);
                if (rpcServer is ITransientRpcServer transientRpcServer)
                {
                    transientRpcServer.CallContext = callContext;
                }

                invokeResult = RpcStore.Execute(rpcServer, callContext.JsonRpcContext.parameters, callContext);
            }

            if (!callContext.JsonRpcContext.needResponse)
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

            Response(callContext, invokeResult.Result, error);
        }
    }
}