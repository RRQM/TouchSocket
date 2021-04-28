using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RRQM内置解析器
    /// </summary>
    public abstract class RRQMRPCParser : RPCParser
    {

        private MethodStore serverMethodStore;
        private MethodStore clientMethodStore;

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
        /// 初始化服务
        /// </summary>
        /// <param name="serverProviders"></param>
        protected override void InitializeServers(ServerProviderCollection serverProviders)
        {
            this.serverMethodStore = new MethodStore();
            this.clientMethodStore = new MethodStore();
            string nameSpace = string.IsNullOrEmpty(this.NameSpace) ? "RRQMRPC" : $"RRQMRPC.{this.NameSpace}";
            List<string> refs = new List<string>();

            PropertyCodeMap propertyCode = new PropertyCodeMap(serverProviders.SingleAssembly, nameSpace);
            string assemblyName = $"{nameSpace}.dll";

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
                            throw new RRQMRPCKeyException($"方法键为{methodName}的方法已经注册");
                        }

                        MethodInstance instanceOfMethod = new MethodInstance();
                        instanceOfMethod.instance = instance;
                        instanceOfMethod.method = method;
                        instanceOfMethod.methodItem = methodItem;
                        instanceOfMethod.isEnable = true;
                        serverMethodStore.AddInstanceMethod(instanceOfMethod);
                    }
                }
            }

            MethodInstance[] instances = this.serverMethodStore.GetAllInstanceMethod();
            foreach (MethodInstance item in instances)
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
            this.serverMethodStore.SetProxyInfo(proxyInfo);

            this.Codes = codes.ToArray();
        }


    }
}
