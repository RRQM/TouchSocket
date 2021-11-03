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
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 工具
    /// </summary>
    internal static class GlobalTools
    {
        internal static bool MethodEquals(MethodInfo m1, MethodInfo m2)
        {
            if (m1.GetCustomAttributes().Count() != m2.GetCustomAttributes().Count())
            {
                return false;
            }
            if (m1.Name != m2.Name)
            {
                return false;
            }
            if (m1.ReturnType.FullName != m2.ReturnType.FullName)
            {
                return false;
            }

            ParameterInfo[] ps1 = m1.GetParameters();
            ParameterInfo[] ps2 = m2.GetParameters();
            if (ps1.Length != ps2.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < ps1.Length; i++)
                {
                    if (ps1[i].ParameterType.FullName != ps2[i].ParameterType.FullName)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static int nullReturnNullParameters = 100000000;
        private static int nullReturnExistParameters = 300000000;
        private static int ExistReturnNullParameters = 500000000;
        private static int ExistReturnExistParameters = 700000000;

        internal static MethodInstance[] GetMethodInstances(IServerProvider serverProvider, bool isSetToken)
        {
            List<MethodInstance> instances = new List<MethodInstance>();

            MethodInfo[] methodInfos = serverProvider.GetType().GetMethods();

            foreach (MethodInfo method in methodInfos)
            {
                if (method.IsGenericMethod)
                {
                    continue;
                }
                IEnumerable<RPCAttribute> attributes = method.GetCustomAttributes<RPCAttribute>(true);
                if (attributes.Count() > 0)
                {
                    MethodInstance methodInstance = new MethodInstance();
                    methodInstance.Provider = serverProvider;
                    methodInstance.ProviderType = serverProvider.GetType();
                    methodInstance.Method = method;
                    methodInstance.RPCAttributes = attributes.ToArray();
                    methodInstance.DescriptionAttributes = method.GetCustomAttributes<DescriptionAttribute>(true).ToArray();
                    methodInstance.IsEnable = true;
                    methodInstance.Parameters = method.GetParameters();
                    foreach (var item in attributes)
                    {
                        methodInstance.MethodFlags |= item.MethodFlags;
                    }
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (methodInstance.Parameters.Length == 0 || !typeof(IServerCallContext).IsAssignableFrom(methodInstance.Parameters[0].ParameterType))
                        {
                            throw new RRQMRPCException($"函数：{method}，标识包含{MethodFlags.IncludeCallContext}时，必须包含{nameof(IServerCallContext)}或其派生类参数，且为第一参数。");
                        }
                    }
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

                    if (method.ReturnType == typeof(void) || method.ReturnType == typeof(Task))
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