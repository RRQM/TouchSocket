//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RRQMCore.ByteManager;
using System.Net;
using RRQMCore.Log;
using RRQMCore.Serialization;
using System.Threading.Tasks;

#if !NET45_OR_GREATER
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Emit;
#endif
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
        /// 添加RPC解析器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parser"></param>
        public void AddRPCParser(string key, IRPCParser parser)
        {
            this.RPCParsers.Add(key, parser);
            parser.InvokeMethod += InvokeMethod;
        }

        private void InvokeMethod(IRPCParser parser, RPCContext content)
        {
            InstanceMethod instanceMethod = this.serverMethodStore.GetInstanceMethod(content.Method);
            if (instanceMethod.async)
            {
                Task.Factory.StartNew(() =>
                {
                    ExecuteMethod(parser, content, instanceMethod);
                });
            }
            else
            {
                ExecuteMethod(parser, content, instanceMethod);
            }
        }

        /// <summary>
        /// 服务器函数映射
        /// </summary>
        private MethodStore serverMethodStore;

        /// <summary>
        /// 客户端函数映射
        /// </summary>
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
        public void SetMethodEnable(string method,bool enable)
        {
            InstanceMethod instance= this.serverMethodStore.GetInstanceMethod(method);
            if (instance==null)
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
        public string[] OpenRPCServer(RPCServerSetting setting)
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
                        methodItem.ParameterTypes = new Type[parameters.Length];
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
                                
                                methodItem.ParameterTypes[i] = propertyCode.GetRefOutType(parameters[i].ParameterType);
                            }
                            else
                            {
                                methodItem.ParameterTypes[i] = parameters[i].ParameterType;
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
                        instanceOfMethod.async = attribute.Async;
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
                clientMethodItem.ParameterTypesString = new string[item.methodItem.ParameterTypes.Length];
                for (int i = 0; i < item.methodItem.ParameterTypes.Length; i++)
                {
                    clientMethodItem.ParameterTypesString[i] = propertyCode.GetTypeFullName(item.methodItem.ParameterTypes[i]);
                }
                clientMethodStore.AddMethodItem(clientMethodItem);
            }

            CodeMap.Namespace = nameSpace;
            CodeMap.PropertyCode = propertyCode;
            List<string> codes = new List<string>();
            codes.Add(CodeMap.GetAssemblyInfo(nameSpace, setting.Version));

            foreach (string className in classAndMethods.Keys)
            {
                CodeMap codeMap = new CodeMap();
                codeMap.ClassName = className;
                codeMap.Methods = classAndMethods[className].ToArray();
                codes.Add(codeMap.GetCode());
            }

            codes.Add(propertyCode.GetPropertyCode());

            this.RPCVersion = CodeMap.Version;

            RPCProxyInfo proxyInfo = new RPCProxyInfo();
            proxyInfo = new RPCProxyInfo();
            proxyInfo.AssemblyName = assemblyName;
            proxyInfo.Version = this.RPCVersion;
            proxyInfo.AssemblyData = CompileCode(assemblyName, codes.ToArray(), refs);
            if (setting.ProxySourceCodeVisible)
            {
                proxyInfo.Codes = codes.ToArray();
            }
            this.serverMethodStore.SetProxyInfo(proxyInfo, setting.ProxyToken);

            foreach (var item in this.RPCParsers)
            {
                item.InitMethodStore(this.serverMethodStore, this.clientMethodStore);
            }
            return codes.ToArray();
        }

#if NET45_OR_GREATER
        private byte[] CompileCode(string assemblyName, string[] codes, List<string> refStrings)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compilerParameters = new CompilerParameters();

            compilerParameters.OutputAssembly = assemblyName;

            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = false;
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParameters.ReferencedAssemblies.Add("RRQMCore.dll");
            compilerParameters.ReferencedAssemblies.Add(this.GetType().Assembly.Location);

            foreach (var item in refStrings)
            {
                compilerParameters.ReferencedAssemblies.Add(item);
            }

            CompilerResults cr = codeProvider.CompileAssemblyFromSource(compilerParameters, codes);
            if (cr.Errors.HasErrors)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (CompilerError err in cr.Errors)
                {
                    stringBuilder.AppendLine(err.ErrorText);
                }

                throw new RRQMRPCException(stringBuilder.ToString());
            }

            return File.ReadAllBytes(assemblyName);
        }
#elif NETCOREAPP || NETSTANDARD
        private byte[] CompileCode(string assemblyName, string[] codes, List<string> refStrings)
        {
            List<MetadataReference> refs = new List<MetadataReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in assemblies)
            {
                if (item.Location != null && item.Location != string.Empty)
                {
                    refs.Add(MetadataReference.CreateFromFile(item.Location));
                }
            }

            foreach (var item in refStrings)
            {
                refs.Add(MetadataReference.CreateFromFile(item));
            }
            CSharpCompilation cSharpCompilation = CSharpCompilation.Create(assemblyName);

            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                usings: null,
                                optimizationLevel: OptimizationLevel.Release,
                                checkOverflow: false,
                                allowUnsafe: false,
                                platform: Platform.AnyCpu,
                                warningLevel: 4,
                                xmlReferenceResolver: null);

            cSharpCompilation = cSharpCompilation.WithOptions(compilationOptions);

            cSharpCompilation = cSharpCompilation.AddReferences(refs);

            using (MemoryStream ms = new MemoryStream())
            {
                List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
                foreach (var item in codes)
                {
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(item));
                }
                using (Stream win32resStream = cSharpCompilation.CreateDefaultWin32Resources(true, true, null, null))
                {
                    EmitResult emitResult = cSharpCompilation.AddSyntaxTrees(syntaxTrees).Emit(ms, win32Resources: win32resStream);
                    if (!emitResult.Success)
                    {
                        IEnumerable<Diagnostic> failures = emitResult.Diagnostics.Where(diagnostic =>
                 diagnostic.IsWarningAsError ||
                 diagnostic.Severity == DiagnosticSeverity.Error);
                        StringBuilder sboutputMessage = new StringBuilder();
                        foreach (Diagnostic diagnostic in failures)
                        {
                            sboutputMessage.AppendFormat(diagnostic.GetMessage() + Environment.NewLine);
                        }
                        throw new RRQMCore.Exceptions.RRQMException(sboutputMessage.ToString());
                    }
                    byte[] datas = ms.ToArray();
                    File.WriteAllBytes(assemblyName, datas);
                    return datas;
                }

            }

        }

#endif

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
        /// 释放资源
        /// </summary>
        public void Dispose()
        {

        }
    }
}
