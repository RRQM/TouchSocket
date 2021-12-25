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
using RRQMCore.XREF.Newtonsoft.Json;
using RRQMCore.XREF.Newtonsoft.Json.Linq;
using RRQMSocket.Http;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpcParser解析器
    /// </summary>
    public class JsonRpcParser : TcpService<JsonRpcSocketClient>, IRPCParser
    {
        private ActionMap actionMap;

        private MethodMap methodMap;

        private JsonRpcProtocolType protocolType;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParser()
        {
            this.idTypeInstance = new ConcurrentDictionary<string, ConcurrentDictionary<Type, IServerProvider>>();
            this.actionMap = new ActionMap();
        }

        /// <summary>
        /// 函数键映射图
        /// </summary>
        public ActionMap ActionMap
        { get { return this.actionMap; } }

        private int maxPackageSize;

        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public int MaxPackageSize
        {
            get { return maxPackageSize; }
        }

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap
        {
            get { return methodMap; }
        }

        /// <summary>
        /// 协议类型
        /// </summary>
        public JsonRpcProtocolType ProtocolType
        {
            get { return protocolType; }
        }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RPCService RPCService { get; private set; }

        private InvokeType invokeType;

        /// <summary>
        /// 调用类型
        /// </summary>
        public InvokeType InvokeType
        {
            get { return invokeType; }
        }

        /// <summary>
        /// 执行函数
        /// </summary>
        public Action<IRPCParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        public void OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            ISocketClient socketClient = (ISocketClient)methodInvoker.Caller;
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
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                this.BuildResponseByteBlock(byteBlock, methodInvoker, jsonRpcContext.id, methodInvoker.ReturnParameter, error);
                if (socketClient.Online)
                {
                    try
                    {
                        string s = Encoding.UTF8.GetString(byteBlock.ToArray());
                        socketClient.Send(byteBlock);
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

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        public void OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                foreach (var att in methodInstance.RPCAttributes)
                {
                    if (att is JsonRpcAttribute attribute)
                    {
                        if (methodInstance.IsByRef)
                        {
                            throw new RRQMRPCException($"JsonRpc服务中不允许有out及ref关键字，服务：{methodInstance.Method.Name}");
                        }
                        string actionKey = string.IsNullOrEmpty(attribute.MemberKey) ? methodInstance.Method.Name : attribute.MemberKey;

                        try
                        {
                            this.actionMap.Add(actionKey, methodInstance);
                        }
                        catch
                        {
                            throw new RRQMRPCException($"函数键为{actionKey}的方法已注册。");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取消注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        public void OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
        }

        /// <summary>
        /// 设置执行委托
        /// </summary>
        /// <param name="executeMethod"></param>
        public void SetExecuteMethod(Action<IRPCParser, MethodInvoker, MethodInstance> executeMethod)
        {
            this.RRQMExecuteMethod = executeMethod;
        }

        /// <summary>
        /// 设置地图映射
        /// </summary>
        /// <param name="methodMap"></param>
        public void SetMethodMap(MethodMap methodMap)
        {
            this.methodMap = methodMap;
        }

        /// <summary>
        /// 设置服务
        /// </summary>
        /// <param name="service"></param>
        public void SetRPCService(RPCService service)
        {
            this.RPCService = service;
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
                            throw new RRQMRPCException("调用参数计数不匹配");
                        }
                        else
                        {
                            JsonRpcServerCallContext jsonRpcServerCallContext = new JsonRpcServerCallContext();
                            jsonRpcServerCallContext.caller = methodInvoker.Caller;
                            jsonRpcServerCallContext.methodInvoker = methodInvoker;
                            jsonRpcServerCallContext.context = jsonRpcContext;
                            jsonRpcServerCallContext.jsonString = jsonString;
                            jsonRpcServerCallContext.methodInstance = methodInstance;
                            jsonRpcServerCallContext.protocolType = this.protocolType;
                            jsonRpcContext.parameters = new object[] { jsonRpcServerCallContext };
                        }
                    }
                    else
                    {
                        if (methodInstance.ParameterNames.Length != 0)
                        {
                            throw new RRQMRPCException("调用参数计数不匹配");
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
                        jsonRpcServerCallContext.protocolType = this.protocolType;
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
                            throw new RRQMRPCException("调用参数计数不匹配");
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
                            throw new RRQMRPCException("调用参数计数不匹配");
                        }
                        jsonRpcContext.parameters = new object[methodInstance.ParameterNames.Length];

                        JsonRpcServerCallContext jsonRpcServerCallContext = new JsonRpcServerCallContext();
                        jsonRpcServerCallContext.caller = methodInvoker.Caller;
                        jsonRpcServerCallContext.methodInvoker = methodInvoker;
                        jsonRpcServerCallContext.context = jsonRpcContext;
                        jsonRpcServerCallContext.jsonString = jsonString;
                        jsonRpcServerCallContext.methodInstance = methodInstance;
                        jsonRpcServerCallContext.protocolType = this.protocolType;

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
                            throw new RRQMRPCException("调用参数计数不匹配");
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
        /// 构建响应数据
        /// </summary>
        /// <param name="responseByteBlock"></param>
        /// <param name="methodInvoker"></param>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        protected virtual void BuildResponseByteBlock(ByteBlock responseByteBlock, MethodInvoker methodInvoker, string id, object result, error error)
        {
            JObject jobject = new JObject();
            if (error == null)
            {
                //成功
                jobject.Add("jsonrpc", JToken.FromObject("2.0"));
                if (result != null)
                {
                    if (result.GetType().FullName == "Newtonsoft.Json.Linq.JObject")
                    {
                        jobject.Add("result", JToken.Parse(((dynamic)result).ToString(0)));
                    }
                    else
                    {
                        jobject.Add("result", JToken.FromObject(result));
                    }
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
            switch (this.protocolType)
            {
                case JsonRpcProtocolType.Tcp:
                    {
                        responseByteBlock.Write(Encoding.UTF8.GetBytes(jobject.ToString(Formatting.None)));
                        break;
                    }
                case JsonRpcProtocolType.Http:
                    {
                        HttpResponse httpResponse = new HttpResponse();
                        httpResponse.FromJson(jobject.ToString(Formatting.None));
                        httpResponse.Build(responseByteBlock);
                        break;
                    }
            }
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(ServiceConfig serviceConfig)
        {
            base.LoadConfig(serviceConfig);
            this.protocolType = (JsonRpcProtocolType)serviceConfig.GetValue(JsonRpcParserConfig.ProtocolTypeProperty);
            this.maxPackageSize = (int)serviceConfig.GetValue(JsonRpcParserConfig.MaxPackageSizeProperty);
            this.invokeType = (InvokeType)serviceConfig.GetValue(JsonRpcParserConfig.InvokeTypeProperty);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(JsonRpcSocketClient socketClient, ClientOperationEventArgs e)
        {
            socketClient.Received += this.OnReceived;

            switch (this.protocolType)
            {
                case JsonRpcProtocolType.Tcp:
                    socketClient.InternalSetAdapter(new TerminatorDataHandlingAdapter(this.maxPackageSize, "\r\n"));
                    break;

                case JsonRpcProtocolType.Http:
                    socketClient.InternalSetAdapter(new HttpDataHandlingAdapter(this.maxPackageSize, HttpType.Server));
                    break;
            }
            base.OnConnecting(socketClient, e);
        }

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, IServerProvider>> idTypeInstance;

        private void OnReceived(SimpleSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = (JsonRpcSocketClient)socketClient;
            methodInvoker.InvokeType = this.invokeType;

            MethodInstance methodInstance = null;
            JsonRpcContext context = null;
            try
            {
                string jsonString = null;
                switch (this.protocolType)
                {
                    case JsonRpcProtocolType.Tcp:
                        {
                            jsonString = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                            break;
                        }
                    case JsonRpcProtocolType.Http:
                        {
                            HttpRequest httpRequest = (HttpRequest)obj;
                            jsonString = httpRequest.Body;
                            methodInvoker.Flag = httpRequest;
                            break;
                        }
                }
                this.BuildRequestContext(methodInvoker, jsonString, out methodInstance, out context);

                if (methodInstance == null)
                {
                    methodInvoker.Status = InvokeStatus.UnFound;
                }
                else if (methodInstance.IsEnable)
                {
                    methodInvoker.Parameters = context.parameters;
                    if (this.invokeType == InvokeType.CustomInstance)
                    {
                        ConcurrentDictionary<Type, IServerProvider> typeInstance;
                        if (!this.idTypeInstance.TryGetValue(socketClient.ID, out typeInstance))
                        {
                            typeInstance = new ConcurrentDictionary<Type, IServerProvider>();
                            this.idTypeInstance.TryAdd(socketClient.ID, typeInstance);
                        }

                        IServerProvider instance;
                        if (!typeInstance.TryGetValue(methodInstance.ProviderType, out instance))
                        {
                            instance = (IServerProvider)Activator.CreateInstance(methodInstance.ProviderType);
                            typeInstance.TryAdd(methodInstance.ProviderType, instance);
                        }

                        methodInvoker.CustomServerProvider = instance;
                    }
                }
                else
                {
                    methodInvoker.Status = InvokeStatus.UnEnable;
                }
            }
            catch (Exception ex)
            {
                methodInvoker.Status = InvokeStatus.Exception;
                methodInvoker.StatusMessage = ex.Message;
            }

            methodInvoker.Flag = context;

            this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
        }
    }
}