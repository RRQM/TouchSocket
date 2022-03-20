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
using RRQMSocket.Http;
using RRQMSocket.Http.Plugins;
using System;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// WebApi解析器
    /// </summary>
    public class WebApiParserPlugin : HttpPluginBase, IRpcParser
    {
        private RouteMap routeMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiParserPlugin()
        {
            this.routeMap = new RouteMap();
            this.ApiDataConverter = new JsonDataConverter();
        }

        /// <summary>
        /// 数据转化器
        /// </summary>
        public ApiDataConverter ApiDataConverter { get; set; }

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 设置代理令箭，当获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken { get; set; }

        /// <summary>
        /// 获取路由映射图
        /// </summary>
        public RouteMap RouteMap => this.routeMap;

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RpcService RpcService { get; private set; }

        /// <summary>
        /// 执行函数
        /// </summary>
        public Action<IRpcParser, MethodInvoker, MethodInstance> RRQMExecuteMethod { get; private set; }

        /// <summary>
        /// 获取WebApi代理
        /// </summary>
        /// <param name="args"></param>
        public virtual void GetProxyInfo(GetProxyInfoArgs args)
        {
            if (args.RpcType.HasFlag(RpcType.WebApi))
            {
                if (args.ProxyToken != this.ProxyToken)
                {
                    args.Message = "在验证WebApi时令箭不正确。";
                    args.RemoveOperation(Operation.Permit);
                    return;
                }
                foreach (var item in this.RpcService.ServerProviders)
                {
                    var serverCellCode1 = CodeGenerator.Generator<HttpGetAttribute>(item.GetType(), OnAction<HttpGetAttribute>);
                    args.Codes.Add(serverCellCode1);

                    var serverCellCode2 = CodeGenerator.Generator<HttpPostAttribute>(item.GetType(), OnAction<HttpPostAttribute>);
                    args.Codes.Add(serverCellCode2);
                }
            }
        }

        private bool OnAction<T>(MethodInstance methodInstance, MethodCellCode methodCellCode) where T : RpcAttribute
        {
            string actionUrl = GetUrl<T>(methodInstance);
            if (!string.IsNullOrEmpty(actionUrl))
            {
                if (typeof(T) == typeof(HttpGetAttribute))
                {
                    if (methodInstance.ParameterNames.Length > 0)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append(actionUrl);
                        stringBuilder.Append("?");
                        for (int i = 0; i < methodInstance.ParameterNames.Length; i++)
                        {
                            stringBuilder.Append(methodInstance.ParameterNames[i] + "={&}".Replace("&",i.ToString()));
                            if (i!= methodInstance.ParameterNames.Length-1)
                            {
                                stringBuilder.Append("&");
                            }
                        }
                        actionUrl = stringBuilder.ToString();
                    }
                    methodCellCode.InvokeKey = $"GET:{actionUrl}";
                }
                else if (typeof(T) == typeof(HttpPostAttribute))
                {
                    methodCellCode.InvokeKey = $"POST:{actionUrl}";
                }
            }
            return true;
        }

        /// <summary>
        /// 设置数据转换器
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        public WebApiParserPlugin SetApiDataConverter(ApiDataConverter converter)
        {
            this.ApiDataConverter = converter ?? throw new ArgumentNullException(nameof(converter));
            return this;
        }

        private string GetUrl<T>(MethodInstance methodInstance) where T : RpcAttribute
        {
            string controllerName;
            RouteAttribute classAtt = methodInstance.Info.DeclaringType.GetCustomAttribute<RouteAttribute>(false);
            if (classAtt == null || string.IsNullOrEmpty(classAtt.Template))
            {
                controllerName = methodInstance.Info.DeclaringType.Name;
            }
            else
            {
                controllerName = classAtt.Template.Replace("[controller]", methodInstance.Info.DeclaringType.Name);
            }
            controllerName = controllerName.Replace("Controller", string.Empty);

            foreach (var att in methodInstance.RpcAttributes)
            {
                if (att is T)
                {
                    string actionUrl;
                    if (controllerName.Contains("[action]"))
                    {
                        actionUrl = controllerName.Replace("[action]", CodeGenerator.GetMethodName<T>(methodInstance));
                    }
                    else
                    {
                        actionUrl = $"{controllerName}/{CodeGenerator.GetMethodName<T>(methodInstance)}";
                    }
                    return actionUrl;
                }
            }

            return default;
        }

        #region RPC解析器

        void IRpcParser.OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            HttpRequest httpRequest = (HttpRequest)methodInvoker.Flag;
            SocketClient socketClient = (SocketClient)methodInvoker.Caller;

            HttpResponse httpResponse = this.ApiDataConverter.OnResult(methodInvoker, methodInstance);

            httpResponse.ProtocolVersion = httpRequest.ProtocolVersion;
            ByteBlock byteBlock = BytePool.GetByteBlock();

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

        void IRpcParser.OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                string actionUrl = GetUrl<HttpGetAttribute>(methodInstance);
                if (!string.IsNullOrEmpty(actionUrl))
                {
                    this.routeMap.Add(actionUrl, methodInstance);
                }

                actionUrl = GetUrl<HttpPostAttribute>(methodInstance);
                if (!string.IsNullOrEmpty(actionUrl))
                {
                    this.routeMap.Add(actionUrl, methodInstance);
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
            this.MethodMap = methodMap;
        }

        void IRpcParser.SetRpcService(RpcService service)
        {
            this.RpcService = service;
        }

        #endregion RPC解析器

        /// <summary>
        /// 设置代理令箭，当获取代理文件时需验证令箭
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public WebApiParserPlugin SetProxyToken(string value)
        {
            this.ProxyToken = value;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.routeMap.TryGet(e.Request.RelativeURL, out MethodInstance methodInstance))
            {
                e.Handled = true;
                MethodInvoker methodInvoker = new MethodInvoker();
                methodInvoker.Caller = client;
                methodInvoker.Flag = e.Request;
                if (methodInstance.IsEnable)
                {
                    try
                    {
                        methodInvoker.Parameters = new object[methodInstance.Parameters.Length];
                        this.ApiDataConverter.OnPost(e.Request, ref methodInvoker, methodInstance);
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
                this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
            }
            
            base.OnPost(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.routeMap.TryGet(e.Request.RelativeURL, out MethodInstance methodInstance))
            {
                MethodInvoker methodInvoker = new MethodInvoker();
                methodInvoker.Caller = client;
                methodInvoker.Flag = e.Request;
                e.Handled = true;
                if (methodInstance.IsEnable)
                {
                    try
                    {
                        methodInvoker.Parameters = new object[methodInstance.Parameters.Length];
                        if (e.Request.Query != null)
                        {
                            for (int i = 0; i < methodInstance.Parameters.Length; i++)
                            {
                                if (e.Request.Query.TryGetValue(methodInstance.ParameterNames[i], out string value))
                                {
                                    methodInvoker.Parameters[i] = value.ParseToType(methodInstance.ParameterTypes[i]);
                                }
                                else
                                {
                                    methodInvoker.Parameters[i] = methodInstance.ParameterTypes[i].GetDefault();
                                }
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

                this.RRQMExecuteMethod.Invoke(this, methodInvoker, methodInstance);
            }

            base.OnGet(client, e);
        }
    }
}