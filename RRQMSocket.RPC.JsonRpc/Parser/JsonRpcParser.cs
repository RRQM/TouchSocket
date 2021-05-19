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
            try
            {
                RpcRequestContext context = this.BuildRequestContext(byteBlock, out methodInstance);
                if (methodInstance == null)
                {
                    methodInvoker.Status = InvokeStatus.UnFound;
                }
                else if (methodInstance.IsEnable)
                {
                    methodInvoker.Flag = context;
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

            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            this.BuildResponseByteBlock(byteBlock, methodInvoker, (RpcRequestContext)methodInvoker.Flag);
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
        /// <returns></returns>
        protected virtual RpcRequestContext BuildRequestContext(ByteBlock byteBlock, out MethodInstance methodInstance)
        {
            byteBlock.Seek(0, SeekOrigin.Begin);
            RpcRequestContext context = (RpcRequestContext)ReadObject(typeof(RpcRequestContext), byteBlock);

            if (this.actionMap.TryGet(context.method, out methodInstance))
            {
                if (context.@params != null)
                {
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
                            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(s));
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            context.@params[i] = ReadObject(type, memoryStream);
                        }
                    }
                }
            }
            else
            {
                methodInstance = null;
            }
            return context;
        }

        /// <summary>
        /// 构建响应数据
        /// </summary>
        /// <param name="responseByteBlock"></param>
        /// <param name="methodInvoker"></param>
        /// <param name="context"></param>
        protected virtual void BuildResponseByteBlock(ByteBlock responseByteBlock, MethodInvoker methodInvoker, RpcRequestContext context)
        {
            if (string.IsNullOrEmpty(context.id))
            {
                return;
            }

            if (methodInvoker.ReturnParameter != null)
            {
                this.WriteObject(responseByteBlock, methodInvoker.ReturnParameter);
            }
        }

        private object ReadObject(Type type, Stream stream)
        {
            DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(type);
            return deseralizer.ReadObject(stream);
        }

        private void WriteObject(Stream stream, object obj)
        {
            DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(obj.GetType());
            deseralizer.WriteObject(stream, obj);
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