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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 通讯服务端主类
    /// </summary>
    public sealed class TcpRPCService : RPCService<TcpRPCSocketClient>, ISerialize
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpRPCService()
        {
            this.SerializeConverter = new BinarySerializeConverter();
        }

        private MethodStore serverMethodStore;
        private MethodStore clientMethodStore;

        /// <summary>
        /// RPC代理文件版本
        /// </summary>
        public Version RPCVersion { get; private set; }
        /// <summary>
        /// 序列化生成器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }
        /// <summary>
        /// 开启RPC服务
        /// </summary>
        /// <param name="setting">设置</param>
        /// <exception cref="RRQMRPCKeyException">RPC方法注册异常</exception>
        /// <exception cref="RRQMRPCException">RPC异常</exception>
        public override void OpenRPCServer(RPCServerSetting setting)
        {
            this.serverMethodStore = new MethodStore();
            this.clientMethodStore = new MethodStore();
            string nameSpace = setting.NameSpace == null ? "RRQMRPC" : $"RRQMRPC.{setting.NameSpace}";

            List<ServerProvider> serverProviders = new List<ServerProvider>();
            Type[] types = (AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes()).Where(p => typeof(ServerProvider).IsAssignableFrom(p) && p.IsAbstract == false)).ToArray();

            Assembly singleAssembly = null;

            foreach (Type type in types)
            {
                serverProviders.Add(Activator.CreateInstance(type) as ServerProvider);
                if (singleAssembly == null)
                {
                    singleAssembly = type.Assembly;
                }
                else if (singleAssembly != type.Assembly)
                {
                    throw new RRQMRPCException("所有的服务类必须声明在同一程序集内");
                }
            }

            PropertyCodeMap propertyCode = new PropertyCodeMap(singleAssembly, nameSpace);

            Microsoft.CSharp.CSharpCodeProvider codeProvider = new Microsoft.CSharp.CSharpCodeProvider();

            System.CodeDom.Compiler.CompilerParameters objCompilerParameters = new System.CodeDom.Compiler.CompilerParameters();

            string assemblyName;
            if (setting.NameSpace != null && setting.NameSpace.Trim().Length > 0)
            {
                assemblyName = string.Format("RRQMRPC.{0}.dll", setting.NameSpace);
            }
            else
            {
                assemblyName = "RRQMRPC.dll";
            }
            objCompilerParameters.OutputAssembly = assemblyName;

            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = false;

            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
            objCompilerParameters.ReferencedAssemblies.Add("mscorlib.dll");
            objCompilerParameters.ReferencedAssemblies.Add("RRQMCore.dll");

            AddReferencedAssemblie(objCompilerParameters, this.GetType().Assembly.Location);

            Dictionary<string, List<MethodInfo>> classAndMethods = new Dictionary<string, List<MethodInfo>>();

            foreach (ServerProvider instance in serverProviders)
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
                                AddReferencedAssemblie(objCompilerParameters, parameters[i].ParameterType.Assembly.Location);
                            }
                            propertyCode.AddTypeString(parameters[i].ParameterType);

                            if (parameters[i].ParameterType.FullName.Contains("&"))
                            {
                                methodItem.ParameterTypes[i] = Type.GetType(parameters[i].ParameterType.FullName.Replace("&", string.Empty));
                            }
                            else
                            {
                                methodItem.ParameterTypes[i] = parameters[i].ParameterType;
                            }

                        }

                        AddReferencedAssemblie(objCompilerParameters, method.ReturnType.Assembly.Location);
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
                        instanceOfMethod.Instance = instance;
                        instanceOfMethod.Method = method;
                        instanceOfMethod.MethodItem = methodItem;
                        serverMethodStore.AddInstanceMethod(instanceOfMethod);
                    }
                }

                EventInfo[] eventInfos = instance.GetType().GetEvents();
                foreach (EventInfo eventInfo in eventInfos)
                {
                    propertyCode.AddTypeString(eventInfo.EventHandlerType);
                    string s = propertyCode.GetTypeFullName(eventInfo.EventHandlerType);
                }
            }

            InstanceMethod[] instances = this.serverMethodStore.GetAllInstanceMethod();
            foreach (InstanceMethod item in instances)
            {
                MethodItem clientMethodItem = new MethodItem();
                clientMethodItem.IsOutOrRef = item.MethodItem.IsOutOrRef;
                clientMethodItem.Method = item.MethodItem.Method;
                clientMethodItem.ReturnTypeString = propertyCode.GetTypeFullName(item.MethodItem.ReturnType);
                clientMethodItem.ParameterTypesString = new string[item.MethodItem.ParameterTypes.Length];
                for (int i = 0; i < item.MethodItem.ParameterTypes.Length; i++)
                {
                    clientMethodItem.ParameterTypesString[i] = propertyCode.GetTypeFullName(item.MethodItem.ParameterTypes[i]);
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
            //foreach (var item in codes)
            //{
            //    Console.WriteLine(item);
            //}
            this.RPCVersion = CodeMap.Version;

            System.CodeDom.Compiler.CompilerResults cr = codeProvider.CompileAssemblyFromSource(objCompilerParameters, codes.ToArray());

            if (cr.Errors.HasErrors)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (System.CodeDom.Compiler.CompilerError err in cr.Errors)
                {
                    stringBuilder.AppendLine(err.ErrorText);
                }

                throw new RRQMRPCException(stringBuilder.ToString());
            }

            RPCProxyInfo proxyInfo = new RPCProxyInfo();
            proxyInfo = new RPCProxyInfo();
            proxyInfo.AssemblyName = assemblyName;
            proxyInfo.Version = this.RPCVersion;
            proxyInfo.AssemblyData = File.ReadAllBytes(assemblyName);
            if (setting.ProxySourceCodeVisible)
            {
                proxyInfo.Codes = codes.ToArray();
            }
            this.serverMethodStore.SetProxyInfo(proxyInfo, setting.ProxyToken);
        }
        
        /// <summary>
        /// 将连接进来的用户进行储存
        /// </summary>
        protected override TcpRPCSocketClient CreatSocketCliect()
        {
            TcpRPCSocketClient socketCliect = this.ObjectPool.GetObject();
            if (socketCliect.NewCreat)
            {
                socketCliect.serverMethodStore = this.serverMethodStore;
                socketCliect.clientMethodStore = this.clientMethodStore;
                socketCliect.SerializeConverter = this.SerializeConverter;
            }
            return socketCliect;
        }

        private void AddReferencedAssemblie(System.CodeDom.Compiler.CompilerParameters objCompilerParameters, string name)
        {
            if (name.Contains("System.dll") || name.Contains("mscorlib.dll"))
            {
                return;
            }
            else if (!objCompilerParameters.ReferencedAssemblies.Contains(name))
            {
                objCompilerParameters.ReferencedAssemblies.Add(name);
            }
        }

    }
}