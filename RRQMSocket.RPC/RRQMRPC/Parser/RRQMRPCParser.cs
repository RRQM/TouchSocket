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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RRQM内置解析器
    /// </summary>
    public abstract class RRQMRPCParser : RPCParser, IService
    {
        private MethodStore clientMethodStore;

        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 获取或设置代理源文件命名空间
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// RPC代理版本
        /// </summary>
        public Version RPCVersion { get; set; }

        /// <summary>
        /// RPC编译器
        /// </summary>
        public IRPCCompiler RPCCompiler { get; set; }

        /// <summary>
        /// 获取生成的代理代码
        /// </summary>
        public CellCode[] Codes { get; private set; }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken { get; set; }

        /// <summary>
        /// 获取代理文件实例
        /// </summary>
        public RPCProxyInfo ProxyInfo { get; private set; }

        /// <summary>
        /// 获取绑定状态
        /// </summary>
        public abstract bool IsBind { get; }

        /// <summary>
        /// 内存池实例
        /// </summary>
        public abstract BytePool BytePool { get; }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="methodInstances"></param>
        protected sealed override void InitializeServers(MethodInstance[] methodInstances)
        {
            this.clientMethodStore = new MethodStore();
            string nameSpace = string.IsNullOrEmpty(this.NameSpace) ? "RRQMRPC" : $"RRQMRPC.{this.NameSpace}";
            List<string> refs = new List<string>();

            PropertyCodeMap propertyCode = new PropertyCodeMap(this.RPCService.ServerProviders.SingleAssembly, nameSpace);
            string assemblyName = $"{nameSpace}.dll";

            foreach (MethodInstance methodInstance in methodInstances)
            {
                foreach (RPCMethodAttribute att in methodInstance.RPCAttributes)
                {
                    if (att is RRQMRPCMethodAttribute attribute)
                    {
                        if (methodInstance.ReturnType != null)
                        {
                            refs.Add(methodInstance.ReturnType.Assembly.Location);
                            propertyCode.AddTypeString(methodInstance.ReturnType);
                        }
                        foreach (var type in methodInstance.ParameterTypes)
                        {
                            refs.Add(type.Assembly.Location);
                            propertyCode.AddTypeString(type);
                        }

                        break;
                    }
                }
            }

            Dictionary<string, List<MethodInfo>> classAndMethods = new Dictionary<string, List<MethodInfo>>();

            foreach (MethodInstance methodInstance in methodInstances)
            {
                foreach (RPCMethodAttribute att in methodInstance.RPCAttributes)
                {
                    if (att is RRQMRPCMethodAttribute attribute)
                    {
                        MethodItem methodItem = new MethodItem();
                        methodItem.IsOutOrRef = methodInstance.IsByRef;
                        methodItem.MethodToken = methodInstance.MethodToken;
                        if (methodInstance.ReturnType != null)
                        {
                            methodItem.ReturnTypeString = propertyCode.GetTypeFullName(methodInstance.ReturnType);
                        }

                        methodItem.ParameterTypesString = new List<string>();
                        methodItem.Method = attribute.MethodKey == null || attribute.MethodKey.Trim().Length == 0 ? methodInstance.Method.Name : attribute.MethodKey;

                        for (int i = 0; i < methodInstance.ParameterTypes.Length; i++)
                        {
                            methodItem.ParameterTypesString.Add(propertyCode.GetTypeFullName(methodInstance.ParameterTypes[i]));
                        }
                        try
                        {
                            clientMethodStore.AddMethodItem(methodItem);
                        }
                        catch
                        {
                            throw new RRQMRPCKeyException($"方法键为{methodItem.Method}的服务已注册");
                        }

                        string className = methodInstance.Provider.GetType().Name;
                        if (!classAndMethods.ContainsKey(className))
                        {
                            classAndMethods.Add(className, new List<MethodInfo>());
                        }
                        classAndMethods[className].Add(methodInstance.Method);
                        break;
                    }
                }
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
            string assemblyInfo = CodeMap.GetAssemblyInfo(nameSpace, this.RPCVersion);
            this.RPCVersion = CodeMap.Version;

            RPCProxyInfo proxyInfo = new RPCProxyInfo();
            proxyInfo.AssemblyName = assemblyName;
            proxyInfo.Version = this.RPCVersion.ToString();
            if (this.RPCCompiler != null)
            {
                List<string> codesString = new List<string>();
                foreach (var item in codes)
                {
                    codesString.Add(item.Code);
                }
                codesString.Add(assemblyInfo);
                proxyInfo.AssemblyData = this.RPCCompiler.CompileCode(assemblyName, codesString.ToArray(), refs);
            }
            proxyInfo.Codes = codes;
            this.ProxyInfo = proxyInfo;

            this.Codes = codes.ToArray();
        }

        /// <summary>
        /// 获取代理文件
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected virtual RPCProxyInfo GetProxyInfo(string proxyToken, RPCParser parser)
        {
            RPCProxyInfo proxyInfo = new RPCProxyInfo();
            if (this.ProxyToken == proxyToken)
            {
                proxyInfo.AssemblyData = this.ProxyInfo.AssemblyData;
                proxyInfo.AssemblyName = this.ProxyInfo.AssemblyName;
                proxyInfo.Codes = this.ProxyInfo.Codes;
                proxyInfo.Version = this.ProxyInfo.Version;
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
        /// 执行内容
        /// </summary>
        /// <param name="context"></param>
        protected virtual void ExecuteContext(RpcContext context)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            methodInvoker.Flag = context;
            if (this.MethodMap.TryGet(context.MethodToken, out MethodInstance methodInstance))
            {
                if (methodInstance.IsEnable)
                {
                    object[] ps = new object[methodInstance.ParameterTypes.Length];
                    for (int i = 0; i < context.ParametersBytes.Count; i++)
                    {
                        ps[i] = this.SerializeConverter.DeserializeParameter(context.ParametersBytes[i], methodInstance.ParameterTypes[i]);
                    }
                    methodInvoker.Parameters = ps;
                }
                else
                {
                    methodInvoker.Status = InvokeStatus.UnEnable;
                }

                this.ExecuteMethod(methodInvoker, methodInstance);
            }
            else
            {
                methodInvoker.Status = InvokeStatus.UnFound;
                this.ExecuteMethod(methodInvoker, null);
            }
        }

        /// <summary>
        /// 获取已注册服务条目
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected virtual List<MethodItem> GetRegisteredMethodItems(RPCParser parser)
        {
            return this.clientMethodStore.GetAllMethodItem();
        }

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public abstract void Bind(int port, int threadCount = 1);

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="iPHost">ip和端口号，格式如“127.0.0.1:7789”。IP可输入Ipv6</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public abstract void Bind(IPHost iPHost, int threadCount);

        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="addressFamily">寻址方案</param>
        /// <param name="endPoint">绑定节点</param>
        /// <param name="threadCount">多线程数量</param>
        /// <exception cref="RRQMException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public abstract void Bind(AddressFamily addressFamily, EndPoint endPoint, int threadCount);
    }
}