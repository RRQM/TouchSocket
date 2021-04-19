//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC服务器类
    /// </summary>
    public class RPCService : IDisposable
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RPCService()
        {
            this.ServerProviders = new ServerProviderCollection();
            this.RPCParsers = new RPCParserCollection();
        }

        /// <summary>
        /// 获取RPC解析器集合
        /// </summary>
        public RPCParserCollection RPCParsers { get; private set; }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken { get; set; }

        /// <summary>
        /// 添加RPC解析器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parser"></param>
        public void AddRPCParser(string key, IRPCParser parser)
        {
            this.RPCParsers.Add(key, parser);
            parser.InvokeMethod += InvokeMethod;
            parser.GetProxyInfo = this.GetProxyInfo;
            parser.InitMethodServer = this.InitMethodServer;
        }

        private void InvokeMethod(IRPCParser parser, RPCContext content)
        {
            InstanceMethod instanceMethod = this.serverMethodStore.GetInstanceMethod(content.Method);
            Task.Factory.StartNew(() =>
            {
                ExecuteMethod(parser, content, instanceMethod);
            });
        }

        private MethodStore serverMethodStore;
        private MethodStore clientMethodStore;

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public ServerProviderCollection ServerProviders { get; private set; }

        /// <summary>
        /// RPC代理文件版本
        /// </summary>
        public Version RPCVersion { get; private set; }

        /// <summary>
        /// 设置服务方法可用性
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="enable">可用性</param>
        /// <exception cref="RRQMRPCException"></exception>
        public void SetMethodEnable(string method, bool enable)
        {
            InstanceMethod instance = this.serverMethodStore.GetInstanceMethod(method);
            if (instance == null)
            {
                throw new RRQMRPCException("未找到该方法");
            }

            instance.isEnable = enable;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverProvider"></param>
        public void RegistService(ServerProvider serverProvider)
        {
            serverProvider.RPCService = this;
            this.ServerProviders.Add(serverProvider);
        }

        /// <summary>
        /// 注册所有服务
        /// </summary>
        /// <returns></returns>
        public int RegistAllService()
        {
            Type[] types = (AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes()).Where(p => typeof(ServerProvider).IsAssignableFrom(p) && p.IsAbstract == false)).ToArray();

            foreach (Type type in types)
            {
                ServerProvider serverProvider = Activator.CreateInstance(type) as ServerProvider;
                RegistService(serverProvider);
            }
            return types.Length;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回T实例</returns>
        public ServerProvider RegistService<T>() where T : ServerProvider
        {
            ServerProvider serverProvider = (ServerProvider)Activator.CreateInstance(typeof(T));
            this.RegistService(serverProvider);
            return serverProvider;
        }

        /// <summary>
        /// 开启RPC服务
        /// </summary>
        /// <param name="setting">设置</param>
        /// <exception cref="RRQMRPCKeyException">RPC方法注册异常</exception>
        /// <exception cref="RRQMRPCException">RPC异常</exception>
        /// <returns>返回源代码</returns>
        public CellCode[] OpenRPCServer(RPCServerSetting setting)
        {
            return OpenRPCServer(setting, null);
        }

        /// <summary>
        /// 开启RPC服务
        /// </summary>
        /// <param name="setting">设置</param>
        /// <param name="compiler">RPC编译器</param>
        /// <exception cref="RRQMRPCKeyException">RPC方法注册异常</exception>
        /// <exception cref="RRQMRPCException">RPC异常</exception>
        /// <returns>返回源代码</returns>
        public CellCode[] OpenRPCServer(RPCServerSetting setting, IRPCCompiler compiler)
        {
            if (this.ServerProviders.Count == 0)
            {
                throw new RRQMRPCException("已注册服务数量为0");
            }

            if (this.RPCParsers.Count == 0)
            {
                throw new RRQMRPCException("请至少添加一种RPC解析器");
            }
           
            this.serverMethodStore = new MethodStore();
            this.clientMethodStore = new MethodStore();
            string nameSpace = setting.NameSpace == null ? "RRQMRPC" : $"RRQMRPC.{setting.NameSpace}";
            List<string> refs = new List<string>();

            PropertyCodeMap propertyCode = new PropertyCodeMap(this.ServerProviders.SingleAssembly, nameSpace);
            string assemblyName;
            if (setting.NameSpace != null && setting.NameSpace.Trim().Length > 0)
            {
                assemblyName = string.Format("RRQMRPC.{0}.dll", setting.NameSpace);
            }
            else
            {
                assemblyName = "RRQMRPC.dll";
            }
            Dictionary<string, List<MethodInfo>> classAndMethods = new Dictionary<string, List<MethodInfo>>();

            foreach (ServerProvider instance in this.ServerProviders)
            {
                if (!classAndMethods.Keys.Contains(instance.GetType().Name))
                {
                    classAndMethods.Add(instance.GetType().Name, new List<MethodInfo>());
                }
                MethodInfo[] methodInfos = instance.GetType().GetMethods();
                foreach (MethodInfo method in methodInfos)
                {
                    if (method.IsGenericMethod)
                    {
                        throw new RRQMRPCException("RPC方法中不支持泛型参数");
                    }
                    RRQMRPCMethodAttribute attribute = method.GetCustomAttribute<RRQMRPCMethodAttribute>();
                    if (attribute != null)
                    {
                        classAndMethods[instance.GetType().Name].Add(method);

                        string methodName = attribute.MethodKey == null || attribute.MethodKey.Trim().Length == 0 ? method.Name : attribute.MethodKey;

                        MethodItem methodItem = new MethodItem();
                        methodItem.Method = methodName;
                        methodItem.SP = attribute.SP;
                        ParameterInfo[] parameters = method.GetParameters();
                        methodItem.ParameterTypes = new List<Type>();
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].ParameterType.IsByRef)
                            {
                                methodItem.IsOutOrRef = true;
                            }

                            if (parameters[i].ParameterType.FullName != null)
                            {
                                refs.Add(parameters[i].ParameterType.Assembly.Location);
                            }
                            propertyCode.AddTypeString(parameters[i].ParameterType);

                            if (parameters[i].ParameterType.FullName.Contains("&"))
                            {
                                methodItem.ParameterTypes.Add(propertyCode.GetRefOutType(parameters[i].ParameterType));
                            }
                            else
                            {
                                methodItem.ParameterTypes.Add(parameters[i].ParameterType);
                            }
                        }

                        refs.Add(method.ReturnType.Assembly.Location);
                        propertyCode.AddTypeString(method.ReturnType);
                        methodItem.ReturnType = method.ReturnType;
                        try
                        {
                            serverMethodStore.AddMethodItem(methodItem);
                        }
                        catch (Exception)
                        {
                            throw new RRQMRPCKeyException($"方法名为{methodName}的方法已经注册");
                        }

                        InstanceMethod instanceOfMethod = new InstanceMethod();
                        instanceOfMethod.instance = instance;
                        instanceOfMethod.method = method;
                        instanceOfMethod.methodItem = methodItem;
                        instanceOfMethod.isEnable = true;
                        serverMethodStore.AddInstanceMethod(instanceOfMethod);
                    }
                }
            }

            InstanceMethod[] instances = this.serverMethodStore.GetAllInstanceMethod();
            foreach (InstanceMethod item in instances)
            {
                MethodItem clientMethodItem = new MethodItem();
                clientMethodItem.IsOutOrRef = item.methodItem.IsOutOrRef;
                clientMethodItem.Method = item.methodItem.Method;
                clientMethodItem.ReturnTypeString = propertyCode.GetTypeFullName(item.methodItem.ReturnType);
                clientMethodItem.ParameterTypesString = new List<string>();
                for (int i = 0; i < item.methodItem.ParameterTypes.Count; i++)
                {
                    clientMethodItem.ParameterTypesString.Add(propertyCode.GetTypeFullName(item.methodItem.ParameterTypes[i]));
                }
                clientMethodStore.AddMethodItem(clientMethodItem);
            }

            CodeMap.Namespace = nameSpace;
            CodeMap.PropertyCode = propertyCode;
            List<CellCode> codes = new List<CellCode>();
            //codes.Add(CodeMap.GetAssemblyInfo(nameSpace, setting.Version));

            foreach (string className in classAndMethods.Keys)
            {
                CodeMap codeMap = new CodeMap();
                codeMap.ClassName = className;
                codeMap.Methods = classAndMethods[className].ToArray();

                CellCode cellCode = new CellCode();
                cellCode.Name = className;
                cellCode.CodeType = CodeType.Service;
                cellCode.Code = codeMap.GetCode();
                codes.Add(cellCode);
            }
            CellCode propertyCellCode = new CellCode();
            propertyCellCode.Name = "ClassArgs";
            propertyCellCode.CodeType = CodeType.ClassArgs;
            propertyCellCode.Code = propertyCode.GetPropertyCode();
            codes.Add(propertyCellCode);
            string assemblyInfo = CodeMap.GetAssemblyInfo(nameSpace, setting.Version);
            this.RPCVersion = CodeMap.Version;

            RPCProxyInfo proxyInfo = new RPCProxyInfo();
            proxyInfo.AssemblyName = assemblyName;
            proxyInfo.Version = this.RPCVersion.ToString();
            if (compiler != null)
            {
                List<string> codesString = new List<string>();
                foreach (var item in codes)
                {
                    codesString.Add(item.Code);
                }
                codesString.Add(assemblyInfo);
                proxyInfo.AssemblyData = compiler.CompileCode(assemblyName, codesString.ToArray(), refs);
            }
            proxyInfo.Codes = codes;
            this.serverMethodStore.SetProxyInfo(proxyInfo);

            return codes.ToArray();
        }

        private void ExecuteMethod(IRPCParser parser, RPCContext content, InstanceMethod instanceMethod)
        {
            if (instanceMethod != null)
            {
                if (instanceMethod.isEnable)
                {
                    ServerProvider instance = instanceMethod.instance;
                    try
                    {
                        MethodItem methodItem = instanceMethod.methodItem;
                        object[] parameters = null;
                        if (content.ParametersBytes != null)
                        {
                            parameters = new object[content.ParametersBytes.Count];
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                parameters[i] = parser.SerializeConverter.DeserializeParameter(content.ParametersBytes[i], methodItem.ParameterTypes[i]);
                            }
                        }

                        instance.RPCEnter(parser, methodItem);
                        MethodInfo method = instanceMethod.method;
                        content.ReturnParameterBytes = parser.SerializeConverter.SerializeParameter(method.Invoke(instance, parameters));
                        content.Status = 1;
                        content.Message = null;
                        if (!instanceMethod.methodItem.IsOutOrRef)
                        {
                            content.ParametersBytes = null;
                        }
                        else
                        {
                            List<byte[]> datas = new List<byte[]>();
                            foreach (object parameter in parameters)
                            {
                                datas.Add(parser.SerializeConverter.SerializeParameter(parameter));
                            }
                            content.ParametersBytes = datas;
                        }
                        instance.RPCLeave(parser, instanceMethod.methodItem);
                    }
                    catch (RRQMAbandonRPCException e)
                    {
                        if (!e.Feedback)
                        {
                            return;
                        }
                        content.Status = 4;
                        content.Message = "函数被阻止执行，信息：" + e.Message;
                    }
                    catch (TargetInvocationException e)
                    {
                        content.Status = 2;
                        if (e.InnerException != null)
                        {
                            content.Message = "函数内部发生异常，信息：" + e.InnerException.Message;
                        }
                        else
                        {
                            content.Message = "函数内部发生异常，信息：未知";
                        }
                        instance.RPCError(parser, instanceMethod.methodItem);
                    }
                    catch (Exception e)
                    {
                        content.Status = 2;
                        content.Message = e.Message;
                        instance.RPCError(parser, instanceMethod.methodItem);
                    }
                }
                else
                {
                    content.Status = 3;
                }
            }
            else
            {
                content.Status = 2;
            }

            parser.EndInvokeMethod(content);
        }

        /// <summary>
        /// 获取代理文件
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <returns></returns>
        protected virtual RPCProxyInfo GetProxyInfo(string proxyToken)
        {
            RPCProxyInfo proxyInfo = new RPCProxyInfo();
            if (this.ProxyToken == proxyToken)
            {
                proxyInfo.AssemblyData = this.serverMethodStore.ProxyInfo.AssemblyData;
                proxyInfo.AssemblyName = this.serverMethodStore.ProxyInfo.AssemblyName;
                proxyInfo.Codes = this.serverMethodStore.ProxyInfo.Codes;
                proxyInfo.Version = this.serverMethodStore.ProxyInfo.Version;
                proxyInfo.Status = 1;
            }
            else
            {
                proxyInfo.Status = 2;
                proxyInfo.Message = "令箭不正确";
            }

            return proxyInfo;
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <returns></returns>
        protected virtual List<MethodItem> InitMethodServer()
        {
           return this.clientMethodStore.GetAllMethodItem();
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
        }
    }
}