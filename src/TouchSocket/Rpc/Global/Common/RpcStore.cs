//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Http;
using TouchSocket.Http.Plugins;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// Rpc仓库
    /// </summary>
    public class RpcStore : DisposableObject, IEnumerable<IRpcParser>
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        public const string Namespace = "namespace";

        /// <summary>
        /// 代理键
        /// </summary>
        public const string ProxyKey = "proxy";

        private static readonly ConcurrentDictionary<string, Type> m_proxyAttributeMap = new ConcurrentDictionary<string, Type>();
        private string m_proxyUrl = "/proxy";
        private HttpService m_service;
        private readonly ConcurrentDictionary<string, IRpcParser> m_parsers = new ConcurrentDictionary<string, IRpcParser>();

        static RpcStore()
        {
            SearchAttribute();
        }

        /// <summary>
        /// 实例化一个Rpc仓库。
        /// <para>需要指定<see cref="IContainer"/>容器。一般和对应的服务器、客户端共用一个容器比较好。</para>
        /// <para>如果，仅仅是只有一个解析器的话，可以考虑从配置<see cref="TouchSocketConfig"/>中，调用<see cref="RpcConfigExtensions.ConfigureRpcStore(TouchSocketConfig, Action{RpcStore})"/></para>
        /// </summary>
        public RpcStore(IContainer container)
        {
            this.ServerProviders = new RpcServerCollection();
            this.MethodMap = new MethodMap();
            this.Container = container ?? throw new ArgumentNullException(nameof(container));
            this.Container.RegisterSingleton<RpcStore>(this);
            SearchAttribute();
        }

        /// <summary>
        /// 代理属性映射。
        /// </summary>
        public static ConcurrentDictionary<string, Type> ProxyAttributeMap => m_proxyAttributeMap;

        /// <summary>
        /// 内置IOC容器
        /// </summary>
        public IContainer Container { get; private set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count => this.m_parsers.Count;

        /// <summary>
        /// 函数映射图实例
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 请求代理。
        /// </summary>
        public Func<HttpRequest, bool> OnRequestProxy { get; set; }

        /// <summary>
        /// 代理路径。默认为“/proxy”。
        /// <para>必须以“/”开头</para>
        /// </summary>
        public string ProxyUrl
        {
            get => this.m_proxyUrl;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "/";
                }
                this.m_proxyUrl = value;
            }
        }

        /// <summary>
        /// 服务实例集合
        /// </summary>
        public RpcServerCollection ServerProviders { get; private set; }

        /// <summary>
        /// 获取IRpcParser
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IRpcParser this[string key] => this.m_parsers[key];

        /// <summary>
        /// 从远程获取代理
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetProxyInfo(string url)
        {
            string result = Get(url);
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("未知错误");
            }
            return result;
        }

        /// <summary>
        /// 添加Rpc解析器
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="parser">解析器实例</param>
        /// <param name="applyServer">是否应用已注册服务</param>
        public void AddRpcParser(string key, IRpcParser parser, bool applyServer = true)
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!this.m_parsers.TryAdd(key, parser))
            {
                throw new System.Exception("相同键值得解析器已经存在。");
            }
            parser.SetRpcStore(this);
            if (applyServer)
            {
                Dictionary<IRpcServer, List<MethodInstance>> pairs = new Dictionary<IRpcServer, List<MethodInstance>>();

                MethodInstance[] instances = this.MethodMap.GetAllMethodInstances();

                foreach (var item in instances)
                {
                    if (!pairs.ContainsKey(item.Server))
                    {
                        pairs.Add(item.Server, new List<MethodInstance>());
                    }

                    pairs[item.Server].Add(item);
                }
                foreach (var item in pairs.Keys)
                {
                    parser.OnRegisterServer(item, pairs[item].ToArray());
                }
            }
        }

        /// <summary>
        /// 执行Rpc
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="methodInstance"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public InvokeResult Execute(object caller, MethodInstance methodInstance, object[] ps)
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            InvokeResult invokeResult = new InvokeResult();
            IRpcServer serverProvider = this.GetServerProvider(methodInstance);
            try
            {
                if (methodInstance.Filters != null)
                {
                    for (int i = 0; i < methodInstance.Filters.Length; i++)
                    {
                        methodInstance.Filters[i].Executing(caller, ref invokeResult, methodInstance, ps);
                    }
                }

                if (invokeResult.Status != InvokeStatus.Ready)
                {
                    return invokeResult;
                }

                if (methodInstance.HasReturn)
                {
                    invokeResult.Result = methodInstance.Invoke(serverProvider, ps);
                }
                else
                {
                    methodInstance.Invoke(serverProvider, ps);
                }
                invokeResult.Status = InvokeStatus.Success;
                if (methodInstance.Filters != null)
                {
                    for (int i = 0; i < methodInstance.Filters.Length; i++)
                    {
                        methodInstance.Filters[i].Executed(caller, ref invokeResult, methodInstance);
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                invokeResult.Status = InvokeStatus.InvocationException;
                if (ex.InnerException != null)
                {
                    invokeResult.Message = "函数内部发生异常，信息：" + ex.InnerException.Message;
                }
                else
                {
                    invokeResult.Message = "函数内部发生异常，信息：未知";
                }
                if (methodInstance.Filters != null)
                {
                    for (int i = 0; i < methodInstance.Filters.Length; i++)
                    {
                        methodInstance.Filters[i].ExecutException(caller, ref invokeResult, methodInstance, ex);
                    }
                }
            }
            catch (System.Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
                if (methodInstance.Filters != null)
                {
                    for (int i = 0; i < methodInstance.Filters.Length; i++)
                    {
                        methodInstance.Filters[i].ExecutException(caller, ref invokeResult, methodInstance, ex);
                    }
                }
            }

            return invokeResult;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_parsers.Values.GetEnumerator();
        }

        /// <summary>
        /// 返回枚举对象
        /// </summary>
        /// <returns></returns>
        IEnumerator<IRpcParser> IEnumerable<IRpcParser>.GetEnumerator()
        {
            return this.m_parsers.Values.GetEnumerator();
        }

        /// <summary>
        /// 本地获取代理
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="attrbuteTypes"></param>
        /// <returns></returns>
        public string GetProxyCodes(string @namespace, Type[] attrbuteTypes)
        {
            var cellCodes = this.GetProxyInfo(attrbuteTypes == null ? ProxyAttributeMap.Values.ToArray() : attrbuteTypes);
            return CodeGenerator.ConvertToCode(@namespace, cellCodes);
        }

        /// <summary>
        /// 本地获取代理
        /// </summary>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public string GetProxyCodes(string @namespace)
        {
            return this.GetProxyCodes(@namespace, null);
        }

        /// <summary>
        /// 从本地获取代理
        /// </summary>
        /// <returns></returns>
        public ServerCellCode[] GetProxyInfo()
        {
            return this.GetProxyInfo(ProxyAttributeMap.Values.ToArray());
        }

        /// <summary>
        /// 从本地获取代理
        /// </summary>
        /// <param name="attrbuteType"></param>
        /// <returns></returns>
        public ServerCellCode[] GetProxyInfo(Type[] attrbuteType)
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            List<ServerCellCode> codes = new List<ServerCellCode>();

            var types = this.ServerProviders.GetRegisterTypes();

            foreach (var attrbute in attrbuteType)
            {
                foreach (var item in types)
                {
                    ServerCellCode serverCellCode = CodeGenerator.Generator(item, attrbute);
                    codes.Add(serverCellCode);
                }
            }
            return codes.ToArray();
        }

        /// <summary>
        /// 移除Rpc解析器
        /// </summary>
        /// <param name="parserName"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool RemoveRpcParser(string parserName, out IRpcParser parser)
        {
            return this.m_parsers.TryRemove(parserName, out parser);
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
        /// <param name="methodKey">方法名</param>
        /// <param name="enable">可用性</param>
        /// <exception cref="RpcException"></exception>
        public void SetMethodEnable(string methodKey, bool enable)
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.MethodMap.TryGet(methodKey, out MethodInstance methodInstance))
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
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.m_service != null)
            {
                return;
            }
            this.m_service = new HttpService();
            this.m_service.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { iPHost }))
                .Start();

            this.m_service.AddPlugin(new InternalPlugin(this));
        }

        /// <summary>
        /// 关闭分享中心
        /// </summary>
        public void StopShareProxy()
        {
            this.m_service.SafeDispose();
        }

        /// <summary>
        /// 获取IRpcParser
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool TryGetRpcParser(string key, out IRpcParser parser)
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            return this.m_parsers.TryGetValue(key, out parser);
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public int UnregisterServer(IRpcServer provider)
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
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (!typeof(IRpcServer).IsAssignableFrom(providerType))
            {
                throw new RpcException("类型不相符");
            }
            this.ServerProviders.Remove(providerType);
            if (this.MethodMap.RemoveServer(providerType, out IRpcServer serverProvider, out MethodInstance[] instances))
            {
                foreach (var parser in this)
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
        public int UnregisterServer<T>() where T : RpcServer
        {
            return this.UnregisterServer(typeof(T));
        }

        internal bool TryRemove(string key, out IRpcParser parser)
        {
            return this.m_parsers.TryRemove(key, out parser);
        }

        #region 注册

        /// <summary>
        /// 注册所有服务
        /// </summary>
        /// <returns>返回搜索到的服务数</returns>
        public int RegisterAllServer()
        {
            List<Type> types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    Type[] t1 = assembly.GetTypes().Where(p => typeof(IRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass).ToArray();
                    types.AddRange(t1);
                }
                catch
                {
                }
            }

            foreach (Type type in types)
            {
                this.RegisterServer(type);
            }
            return types.Count;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IRpcServer RegisterServer<T>() where T : IRpcServer
        {
            return this.RegisterServer(typeof(T));
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public IRpcServer RegisterServer(Type providerType)
        {
            if (!typeof(IRpcServer).IsAssignableFrom(providerType))
            {
                throw new RpcException("类型不相符");
            }
            this.Container.RegisterSingleton(providerType, providerType);
            IRpcServer serverProvider = (IRpcServer)this.Container.Resolve(providerType);
            this.RegisterServer(serverProvider);
            return serverProvider;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverProvider"></param>
        public void RegisterServer(IRpcServer serverProvider)
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            this.ServerProviders.Add(serverProvider.GetType(), serverProvider);
            MethodInstance[] methodInstances = CodeGenerator.GetMethodInstances(serverProvider.GetType());

            foreach (var item in methodInstances)
            {
                item.Server = serverProvider;
                this.MethodMap.Add(item);
            }
            foreach (var parser in this)
            {
                parser.OnRegisterServer(serverProvider, methodInstances);
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <returns></returns>
        public IRpcServer RegisterServer<TFrom, TTo>() where TFrom : class, IRpcServer where TTo : TFrom
        {
            return this.RegisterServer(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="providerInterfaceType"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public IRpcServer RegisterServer(Type providerInterfaceType, Type providerType)
        {
            if (!typeof(IRpcServer).IsAssignableFrom(providerInterfaceType))
            {
                throw new RpcException("类型不相符");
            }

            if (!typeof(IRpcServer).IsAssignableFrom(providerType))
            {
                throw new RpcException("类型不相符");
            }

            this.Container.RegisterSingleton(providerInterfaceType, providerType);
            IRpcServer serverProvider = (IRpcServer)this.Container.Resolve(providerInterfaceType);
            this.RegisterServer(providerInterfaceType, serverProvider);
            return serverProvider;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverProvider"></param>
        public void RegisterServer<TFrom>(IRpcServer serverProvider) where TFrom : class, IRpcServer
        {
            this.RegisterServer(typeof(TFrom), serverProvider);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="providerInterfaceType"></param>
        /// <param name="serverProvider"></param>
        public void RegisterServer(Type providerInterfaceType, IRpcServer serverProvider)
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            this.ServerProviders.Add(providerInterfaceType, serverProvider);
            MethodInstance[] methodInstances = CodeGenerator.GetMethodInstances(providerInterfaceType);

            foreach (var item in methodInstances)
            {
                item.Server = serverProvider;
                this.MethodMap.Add(item);
            }
            foreach (var parser in this)
            {
                parser.OnRegisterServer(serverProvider, methodInstances);
            }
        }

        #endregion 注册

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!this.m_disposedValue)
            {
                this.StopShareProxy();
                foreach (var item in this)
                {
                    item.SafeDispose();
                }
            }

            base.Dispose(disposing);
        }

        private static string Get(string url)
        {
            string result = null;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            //添加参数
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                resp.SafeDispose();
                stream.Close();
            }
            return result;
        }

        private static void SearchAttribute()
        {
            List<Type> types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    Type[] t1 = assembly.GetTypes().Where(p => typeof(RpcAttribute).IsAssignableFrom(p) && !p.IsAbstract).ToArray();
                    types.AddRange(t1);
                }
                catch
                {
                }
            }

            foreach (Type type in types)
            {
                ProxyAttributeMap.TryAdd(type.Name.Replace("Attribute", string.Empty).ToLower(), type);
            }
        }

        private IRpcServer GetServerProvider(MethodInstance methodInstance)
        {
            return methodInstance.Server;
        }
    }

    internal class InternalPlugin : HttpPluginBase
    {
        private readonly RpcStore m_rpcStore;

        public InternalPlugin(RpcStore rpcCerter)
        {
            this.m_rpcStore = rpcCerter;
        }

        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (e.Context.Request.UrlEquals(this.m_rpcStore.ProxyUrl))
            {
                bool? b = this.m_rpcStore.OnRequestProxy?.Invoke(e.Context.Request);
                if (b == false)
                {
                    using (ByteBlock byteBlock = new ByteBlock())
                    {
                        e.Context.Response
                            .FromText("拒绝响应内容")
                        .SetStatus("403", "Forbidden")
                        .Build(byteBlock);
                        client.DefaultSend(byteBlock);
                    }
                    return;
                }
                if (e.Context.Request.TryGetQuery(RpcStore.ProxyKey, out string value))
                {
                    List<Type> types = new List<Type>();

                    if (value.Equals("all", StringComparison.CurrentCultureIgnoreCase))
                    {
                        types = RpcStore.ProxyAttributeMap.Values.ToList();
                    }
                    else
                    {
                        string[] vs = value.Split(',');
                        foreach (var item in vs)
                        {
                            if (RpcStore.ProxyAttributeMap.TryGetValue(item, out Type type))
                            {
                                types.Add(type);
                            }
                        }
                    }

                    e.Context.Request.TryGetQuery(RpcStore.Namespace, out string names);

                    names = string.IsNullOrEmpty(names) ? "RRQMProxy" : names;

                    string code = CodeGenerator.ConvertToCode(names, this.m_rpcStore.GetProxyInfo(types.ToArray()));

                    using (ByteBlock byteBlock = new ByteBlock())
                    {
                        e.Context.Response
                        .SetStatus()
                        .SetContent(code)
                        .SetContentTypeFromFileName($"{names}.cs")
                        .Build(byteBlock);
                        client.DefaultSend(byteBlock);
                    }
                }
            }
            base.OnGet(client, e);
        }
    }
}