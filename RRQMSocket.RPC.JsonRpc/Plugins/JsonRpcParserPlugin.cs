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
using RRQMCore.Extensions;
using RRQMCore.Log;
using RRQMCore.XREF.Newtonsoft.Json;
using RRQMCore.XREF.Newtonsoft.Json.Linq;
using RRQMSocket.Http;
using RRQMSocket.Http.Plugins;
using System;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpcParser解析器插件
    /// </summary>
    public class JsonRpcParserPlugin : HttpPluginBase, IRpcParser
    {
        private ActionMap actionMap;
        private string jsonRpcUrl;
        private MethodMap methodMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParserPlugin(string jsonRpcUrl = "/jsonrpc")
        {
            this.actionMap = new ActionMap();
            this.JsonRpcUrl = jsonRpcUrl;
        }

        /// <summary>
        /// 函数键映射图
        /// </summary>
        public ActionMap ActionMap => this.actionMap;

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        public string JsonRpcUrl
        {
            get => this.jsonRpcUrl;
            set => this.jsonRpcUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap => this.methodMap;

        /// <summary>
        /// 代理令箭，当获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken { get; set; }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RpcService RpcService { get; private set; }

        /// <summary>
        /// 执行函数
        /// </summary>
        public Action<IRpcParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        public virtual void GetProxyInfo(GetProxyInfoArgs args)
        {
            if (args.RpcType.HasFlag(RpcType.JsonRpc))
            {
                if (args.ProxyToken != this.ProxyToken)
                {
                    args.Message = "在验证JsonRpc时令箭不正确。";
                    args.RemoveOperation(Operation.Permit);
                    return;
                }
                foreach (var item in this.RpcService.ServerProviders)
                {
                    var serverCellCode = CodeGenerator.Generator<JsonRpcAttribute>(item.GetType());
                    args.Codes.Add(serverCellCode);
                }
            }
        }

        #region RPC解析器

        void IRpcParser.OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            ISocketClient client = (ISocketClient)methodInvoker.Caller;
            error error = new error();

            switch (methodInvoker.Status)
            {
                case InvokeStatus.Success:
                    {
                        error = null;
                        break;
                    }
                case InvokeStatus.UnFound:
                    {
                        error.code = -32601;
                        error.message = "函数未找到";
                        break;
                    }
                case InvokeStatus.UnEnable:
                    {
                        error.code = -32601;
                        error.message = "函数已被禁用";
                        break;
                    }
                case InvokeStatus.Abort:
                    {
                        error.code = -32601;
                        error.message = "函数已被中断执行";
                        break;
                    }
                case InvokeStatus.InvocationException:
                    {
                        error.code = -32603;
                        error.message = "函数内部异常";
                        break;
                    }
                case InvokeStatus.Exception:
                    {
                        error.code = -32602;
                        error.message = methodInvoker.StatusMessage;
                        break;
                    }
            }
            JsonRpcContext jsonRpcContext = (JsonRpcContext)methodInvoker.Flag;
            if (jsonRpcContext.needResponse)
            {
                ByteBlock byteBlock = BytePool.GetByteBlock();
                this.BuildResponseByteBlock(client, byteBlock, methodInvoker, jsonRpcContext.id, methodInvoker.ReturnParameter, error);
                if (client.Online)
                {
                    try
                    {
                        client.Send(byteBlock);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Debug(LogType.Error, this, ex.Message);
                    }
                    finally
                    {
                        byteBlock.Dispose();
                    }
                }
            }
        }

        void IRpcParser.OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                foreach (var att in methodInstance.RpcAttributes)
                {
                    if (att is JsonRpcAttribute attribute)
                    {
                        if (methodInstance.IsByRef)
                        {
                            throw new RpcException($"JsonRpc服务中不允许有out及ref关键字，服务：{methodInstance.Name}");
                        }

                        string actionKey = CodeGenerator.GetMethodName<JsonRpcAttribute>(methodInstance);

                        try
                        {
                            this.actionMap.Add(actionKey, methodInstance);
                        }
                        catch
                        {
                            throw new RpcException($"函数键为{actionKey}的方法已注册。");
                        }
                    }
                }
            }
        }

        void IRpcParser.OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
        }

        void IRpcParser.SetExecuteMethod(Action<IRpcParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.RRQMExecuteMethod = executeMethod;
        }

        void IRpcParser.SetMethodMap(MethodMap methodMap)
        {
            this.methodMap = methodMap;
        }

        void IRpcParser.SetRpcService(RpcService service)
        {
            this.RpcService = service;
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
        /// 设置代理令箭，当获取代理文件时需验证令箭
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonRpcParserPlugin SetProxyToken(string value)
        {
            this.ProxyToken = value;
            return this;
        }

        /// <summary>
        /// 构建请求内容
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="jsonString">数据</param>
        /// <param name="methodInstance">调用服务实例</param>
        /// <param name="jsonRpcContext"></param>
        /// <returns></returns>
        protected virtual void BuildRequestContext(MethodInvoker methodInvoker, string jsonString, out MethodInstance methodInstance, out JsonRpcContext jsonRpcContext)
        {
            try
            {
                jsonRpcContext = JsonConvert.DeserializeObject<JsonRpcContext>(jsonString);
                if (jsonRpcContext.id != null)
                {
                    jsonRpcContext.needResponse = true;
                }
            }
            catch
            {
                jsonRpcContext = new JsonRpcContext();
                jsonRpcContext.needResponse = true;
                throw;
            }

            if (this.actionMap.TryGet(jsonRpcContext.method, out methodInstance))
            {
                if (jsonRpcContext.@params == null)
                {
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (methodInstance.ParameterNames.Length > 1)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        else
                        {
                            JsonRpcServerCallContext jsonRpcServerCallContext = new JsonRpcServerCallContext();
                            jsonRpcServerCallContext.caller = methodInvoker.Caller;
                            jsonRpcServerCallContext.methodInvoker = methodInvoker;
                            jsonRpcServerCallContext.context = jsonRpcContext;
                            jsonRpcServerCallContext.jsonString = jsonString;
                            jsonRpcServerCallContext.methodInstance = methodInstance;
                            jsonRpcContext.parameters = new object[] { jsonRpcServerCallContext };
                        }
                    }
                    else
                    {
                        if (methodInstance.ParameterNames.Length != 0)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                    }
                    return;
                }
                if (jsonRpcContext.@params.GetType() != typeof(JArray))
                {
                    JObject obj = (JObject)jsonRpcContext.@params;
                    jsonRpcContext.parameters = new object[methodInstance.ParameterNames.Length];
                    //内联
                    int i = 0;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        JsonRpcServerCallContext jsonRpcServerCallContext = new JsonRpcServerCallContext();
                        jsonRpcServerCallContext.caller = methodInvoker.Caller;
                        jsonRpcServerCallContext.methodInvoker = methodInvoker;
                        jsonRpcServerCallContext.context = jsonRpcContext;
                        jsonRpcServerCallContext.jsonString = jsonString;
                        jsonRpcServerCallContext.methodInstance = methodInstance;
                        jsonRpcContext.parameters[0] = jsonRpcServerCallContext;
                        i = 1;
                    }
                    for (; i < methodInstance.ParameterNames.Length; i++)
                    {
                        if (obj.TryGetValue(methodInstance.ParameterNames[i], out JToken jToken))
                        {
                            Type type = methodInstance.ParameterTypes[i];
                            jsonRpcContext.parameters[i] = jToken.ToObject(type);
                        }
                        else if (methodInstance.Parameters[i].HasDefaultValue)
                        {
                            jsonRpcContext.parameters[i] = methodInstance.Parameters[i].DefaultValue;
                        }
                        else
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                    }
                }
                else
                {
                    JArray array = (JArray)jsonRpcContext.@params;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (array.Count != methodInstance.ParameterNames.Length - 1)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcContext.parameters = new object[methodInstance.ParameterNames.Length];

                        JsonRpcServerCallContext jsonRpcServerCallContext = new JsonRpcServerCallContext();
                        jsonRpcServerCallContext.caller = methodInvoker.Caller;
                        jsonRpcServerCallContext.methodInvoker = methodInvoker;
                        jsonRpcServerCallContext.context = jsonRpcContext;
                        jsonRpcServerCallContext.jsonString = jsonString;
                        jsonRpcServerCallContext.methodInstance = methodInstance;

                        jsonRpcContext.parameters[0] = jsonRpcServerCallContext;
                        for (int i = 0; i < array.Count; i++)
                        {
                            jsonRpcContext.parameters[i + 1] = jsonRpcContext.@params[i].ToObject(methodInstance.ParameterTypes[i + 1]);
                        }
                    }
                    else
                    {
                        if (array.Count != methodInstance.ParameterNames.Length)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcContext.parameters = new object[methodInstance.ParameterNames.Length];

                        for (int i = 0; i < array.Count; i++)
                        {
                            jsonRpcContext.parameters[i] = jsonRpcContext.@params[i].ToObject(methodInstance.ParameterTypes[i]);
                        }
                    }
                }
            }
            else
            {
                methodInstance = null;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.jsonRpcUrl == "/" || e.Request.RelativeURL.Equals(this.jsonRpcUrl, StringComparison.OrdinalIgnoreCase))
            {
                MethodInvoker methodInvoker = new MethodInvoker();
                methodInvoker.Caller = client;
                MethodInstance methodInstance = null;
                JsonRpcContext context = null;
                try
                {
                    string jsonString = e.Request.GetBody();

                    methodInvoker.Flag = e.Request;

                    this.BuildRequestContext(methodInvoker, jsonString, out methodInstance, out context);

                    if (methodInstance == null)
                    {
                        methodInvoker.Status = InvokeStatus.UnFound;
                    }
                    else if (methodInstance.IsEnable)
                    {
                        methodInvoker.Parameters = context.parameters;
                    }
                    else
                    {
                        methodInvoker.Status = InvokeStatus.UnEnable;
                    }
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    methodInvoker.Status = InvokeStatus.Exception;
                    methodInvoker.StatusMessage = ex.Message;
                }

                methodInvoker.Flag = context;

                this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
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
            if (client.Protocol == Protocol.TCP)
            {
                MethodInvoker methodInvoker = new MethodInvoker();
                methodInvoker.Caller = client;
                MethodInstance methodInstance = null;
                JsonRpcContext context = null;
                try
                {
                    string jsonString = jsonString = e.ByteBlock.ToString();
                    this.BuildRequestContext(methodInvoker, jsonString, out methodInstance, out context);

                    if (methodInstance == null)
                    {
                        methodInvoker.Status = InvokeStatus.UnFound;
                    }
                    else if (methodInstance.IsEnable)
                    {
                        methodInvoker.Parameters = context.parameters;
                    }
                    else
                    {
                        methodInvoker.Status = InvokeStatus.UnEnable;
                    }

                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    methodInvoker.Status = InvokeStatus.Exception;
                    methodInvoker.StatusMessage = ex.Message;
                }

                methodInvoker.Flag = context;

                this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
            }

            base.OnReceivedData(client, e);
        }

        private void BuildResponseByteBlock(ITcpClientBase client, ByteBlock responseByteBlock, MethodInvoker methodInvoker, string id, object result, error error)
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

                jobject.Add("id", id == null ? null : JToken.FromObject(id));
            }
            else
            {
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                jobject.Add("error", JToken.FromObject(error));
                jobject.Add("id", id == null ? null : JToken.FromObject(id));
            }
            if (client.Protocol == Protocol.TCP)
            {
                responseByteBlock.Write(jobject.ToString(Formatting.None).ToUTF8Bytes());
            }
            else if (client.Protocol == Protocol.Http)
            {
                HttpResponse httpResponse = new HttpResponse();
                httpResponse.FromJson(jobject.ToString(Formatting.None));
                httpResponse.Build(responseByteBlock);
            }
            else
            {
                client.Logger.Warning("JsonRpc只能适用于TCP协议或HTTP协议");
            }
        }
    }
}