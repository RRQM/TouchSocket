using RRQMCore;
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 工具
    /// </summary>
    internal static class GlobalTools
    {
        private static int nullReturnNullParameters   = 100000000;
        private static int nullReturnExistParameters  = 300000000;
        private static int ExistReturnNullParameters  = 500000000;
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
                    methodInstance.IsEnable = true;
                    methodInstance.Parameters = method.GetParameters();
                    foreach (var item in attributes)
                    {
                        methodInstance.MethodFlags |= item.MethodFlags;
                    }
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (methodInstance.Parameters.Length==0||!typeof(IServerCallContext).IsAssignableFrom(methodInstance.Parameters[0].ParameterType) )
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
