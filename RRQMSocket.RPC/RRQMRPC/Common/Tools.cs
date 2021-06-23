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
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RRQMSocket.RPC.RRQMRPC
{
    internal static class Tools
    {
        internal static void GetRPCMethod(
            ServerProviderCollection providers,
            MethodInstance[] methodInstances,
            string nameSpaceOld, Assembly assembly,
            out MethodStore methodStore,
             Version version,
             IRPCCompiler compiler,
             out RPCProxyInfo proxyInfo,
             out CellCode[] cellCodes)
        {
            methodStore = new MethodStore();
            string nameSpace = string.IsNullOrEmpty(nameSpaceOld) ? "RRQMRPC" : $"RRQMRPC.{nameSpaceOld}";
            List<string> refs = new List<string>();

            PropertyCodeMap propertyCode = new PropertyCodeMap(assembly, nameSpace);
            string assemblyName = $"{nameSpace}.dll";

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

            Dictionary<string, List<MethodInfo>> classAndMethods = new Dictionary<string, List<MethodInfo>>();

            foreach (MethodInstance methodInstance in methodInstances)
            {
                foreach (RPCAttribute att in methodInstance.RPCAttributes)
                {
                    if (att is RRQMRPCAttribute attribute)
                    {
                        MethodItem methodItem = new MethodItem();
                        methodItem.IsOutOrRef = methodInstance.IsByRef;
                        methodItem.MethodToken = methodInstance.MethodToken;

                        methodItem.Method = string.IsNullOrEmpty(attribute.MemberKey) ? methodInstance.Method.Name : attribute.MemberKey;
                        try
                        {
                            methodStore.AddMethodItem(methodItem);
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
            string assemblyInfo = CodeMap.GetAssemblyInfo(nameSpace, version);

            proxyInfo = new RPCProxyInfo();
            proxyInfo.AssemblyName = assemblyName;
            proxyInfo.Version = version == null ? "1.0.0.0" : version.ToString();
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

            cellCodes = codes.ToArray();
        }
    }
}