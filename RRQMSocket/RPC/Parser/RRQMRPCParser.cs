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
        /// 获取代理文件实例
        /// </summary>
        public RPCProxyInfo ProxyInfo { get; private set; }


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
                        MethodItem clientMethodItem = new MethodItem();
                        clientMethodItem.IsOutOrRef = methodInstance.IsByRef;
                        clientMethodItem.MethodToken = methodInstance.MethodToken;
                        clientMethodItem.ReturnTypeString = propertyCode.GetTypeFullName(methodInstance.ReturnType);
                        clientMethodItem.ParameterTypesString = new List<string>();
                        for (int i = 0; i < methodInstance.ParameterTypes.Length; i++)
                        {
                            clientMethodItem.ParameterTypesString.Add(propertyCode.GetTypeFullName(methodInstance.ParameterTypes[i]));
                        }
                        clientMethodStore.AddMethodItem(clientMethodItem);

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
        protected virtual void ExecuteContext(RPCContext context)
        {
            MethodInvoker methodInvoker = new MethodInvoker();
            if (this.MethodMap.TryGet(context.MethodToken, out MethodInstance methodInstance))
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
                methodInvoker.Status = InvokeStatus.Exception;
            }
            this.ExecuteMethod(methodInvoker);
        }
    }
}
