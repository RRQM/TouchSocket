//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
    internal static class RRQMRPCTools
    {
        internal static void GetRPCMethod(
            MethodInstance[] methodInstances,
            string nameSpaceOld,
            ref MethodStore methodStore,
             Version version,
             ref RpcProxyInfo proxyInfo)
        {
            string nameSpace = string.IsNullOrEmpty(nameSpaceOld) ? "RRQMRPC" : $"RRQMRPC.{nameSpaceOld}";
            List<string> refs = new List<string>();

            PropertyCodeGenerator propertyCode = new PropertyCodeGenerator(nameSpace, methodStore);

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

                        int i = 0;
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            i = 1;
                        }
                        for (; i < methodInstance.ParameterTypes.Length; i++)
                        {
                            refs.Add(methodInstance.ParameterTypes[i].Assembly.Location);
                            propertyCode.AddTypeString(methodInstance.ParameterTypes[i]);
                        }

                        break;
                    }
                }
            }

#if NET45_OR_GREATER
            foreach (var item in refs)
            {
                RpcCompiler.AddRef(item);
            }
#endif
            string className = null;

            List<MethodInstance> instances = new List<MethodInstance>();
            foreach (MethodInstance methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<RRQMRPCAttribute>() is RRQMRPCAttribute attribute)
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
                    instances.Add(methodInstance);
                }
            }

            CodeGenerator.Namespace = nameSpace;
            CodeGenerator.PropertyCode = propertyCode;

            CodeGenerator codeMap = new CodeGenerator();
            codeMap.ClassName = className;
            codeMap.MethodInstances = instances.ToArray();

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

    }
}