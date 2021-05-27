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
using RRQMCore.Exceptions;
using RRQMCore.Helper;
using RRQMCore.Log;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// WebApi解析器
    /// </summary>
    public class JsonRpcParser : RPCParser, IService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParser()
        {
            this.tcpService = new RRQMTcpService();
            this.actionMap = new ActionMap();
            this.tcpService.CreatSocketCliect += this.OnCreatSocketCliect;
            this.tcpService.OnReceived += this.OnReceived;
            this.JsonConverter = new DataContractJsonConverter();
        }

        /// <summary>
        /// 在初次接收时
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="creatOption"></param>
        private void OnCreatSocketCliect(RRQMSocketClient socketClient, CreatOption creatOption)
        {
            if (creatOption.NewCreate)
            {
                socketClient.DataHandlingAdapter = new TerminatorDataHandlingAdapter(this.BufferLength, "\r\n");
            }
        }

        private RRQMTcpService tcpService;

        /// <summary>
        /// 函数键映射图
        /// </summary>
        public ActionMap ActionMap { get { return this.actionMap; } }

        private ActionMap actionMap;

        /// <summary>
        /// 获取当前服务通信器
        /// </summary>
        public RRQMTcpService Service { get { return this.tcpService; } }

        /// <summary>
        /// 获取绑定状态
        /// </summary>
        public bool IsBind => this.tcpService.IsBind;

        /// <summary>
        /// 获取或设置缓存大小
        /// </summary>
        public int BufferLength { get { return this.tcpService.BufferLength; } set { this.tcpService.BufferLength = value; } }

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public BytePool BytePool => this.tcpService.BytePool;

        /// <summary>
        /// 获取或设置日志记录器
        /// </summary>
        public ILog Logger { get { return this.tcpService.Logger; } set { this.tcpService.Logger = value; } }

        /// <summary>
        /// Json转换器
        /// </summary>
        public JsonConverter JsonConverter { get; set; }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Bind(int port, int threadCount = 1)
        {
            this.tcpService.Bind(port, threadCount);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="iPHost">ip和端口号，格式如“127.0.0.1:7789”。IP可输入Ipv6</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Bind(IPHost iPHost, int threadCount)
        {
            this.tcpService.Bind(iPHost, threadCount);
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="addressFamily">寻址方案</param>
        /// <param name="endPoint">绑定节点</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public void Bind(AddressFamily addressFamily, EndPoint endPoint, int threadCount)
        {
            this.tcpService.Bind(addressFamily, endPoint, threadCount);
        }

        private void OnReceived(RRQMSocketClient socketClient, ByteBlock byteBlock, object obj)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Caller = socketClient;
            MethodInstance methodInstance = null;
            RpcRequestContext context = null;
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

            this.ExecuteMethod(methodInvoker, methodInstance);
        }

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected sealed override void EndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            ISocketClient socketClient = (ISocketClient)methodInvoker.Caller;

            RpcResponseContext context = new RpcResponseContext();
            context.id = ((RpcRequestContext)methodInvoker.Flag).id;
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
        /// <param name="methodInstances"></param>
        protected sealed override void InitializeServers(MethodInstance[] methodInstances)
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
        /// 构建请求内容
        /// </summary>
        /// <param name="byteBlock">数据</param>
        /// <param name="methodInstance">调用服务实例</param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual void BuildRequestContext(ByteBlock byteBlock, out MethodInstance methodInstance, out RpcRequestContext context)
        {
            context = (RpcRequestContext)this.JsonConverter.Deserialize(Encoding.UTF8.GetString(byteBlock.Buffer, 0, (int)byteBlock.Length), typeof(RpcRequestContext));

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
                            context.@params[i] = this.JsonConverter.Deserialize(s, type);
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
        protected virtual void BuildResponseByteBlock(ByteBlock responseByteBlock, RpcResponseContext responseContext)
        {
            if (string.IsNullOrEmpty(responseContext.id))
            {
                return;
            }
            this.JsonConverter.Serialize(responseByteBlock, responseContext);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            this.tcpService.Dispose();
        }
    }
}