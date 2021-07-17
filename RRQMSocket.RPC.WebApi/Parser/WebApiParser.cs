//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
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
using RRQMCore.Helper;
using RRQMCore.Log;
using RRQMSocket.Http;
using System;
using System.Net.Sockets;
using System.Reflection;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// WebApi解析器
    /// </summary>
    public class WebApiParser : TcpService<SimpleSocketClient>, IRPCParser
    {
        private RouteMap routeMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiParser()
        {
            this.routeMap = new RouteMap();
        }

        /// <summary>
        /// 数据转化器
        /// </summary>
        public ApiDataConverter ApiDataConverter { get; private set; }

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 获取路由映射图
        /// </summary>
        public RouteMap RouteMap { get { return this.routeMap; } }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RPCService RPCService { get; private set; }

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
            HttpRequest httpRequest = (HttpRequest)methodInvoker.Flag;
            SimpleSocketClient socketClient = (SimpleSocketClient)methodInvoker.Caller;

            HttpResponse httpResponse = this.ApiDataConverter.OnResult(methodInvoker, methodInstance);

            httpResponse.ProtocolVersion = httpRequest.ProtocolVersion;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);

            try
            {
                httpResponse.Build(byteBlock);
                socketClient.Send(byteBlock);
            }
            finally
            {
                byteBlock.Dispose();
            }

            if (!httpRequest.KeepAlive)
            {
                socketClient.Shutdown(SocketShutdown.Both);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInstances"></param>
        public void OnRegisterServer(ServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if ((typeof(ControllerBase).IsAssignableFrom(methodInstance.Provider.GetType())))
                {
                    string controllerName;
                    RouteAttribute classAtt = methodInstance.Provider.GetType().GetCustomAttribute<RouteAttribute>(false);
                    if (classAtt == null || string.IsNullOrEmpty(classAtt.Template))
                    {
                        controllerName = methodInstance.Provider.GetType().Name;
                    }
                    else
                    {
                        controllerName = classAtt.Template.Replace("[controller]", methodInstance.Provider.GetType().Name);
                    }

                    foreach (var att in methodInstance.RPCAttributes)
                    {
                        if (att is RouteAttribute attribute)
                        {
                            if (methodInstance.IsByRef)
                            {
                                throw new RRQMRPCException("WebApi服务中不允许有out及ref关键字");
                            }
                            string actionUrl;

                            if (controllerName.Contains("[action]"))
                            {
                                actionUrl = controllerName.Replace("[action]", methodInstance.Method.Name);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(attribute.Template))
                                {
                                    actionUrl = $"{controllerName}/{methodInstance.Method.Name}";
                                }
                                else
                                {
                                    actionUrl = $"{controllerName}/{attribute.Template.Replace("[action]", methodInstance.Method.Name)}";
                                }
                            }

                            this.routeMap.Add(actionUrl, methodInstance);
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
        public void OnUnregisterServer(ServerProvider provider, MethodInstance[] methodInstances)
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
            this.MethodMap = methodMap;
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
        /// 载入配置
        /// </summary>
        /// <param name="serverConfig"></param>
        protected override void LoadConfig(ServiceConfig serverConfig)
        {
            base.LoadConfig(serverConfig);
            this.ApiDataConverter = (ApiDataConverter)serverConfig.GetValue(WebApiParserConfig.ApiDataConverterProperty);
        }

        /// <summary>
        /// 在初次接收时
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="createOption"></param>
        protected override void OnCreateSocketCliect(SimpleSocketClient socketClient, CreateOption createOption)
        {
            if (createOption.NewCreate)
            {
                socketClient.OnReceived = this.OnReceived;
            }
            socketClient.SetDataHandlingAdapter(new Http.HttpDataHandlingAdapter(this.BufferLength, HttpType.Server));
        }

        private void OnReceived(SimpleSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            HttpRequest httpRequest = (HttpRequest)obj;
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = socketClient;
            methodInvoker.Flag = httpRequest;

            if (this.routeMap.TryGet(httpRequest.RelativeURL, out MethodInstance methodInstance))
            {
                if (methodInstance.IsEnable)
                {
                    try
                    {
                        methodInvoker.Parameters = new object[methodInstance.Parameters.Length];
                        switch (httpRequest.Method)
                        {
                            case "GET":
                                {
                                    if (httpRequest.Query != null)
                                    {
                                        for (int i = 0; i < methodInstance.Parameters.Length; i++)
                                        {
                                            if (httpRequest.Query.TryGetValue(methodInstance.ParameterNames[i], out string value))
                                            {
                                                methodInvoker.Parameters[i] = value.ParseToType(methodInstance.ParameterTypes[i]);
                                            }
                                            else
                                            {
                                                methodInvoker.Parameters[i] = methodInstance.ParameterTypes[i].GetDefault();
                                            }
                                        }
                                    }
                                    break;
                                }
                            case "POST":
                                {
                                    this.ApiDataConverter.OnPost(httpRequest, ref methodInvoker, methodInstance);
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        methodInvoker.Status = InvokeStatus.Exception;
                        methodInvoker.StatusMessage = ex.Message;
                        this.Logger.Debug(LogType.Error, this, ex.Message, ex);
                    }
                }
                else
                {
                    methodInvoker.Status = InvokeStatus.UnEnable;
                }
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
            }

            this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
        }
    }
}