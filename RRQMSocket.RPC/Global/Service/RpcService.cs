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
using RRQMCore.ByteManager;
using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// Rpc服务器类
    /// </summary>
    public class RpcService : IDisposable
    {
        private ProtocolService service;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcService()
        {
            this.ServerProviders = new ServerProviderCollection();
            this.RpcParsers = new RpcParserCollection();
            this.MethodMap = new MethodMap();
            this.Container = new Container();
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// 注入容器
        /// </summary>
        public IContainer Container { get; set; }

        /// <summary>
        /// 获取函数映射图实例
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 获取Rpc解析器集合
        /// </summary>
        public RpcParserCollection RpcParsers { get; private set; }

        /// <summary>
        /// 服务实例集合
        /// </summary>
        public ServerProviderCollection ServerProviders { get; private set; }

        /// <summary>
        /// 添加Rpc解析器，并将之前注册的服务应用
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="parser">解析器实例</param>
        public void AddRpcParser(string key, IRpcParser parser)
        {
            this.AddRpcParser(key, parser, true);
        }

        /// <summary>
        /// 添加Rpc解析器
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="parser">解析器实例</param>
        /// <param name="applyServer">是否应用已注册服务</param>
        public void AddRpcParser(string key, IRpcParser parser, bool applyServer)
        {
            this.RpcParsers.Add(key, parser);
            parser.SetRpcService(this);
            parser.SetExecuteMethod(this.PreviewExecuteMethod);
            parser.SetMethodMap(this.MethodMap);

            if (applyServer)
            {
                Dictionary<IServerProvider, List<MethodInstance>> pairs = new Dictionary<IServerProvider, List<MethodInstance>>();

                MethodInstance[] instances = this.MethodMap.GetAllMethodInstances();

                foreach (var item in instances)
                {
                    if (!pairs.ContainsKey(item.Provider))
                    {
                        pairs.Add(item.Provider, new List<MethodInstance>());
                    }

                    pairs[item.Provider].Add(item);
                }
                foreach (var item in pairs.Keys)
                {
                    parser.OnRegisterServer(item, pairs[item].ToArray());
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.StopShareProxy();
            foreach (var item in this.RpcParsers)
            {
                item.Dispose();
            }
            this.RpcParsers = null;
        }

        /// <summary>
        /// 从远程获取代理
        /// </summary>
        /// <param name="iPHost"></param>
        /// <param name="rpcType"></param>
        /// <param name="proxyToken"></param>
        /// <returns></returns>
        public RpcProxyInfo GetProxyInfo(IPHost iPHost, RpcType rpcType, string proxyToken)
        {
            ProtocolClient client = new ProtocolClient();
            client.Connecting += (client, e) =>
            {
                e.DataHandlingAdapter = new FixedHeaderPackageAdapter();
            };
            ByteBlock byteBlock = new ByteBlock();
            try
            {
                RRQMConfig config = new RRQMConfig();
                config.SetRemoteIPHost(iPHost);
                client.Setup(config).Connect();
                WaitSenderSubscriber subscriber = new WaitSenderSubscriber(100);
                client.AddProtocolSubscriber(subscriber);

                byteBlock.Write((int)rpcType);
                byteBlock.Write(proxyToken);
                byte[] data = subscriber.SendThenReturn(byteBlock);
                return RRQMCore.Serialization.SerializeConvert.RRQMBinaryDeserialize<RpcProxyInfo>(data, 0);
            }
            finally
            {
                byteBlock.Dispose();
                client.Dispose();
            }
        }

        /// <summary>
        /// 注册所有服务
        /// </summary>
        /// <returns>返回搜索到的服务数</returns>
        public int RegisterAllServer()
        {
            Type[] types = (AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes()).Where(p => typeof(ServerProvider).IsAssignableFrom(p) && !p.IsAbstract)).ToArray();

            foreach (Type type in types)
            {
                this.RegisterServer(type);
            }
            return types.Length;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IServerProvider RegisterServer<T>() where T : IServerProvider
        {
            return this.RegisterServer(typeof(T));
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public IServerProvider RegisterServer(Type providerType)
        {
            if (!typeof(IServerProvider).IsAssignableFrom(providerType))
            {
                throw new RpcException("类型不相符");
            }
            IServerProvider serverProvider = (IServerProvider)this.Container.Resolve(providerType);
            this.RegisterServer(serverProvider);
            return serverProvider;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverProvider"></param>
        public void RegisterServer(IServerProvider serverProvider)
        {
            serverProvider.RpcService = this;
            this.ServerProviders.Add(serverProvider);
            MethodInstance[] methodInstances = GlobalTools.GetMethodInstances(serverProvider);

            foreach (var item in methodInstances)
            {
                this.MethodMap.Add(item);
            }
            foreach (var parser in this.RpcParsers)
            {
                parser.OnRegisterServer(serverProvider, methodInstances);
            }
        }

        /// <summary>
        /// 移除Rpc解析器
        /// </summary>
        /// <param name="parserName"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool RemoveRpcParser(string parserName, out IRpcParser parser)
        {
            return this.RpcParsers.TryRemove(parserName, out parser);
        }

        /// <summary>
        /// 移除Rpc解析器
        /// </summary>
        /// <param name="parserName"></param>
        /// <returns></returns>
        public bool RemoveRpcParser(string parserName)
        {
            return this.RemoveRpcParser(parserName, out _);
        }

        /// <summary>
        /// 设置服务方法可用性
        /// </summary>
        /// <param name="methodToken">方法名</param>
        /// <param name="enable">可用性</param>
        /// <exception cref="RpcException"></exception>
        public void SetMethodEnable(int methodToken, bool enable)
        {
            if (this.MethodMap.TryGet(methodToken, out MethodInstance methodInstance))
            {
                methodInstance.IsEnable = enable;
            }
            else
            {
                throw new RpcException("未找到该方法");
            }
        }

        /// <summary>
        /// 分享代理。
        /// </summary>
        /// <param name="iPHost"></param>
        public void ShareProxy(IPHost iPHost)
        {
            if (this.service != null)
            {
                return;
            }
            this.service = new ProtocolService();
            this.service.Connecting += (client, e) =>
            {
                e.DataHandlingAdapter = new FixedHeaderPackageAdapter();
            };
            this.service.Received += this.Service_Received;
            this.service.Setup(new RRQMConfig().SetListenIPHosts(new IPHost[] { iPHost }));
            this.service.Start();
        }

        /// <summary>
        /// 停止分享代理。
        /// </summary>
        public void StopShareProxy()
        {
            if (this.service != null)
            {
                this.service.Dispose();
                this.service = null;
            }
        }

        /// <summary>
        /// 获取解析器
        /// </summary>
        /// <param name="parserKey"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool TryGetRpcParser(string parserKey, out IRpcParser parser)
        {
            return this.RpcParsers.TryGetRpcParser(parserKey, out parser);
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public int UnregisterServer(IServerProvider provider)
        {
            return this.UnregisterServer(provider.GetType());
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public int UnregisterServer(Type providerType)
        {
            if (!typeof(IServerProvider).IsAssignableFrom(providerType))
            {
                throw new RpcException("类型不相符");
            }
            this.ServerProviders.Remove(providerType);
            if (this.MethodMap.RemoveServer(providerType, out IServerProvider serverProvider, out MethodInstance[] instances))
            {
                foreach (var parser in this.RpcParsers)
                {
                    parser.OnUnregisterServer(serverProvider, instances);
                }

                return instances.Length;
            }
            return 0;
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int UnregisterServer<T>() where T : ServerProvider
        {
            return this.UnregisterServer(typeof(T));
        }

        private void ExecuteMethod(IRpcParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            if (methodInvoker.Status == InvokeStatus.Ready)
            {
                IServerProvider serverProvider = this.GetServerProvider(methodInvoker, methodInstance);
                try
                {
                    serverProvider.RpcEnter(parser, methodInvoker, methodInstance);
                    if (methodInstance.HasReturn)
                    {
                        methodInvoker.ReturnParameter = methodInstance.Invoke(serverProvider, methodInvoker.Parameters);
                    }
                    else
                    {
                        methodInstance.Invoke(serverProvider, methodInvoker.Parameters);
                    }
                    serverProvider.RpcLeave(parser, methodInvoker, methodInstance);
                    methodInvoker.Status = InvokeStatus.Success;
                }
                catch (AbandonRpcException e)
                {
                    methodInvoker.Status = InvokeStatus.Abort;
                    methodInvoker.StatusMessage = "函数被阻止执行，信息：" + e.Message;
                }
                catch (TargetInvocationException e)
                {
                    methodInvoker.Status = InvokeStatus.InvocationException;
                    if (e.InnerException != null)
                    {
                        methodInvoker.StatusMessage = "函数内部发生异常，信息：" + e.InnerException.Message;
                    }
                    else
                    {
                        methodInvoker.StatusMessage = "函数内部发生异常，信息：未知";
                    }
                    serverProvider.RpcError(parser, methodInvoker, methodInstance);
                }
                catch (Exception e)
                {
                    methodInvoker.Status = InvokeStatus.Exception;
                    methodInvoker.StatusMessage = e.Message;
                    serverProvider.RpcError(parser, methodInvoker, methodInstance);
                }
            }

            parser.OnEndInvoke(methodInvoker, methodInstance);
        }

        private IServerProvider GetServerProvider(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            return methodInstance.Provider;
        }

        /// <summary>
        /// 从本地获取代理
        /// </summary>
        /// <param name="rpcType"></param>
        /// <param name="proxyToken"></param>
        /// <returns></returns>
        public RpcProxyInfo GetProxyInfo(RpcType rpcType, string proxyToken)
        {
            GetProxyInfoArgs args = new GetProxyInfoArgs(proxyToken, rpcType);
            if (this.RpcParsers.Count == 0)
            {
                return new RpcProxyInfo() { IsSuccess = false, ErrorMessage = "没有可用解析器提供代理。" };
            }

            foreach (var item in this.RpcParsers)
            {
                item.GetProxyInfo(args);
                if (!args.Operation.HasFlag(RRQMCore.Operation.Permit))
                {
                    return new RpcProxyInfo()
                    {
                        IsSuccess = false,
                        ErrorMessage = args.Message,
                        Namespace = this.NameSpace,
                        Version = this.Version
                    };
                }
            }
            return new RpcProxyInfo() { IsSuccess = true, Version = this.Version, Namespace = this.NameSpace, Codes = args.Codes.ToArray() };
        }

        private void PreviewExecuteMethod(IRpcParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            if (methodInvoker.Status == InvokeStatus.Ready && methodInstance.AsyncType.HasFlag(AsyncType.Async))
            {
                Task.Run(() =>
                {
                    this.ExecuteMethod(parser, methodInvoker, methodInstance);
                });
            }
            else
            {
                this.ExecuteMethod(parser, methodInvoker, methodInstance);
            }
        }

        private void Service_Received(ProtocolSocketClient socketClient, short protocol, ByteBlock byteBlock)
        {
            switch (protocol)
            {
                case 100:
                    {
                        byteBlock.Pos = 2;
                        RpcType rpcType = (RpcType)byteBlock.ReadInt32();
                        string token = byteBlock.ReadString();
                        RpcProxyInfo rpcProxyInfo = this.GetProxyInfo(rpcType, token);
                        socketClient.Send(100, RRQMCore.Serialization.SerializeConvert.RRQMBinarySerialize(rpcProxyInfo));
                        break;
                    }
                default:
                    break;
            }
        }
    }
}