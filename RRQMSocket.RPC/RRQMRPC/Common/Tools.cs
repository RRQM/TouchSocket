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
using RRQMCore;
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    internal static class Tools
    {
        internal static void GetRPCMethod(
            MethodInstance[] methodInstances,
            string nameSpaceOld,
            ref MethodStore methodStore,
             Version version,
             ref RPCProxyInfo proxyInfo)
        {
            string nameSpace = string.IsNullOrEmpty(nameSpaceOld) ? "RRQMRPC" : $"RRQMRPC.{nameSpaceOld}";
            List<string> refs = new List<string>();

            PropertyCodeMap propertyCode = new PropertyCodeMap(nameSpace, methodStore);

            foreach (MethodInstance methodInstance in methodInstances)
            {
                foreach (RPCAttribute att in methodInstance.RPCAttributes)
                {
                    if (att is RRQMRPCAttribute attribute)
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

#if NET45_OR_GREATER
            foreach (var item in refs)
            {
                RPCCompiler.AddRef(item);
            }
#endif
            List<MethodInfo> methods = new List<MethodInfo>();
            string className = null;

            foreach (MethodInstance methodInstance in methodInstances)
            {
                foreach (RPCAttribute att in methodInstance.RPCAttributes)
                {
                    if (att is RRQMRPCAttribute attribute)
                    {
                        MethodItem methodItem = new MethodItem();
                        methodItem.IsOutOrRef = methodInstance.IsByRef;
                        methodItem.MethodToken = methodInstance.MethodToken;
                        methodItem.ServerName = methodInstance.Provider.GetType().Name;
                        methodItem.Method = string.IsNullOrEmpty(attribute.MemberKey) ? methodInstance.Method.Name : attribute.MemberKey;
                        try
                        {
                            methodStore.AddMethodItem(methodItem);
                        }
                        catch
                        {
                            throw new RRQMRPCKeyException($"方法键为{methodItem.Method}的服务已注册");
                        }

                        if (className == null)
                        {
                            className = methodInstance.Provider.GetType().Name;
                        }
                        else if (className != methodInstance.Provider.GetType().Name)
                        {
                            throw new RRQMRPCException("方法来源于不同服务");
                        }
                        methods.Add(methodInstance.Method);
                        break;
                    }
                }
            }

            CodeMap.Namespace = nameSpace;
            CodeMap.PropertyCode = propertyCode;


            CodeMap codeMap = new CodeMap();
            codeMap.ClassName = className;
            codeMap.Methods = methods.ToArray();

            CellCode cellCode = new CellCode();
            cellCode.Name = className;
            cellCode.CodeType = CodeType.Service;
            cellCode.Code = codeMap.GetCode();

            if (proxyInfo.Codes == null)
            {
                proxyInfo.Codes = new List<CellCode>();
            }
            proxyInfo.Codes.Add(cellCode);

            CellCode argesCode = proxyInfo.Codes.FirstOrDefault(a => a.Name == "ClassArgs");
            if (argesCode == null)
            {
                argesCode = new CellCode();
                argesCode.Name = "ClassArgs";
                argesCode.CodeType = CodeType.ClassArgs;
                argesCode.Code = propertyCode.GetPropertyCode();
                proxyInfo.Codes.Add(argesCode);
            }
            else
            {
                argesCode.Code = propertyCode.GetPropertyCode();
            }

            proxyInfo.AssemblyName = $"{nameSpace}.dll";
            proxyInfo.Version = version == null ? "1.0.0.0" : version.ToString();
        }

        private static int nullReturnNullParameters = 1000000000;
        private static int nullReturnExistParameters = 300000000;
        private static int ExistReturnNullParameters = 500000000;
        private static int ExistReturnExistParameters = 700000000;

        internal static MethodInstance[] GetMethodInstances(ServerProvider serverProvider, bool isSetToken)
        {
            List<MethodInstance> instances = new List<MethodInstance>();

            MethodInfo[] methodInfos = serverProvider.GetType().GetMethods();

            foreach (MethodInfo method in methodInfos)
            {
                if (method.IsGenericMethod)
                {
                    throw new RRQMRPCException("RPC方法中不支持泛型参数");
                }
                IEnumerable<RPCAttribute> attributes = method.GetCustomAttributes<RPCAttribute>(true);
                if (attributes.Count() > 0)
                {
                    MethodInstance methodInstance = new MethodInstance();
                    methodInstance.Provider = serverProvider;
                    methodInstance.Method = method;
                    methodInstance.RPCAttributes = attributes.ToArray();
                    methodInstance.IsEnable = true;
                    methodInstance.Parameters = method.GetParameters();
                    List<string> names = new List<string>();
                    foreach (var parameterInfo in methodInstance.Parameters)
                    {
                        names.Add(parameterInfo.Name);
                    }
                    methodInstance.ParameterNames = names.ToArray();
                    if (typeof(Task).IsAssignableFrom(method.ReturnType))
                    {
                        methodInstance.Async = true;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    List<Type> types = new List<Type>();
                    foreach (var parameter in parameters)
                    {
                        types.Add(parameter.ParameterType.GetRefOutType());
                        if (parameter.ParameterType.IsByRef)
                        {
                            methodInstance.IsByRef = true;
                        }
                    }
                    methodInstance.ParameterTypes = types.ToArray();

                    if (method.ReturnType == typeof(void)|| method.ReturnType==typeof(Task))
                    {
                        methodInstance.ReturnType = null;

                        if (isSetToken)
                        {
                            if (parameters.Length == 0)
                            {
                                methodInstance.MethodToken = ++nullReturnNullParameters;
                            }
                            else
                            {
                                methodInstance.MethodToken = ++nullReturnExistParameters;
                            }
                        }

                    }
                    else
                    {
                        if (methodInstance.Async)
                        {
                            Type[] ts = method.ReturnType.GetGenericArguments();
                            if (ts.Length == 1)
                            {
                                methodInstance.ReturnType = ts[0];
                            }
                            else
                            {
                                methodInstance.ReturnType = null;
                            }
                        }
                        else
                        {
                            methodInstance.ReturnType = method.ReturnType;
                        }

                        if (isSetToken)
                        {
                            if (parameters.Length == 0)
                            {
                                methodInstance.MethodToken = ++ExistReturnNullParameters;
                            }
                            else
                            {
                                methodInstance.MethodToken = ++ExistReturnExistParameters;
                            }
                        }
                    }
                    instances.Add(methodInstance);

                }
            }

            return instances.ToArray();
        }
    }
}