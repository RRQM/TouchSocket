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
using RRQMCore.Log;
using RRQMCore.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// TCP RPC解释器
    /// </summary>
    public class TcpRPCParser : RRQMRPCParser, ITcpService<RPCSocketClient>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpRPCParser()
        {
            this.service = new TcpRPCService();
            this.service.Received += this.OnReceived;
            this.service.ClientConnected += this.Service_ClientConnected;
            this.service.ClientDisconnected += this.Service_ClientDisconnected;
        }

        private void Service_ClientDisconnected(object sender, MesEventArgs e)
        {
            this.ClientDisconnected?.Invoke(sender, e);
        }

        private void Service_ClientConnected(object sender, MesEventArgs e)
        {
            this.ClientConnected?.Invoke(service, e);
        }

        /// <summary>
        /// 获取或设置日志记录器
        /// </summary>
        public ILog Logger { get { return this.service.Logger; } set { this.service.Logger = value; } }

        /// <summary>
        /// 获取通信实例
        /// </summary>
        public TcpRPCService Service => this.service;

        /// <summary>
        /// 获取内存池实例
        /// </summary>
        public override sealed BytePool BytePool { get { return this.service.BytePool; } }

        /// <summary>
        /// 获取或设置缓存大小
        /// </summary>
        public int BufferLength { get { return this.service.BufferLength; } set { this.service.BufferLength = value; } }

        /// <summary>
        /// 服务状态
        /// </summary>
        public ServerState ServerState => this.service.ServerState;

        /// <summary>
        /// 服务配置
        /// </summary>
        public ServerConfig ServerConfig => this.service.ServerConfig;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxCount => this.service.MaxCount;

        /// <summary>
        /// 清理间隔
        /// </summary>
        public int ClearInterval => this.service.ClearInterval;

        private TcpRPCService service;

        /// <summary>
        /// 连接
        /// </summary>
        public event RRQMMessageEventHandler ClientConnected;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event RRQMMessageEventHandler ClientDisconnected;

        private void OnReceived(RPCSocketClient socketClient, ByteBlock byteBlock)
        {
            byte[] buffer = byteBlock.Buffer;
            int r = (int)byteBlock.Position;
            int agreement = BitConverter.ToInt32(buffer, 0);

            switch (agreement)
            {
                case 100:/*100，请求RPC文件*/
                    {
                        try
                        {
                            string proxyToken = null;
                            if (r - 4 > 0)
                            {
                                proxyToken = Encoding.UTF8.GetString(buffer, 4, r - 4);
                            }
                            socketClient.agreementHelper.SocketSend(100, SerializeConvert.RRQMBinarySerialize(this.GetProxyInfo(proxyToken, this), true));
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 100, 错误详情:{e.Message}");
                        }
                        break;
                    }

                case 101:/*函数调用*/
                    {
                        try
                        {
                            RpcContext content = RpcContext.Deserialize(buffer, 4);
                            this.ExecuteContext(content, socketClient);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 101, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 102:/*获取注册服务*/
                    {
                        try
                        {
                            byte[] data = SerializeConvert.RRQMBinarySerialize(this.GetRegisteredMethodItems(this, socketClient.ID), true);
                            socketClient.agreementHelper.SocketSend(102, data);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 102, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 110:/*数据返回*/
                    {
                        try
                        {
                            socketClient.Agreement_110(buffer, r);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 110, 错误详情:{e.Message}");
                        }
                        break;
                    }
                case 112:/*回调函数调用*/
                    {
                        try
                        {
                            socketClient.Agreement_112(buffer, r);
                        }
                        catch (Exception e)
                        {
                            Logger.Debug(LogType.Error, this, $"错误代码: 112, 错误详情:{e.Message}");
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 在调用结束后调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected override void EndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RpcContext context = (RpcContext)methodInvoker.Flag;
            if (context.Feedback == 0)
            {
                return;
            }
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            try
            {
                switch (methodInvoker.Status)
                {
                    case InvokeStatus.Ready:
                        {
                            break;
                        }

                    case InvokeStatus.UnFound:
                        {
                            context.Status = 2;
                            break;
                        }
                    case InvokeStatus.Success:
                        {
                            if (methodInstance.MethodToken > 50000000)
                            {
                                context.ReturnParameterBytes = this.SerializeConverter.SerializeParameter(methodInvoker.ReturnParameter);
                            }
                            else
                            {
                                context.ReturnParameterBytes = null;
                            }

                            if (methodInstance.IsByRef)
                            {
                                context.ParametersBytes = new List<byte[]>();
                                foreach (var item in methodInvoker.Parameters)
                                {
                                    context.ParametersBytes.Add(this.SerializeConverter.SerializeParameter(item));
                                }
                            }
                            else
                            {
                                context.ParametersBytes = null;
                            }

                            context.Status = 1;
                            break;
                        }
                    case InvokeStatus.Abort:
                        {
                            context.Status = 4;
                            context.Message = methodInvoker.StatusMessage;
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            context.Status = 3;
                            break;
                        }
                    case InvokeStatus.InvocationException:
                        {
                            break;
                        }
                    case InvokeStatus.Exception:
                        {
                            break;
                        }
                    default:
                        break;
                }

                context.Serialize(byteBlock);
                ((RPCSocketClient)methodInvoker.Caller).agreementHelper.SocketSend(101, byteBlock);
            }
            catch (Exception ex)
            {
                Logger.Debug(LogType.Error, this, ex.Message);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <typeparam name="T">返回值</typeparam>
        /// <param name="iDToken">ID</param>
        /// <param name="methodToken">函数唯一标识</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public T CallBack<T>(string iDToken, int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            if (this.Service.SocketClients.TryGetSocketClient(iDToken, out RPCSocketClient socketClient))
            {
                return socketClient.CallBack<T>(methodToken, invokeOption, parameters);
            }
            else
            {
                throw new RRQMRPCException("未找到该客户端");
            }
        }

        /// <summary>
        /// 回调RPC
        /// </summary>
        /// <param name="iDToken">ID</param>
        /// <param name="methodToken">函数唯一标识</param>
        /// <param name="invokeOption">调用设置</param>
        /// <param name="parameters">参数</param>
        public void CallBack(string iDToken, int methodToken, InvokeOption invokeOption = null, params object[] parameters)
        {
            if (this.Service.SocketClients.TryGetSocketClient(iDToken, out RPCSocketClient socketClient))
            {
                socketClient.CallBack(methodToken, invokeOption, parameters);
            }
            else
            {
                throw new RRQMRPCException("未找到该客户端");
            }
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="serverConfig"></param>
        public void Setup(ServerConfig serverConfig)
        {
            this.service.Setup(serverConfig);
            this.SerializeConverter = (SerializeConverter)serverConfig.GetValue(RRQMRPCParserConfig.SerializeConverterProperty);
            this.NameSpace = (string)serverConfig.GetValue(RRQMRPCParserConfig.NameSpaceProperty);
            this.RPCVersion = (Version)serverConfig.GetValue(RRQMRPCParserConfig.RPCVersionProperty);
            this.RPCCompiler = (IRPCCompiler)serverConfig.GetValue(RRQMRPCParserConfig.RPCCompilerProperty);
            this.ProxyToken = (string)serverConfig.GetValue(RRQMRPCParserConfig.ProxyTokenProperty);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="port"></param>
        public void Setup(int port)
        {
            this.service.Setup(port);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            this.service.serializeConverter = this.SerializeConverter;
            this.service.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            this.service.Stop();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            this.service.Dispose();
        }

        /// <summary>
        /// 判断Client是否在线
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SocketClientExist(string id)
        {
            return this.service.SocketClientExist(id);
        }

        /// <summary>
        /// 尝试获取Tclient
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        public bool TryGetSocketClient(string id, out RPCSocketClient socketClient)
        {
            return this.service.TryGetSocketClient(id, out socketClient);
        }
    }
}