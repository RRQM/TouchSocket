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
using System;
using System.Text;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRpcParser解析器
    /// </summary>
    public class JsonRpcParser : TcpService<SimpleSocketClient>, IRPCParser
    {
        private ActionMap actionMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParser()
        {
            this.actionMap = new ActionMap();
        }

        /// <summary>
        /// 函数键映射图
        /// </summary>
        public ActionMap ActionMap { get { return this.actionMap; } }

        /// <summary>
        /// Json转换器
        /// </summary>
        public JsonFormatConverter JsonFormatConverter { get; private set; }

        /// <summary>
        /// 函数映射
        /// </summary>
        public MethodMap MethodMap { get; private set; }

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
        public void RRQMEndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            ISocketClient socketClient = (ISocketClient)methodInvoker.Caller;

            JsonResponseContext context = new JsonResponseContext();
            context.id = ((JsonRequestContext)methodInvoker.Flag).id;
            context.result = methodInvoker.ReturnParameter;
            error error = new error();
            context.error = error;

            switch (methodInvoker.Status)
            {
                case InvokeStatus.Success:
                    {
                        context.error = null;
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

            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            this.BuildResponseByteBlock(byteBlock, context);
            if (socketClient.Online)
            {
                try
                {
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

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="providers"></param>
        /// <param name="methodInstances"></param>
        public void RRQMInitializeServers(ServerProviderCollection providers, MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                foreach (var att in methodInstance.RPCAttributes)
                {
                    if (att is JsonRpcAttribute attribute)
                    {
                        if (methodInstance.IsByRef)
                        {
                            throw new RRQMRPCException("JsonRpc服务中不允许有out及ref关键字");
                        }
                        string actionKey = string.IsNullOrEmpty(attribute.MethodKey) ? methodInstance.Method.Name : attribute.MethodKey;

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
        protected override void LoadConfig(ServerConfig serverConfig)
        {
            base.LoadConfig(serverConfig);
            this.JsonFormatConverter = (JsonFormatConverter)serverConfig.GetValue(JsonRpcParserConfig.JsonFormatConverterProperty);
        }

        /// <summary>
        /// 构建请求内容
        /// </summary>
        /// <param name="byteBlock">数据</param>
        /// <param name="methodInstance">调用服务实例</param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual void BuildRequestContext(ByteBlock byteBlock, out MethodInstance methodInstance, out JsonRequestContext context)
        {
            context = (JsonRequestContext)this.JsonFormatConverter.Deserialize(Encoding.UTF8.GetString(byteBlock.Buffer, 0, (int)byteBlock.Length), typeof(JsonRequestContext));

            if (this.actionMap.TryGet(context.method, out methodInstance))
            {
                if (context.@params != null)
                {
                    if (context.@params.Length != methodInstance.ParameterTypes.Length)
                    {
                        throw new RRQMRPCException("调用参数计数不匹配");
                    }
                    for (int i = 0; i < context.@params.Length; i++)
                    {
                        string s = context.@params[i].ToString();

                        Type type = methodInstance.ParameterTypes[i];
                        if (type.IsPrimitive || type == typeof(string))
                        {
                            context.@params[i] = s.ParseToType(type);
                        }
                        else
                        {
                            context.@params[i] = this.JsonFormatConverter.Deserialize(s, type);
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
        /// <param name="responseContext"></param>
        protected virtual void BuildResponseByteBlock(ByteBlock responseByteBlock, JsonResponseContext responseContext)
        {
            if (string.IsNullOrEmpty(responseContext.id))
            {
                return;
            }
            this.JsonFormatConverter.Serialize(responseByteBlock, responseContext);
        }

        /// <summary>
        /// 创建SocketCliect
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="createOption"></param>
        protected override void OnCreateSocketCliect(SimpleSocketClient socketClient, CreateOption createOption)
        {
            if (createOption.NewCreate)
            {
                socketClient.OnReceived = this.OnReceived;
            }
            socketClient.SetDataHandlingAdapter(new TerminatorDataHandlingAdapter(this.BufferLength, "\r\n"));
        }

        private void OnReceived(SimpleSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = socketClient;
            MethodInstance methodInstance = null;
            JsonRequestContext context = null;
            try
            {
                this.BuildRequestContext(byteBlock, out methodInstance, out context);

                if (methodInstance == null)
                {
                    methodInvoker.Status = InvokeStatus.UnFound;
                }
                else if (methodInstance.IsEnable)
                {
                    methodInvoker.Parameters = context.@params;
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