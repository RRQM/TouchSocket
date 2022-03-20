//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 代码生成器
    /// </summary>
    public static class CodeGenerator
    {
        private static Dictionary<Type, string> proxyType = new Dictionary<Type, string>();

        /// <summary>
        /// 生成回调。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInstance"></param>
        /// <param name="methodCellCode"></param>
        /// <returns></returns>
        public delegate bool GeneratorCallback<T>(MethodInstance methodInstance, MethodCellCode methodCellCode)where T:RpcAttribute;

        /// <summary>
        /// 添加代理类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="deepSearch"></param>
        public static void AddProxyType(Type type, bool deepSearch = true)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                return;
            }
            if (!proxyType.ContainsKey(type))
            {
                RRQMProxyAttribute attribute = type.GetCustomAttribute<RRQMProxyAttribute>();
                proxyType.Add(type, attribute == null ? type.Name : attribute.ClassName);
                if (deepSearch)
                {
                    PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
                    foreach (var item in properties)
                    {
                        AddProxyType(item.PropertyType);
                    }
                }
            }
        }

        /// <summary>
        /// 添加代理类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deepSearch"></param>
        public static void AddProxyType<T>(bool deepSearch = true)
        {
            AddProxyType(typeof(T), deepSearch);
        }

        /// <summary>
        /// 是否包含类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ContainsType(Type type)
        {
            return proxyType.ContainsKey(type);
        }

        /// <summary>
        /// 转换为cs代码。
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="serverCodes"></param>
        /// <returns></returns>
        public static string ConvertToCode(string @namespace, params ServerCellCode[] serverCodes)
        {
            Dictionary<string, ServerCellCode> serverCellCodes = new Dictionary<string, ServerCellCode>();
            Dictionary<string, ClassCellCode> classCellCodes = new Dictionary<string, ClassCellCode>();
            StringBuilder codeString = new StringBuilder();
            foreach (var serverCellCode in serverCodes)
            {
                if (serverCellCodes.ContainsKey(serverCellCode.Name))
                {
                    var ccm = serverCellCodes[serverCellCode.Name].Methods;
                    foreach (var item in serverCellCode.Methods.Keys)
                    {
                        if (!ccm.ContainsKey(item))
                        {
                            ccm.Add(item, serverCellCode.Methods[item]);
                        }
                    }
                }
                else
                {
                    serverCellCodes.Add(serverCellCode.Name, serverCellCode);
                }

                foreach (var item in serverCellCode.ClassCellCodes.Keys)
                {
                    if (!classCellCodes.ContainsKey(item))
                    {
                        classCellCodes.Add(item, serverCellCode.ClassCellCodes[item]);
                    }
                }
            }

            string namesp = string.IsNullOrEmpty(@namespace) ? "RRQMProxy" : @namespace;

            codeString.AppendLine("using System;");
            codeString.AppendLine("using RRQMCore;");
            codeString.AppendLine("using RRQMSocket;");
            codeString.AppendLine("using RRQMSocket.RPC;");
            codeString.AppendLine("using RRQMSocket.RPC.RRQMRPC;");
            codeString.AppendLine("using System.Collections.Generic;");
            codeString.AppendLine("using System.Diagnostics;");
            codeString.AppendLine("using System.Text;");
            codeString.AppendLine("using System.Threading.Tasks;");
            codeString.AppendLine(string.Format("namespace {0}", namesp));
            codeString.AppendLine("{");

            foreach (var serverCellCode in serverCellCodes.Values)
            {
                //接口
                codeString.AppendLine($"public interface I{serverCellCode.Name}:IRemoteServer");//类开始
                codeString.AppendLine("{");
                foreach (var item in serverCellCode.Methods.Values)
                {
                    codeString.AppendLine(item.GetInterfaceCode());
                }
                codeString.AppendLine("}");
                //接口

                //类
                codeString.AppendLine($"public class {serverCellCode.Name} :I{serverCellCode.Name}");//类开始
                codeString.AppendLine("{");
                codeString.AppendLine($"public {serverCellCode.Name}(IRpcClient client)");
                codeString.AppendLine("{");
                codeString.AppendLine("this.Client=client;");
                codeString.AppendLine("}");
                codeString.AppendLine("public IRpcClient Client{get;private set; }");
                foreach (var item in serverCellCode.Methods.Values)
                {
                    codeString.AppendLine(item.GetCode());
                }
                codeString.AppendLine("}");
                //类
            }

            foreach (var item in classCellCodes.Values)
            {
                codeString.AppendLine(item.Code);
            }

            codeString.AppendLine("}");

            return codeString.ToString();
        }

        /// <summary>
        /// 生成代码代理
        /// </summary>
        /// <typeparam name="TServer">服务类型</typeparam>
        /// <typeparam name="TAttribute">属性标签</typeparam>
        /// <param name="onMethod">当每次使用<see cref="MethodInstance"/>生成<see cref="MethodCellCode"/>时</param>
        /// <returns></returns>
        public static ServerCellCode Generator<TServer, TAttribute>(GeneratorCallback<TAttribute> onMethod = default) where TServer : IServerProvider where TAttribute : RpcAttribute
        {
            return Generator<TAttribute>(typeof(TServer), onMethod);
        }

        /// <summary>
        /// 生成代码代理
        /// </summary>
        /// <typeparam name="TAttribute">属性标签</typeparam>
        /// <param name="serverType">服务类型</param>
        /// <param name="onMethod">当每次使用<see cref="MethodInstance"/>生成<see cref="MethodCellCode"/>时</param>
        /// <returns></returns>
        public static ServerCellCode Generator<TAttribute>(Type serverType, GeneratorCallback<TAttribute> onMethod = default) where TAttribute : RpcAttribute
        {
            ServerCellCode serverCellCode = new ServerCellCode();
            MethodInstance[] methodInstances = GetMethodInstances(serverType);

            ClassCodeGenerator classCodeGenerator = new ClassCodeGenerator(serverType.Assembly);

            serverCellCode.Name = serverType.Name;
            List<MethodInstance> instances = new List<MethodInstance>();

            foreach (MethodInstance methodInstance in methodInstances)
            {
                foreach (RpcAttribute att in methodInstance.RpcAttributes)
                {
                    if (att is TAttribute)
                    {
                        if (methodInstance.ReturnType != null)
                        {
                            classCodeGenerator.AddTypeString(methodInstance.ReturnType);
                        }

                        int i = 0;
                        if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                        {
                            i = 1;
                        }
                        for (; i < methodInstance.ParameterTypes.Length; i++)
                        {
                            classCodeGenerator.AddTypeString(methodInstance.ParameterTypes[i]);
                        }

                        instances.Add(methodInstance);
                        break;
                    }
                }
            }
            foreach (var item in classCodeGenerator.GetClassCellCodes())
            {
                serverCellCode.ClassCellCodes.Add(item.Name, item);
            }

            ServerCodeGenerator serverCodeGenerator = new ServerCodeGenerator(classCodeGenerator);

            foreach (var item in instances)
            {
                MethodCellCode methodCellCode = new MethodCellCode();
                methodCellCode.InterfaceTemple = serverCodeGenerator.GetInterfaceProxy<TAttribute>(item);
                methodCellCode.CodeTemple = serverCodeGenerator.GetMethodProxy<TAttribute>(item);
                methodCellCode.Name = CodeGenerator.GetMethodName<TAttribute>(item);
                if (onMethod != null)
                {
                    if (onMethod.Invoke(item, methodCellCode))
                    {
                        serverCellCode.Methods.Add(methodCellCode.Name, methodCellCode);
                    }
                }
                else
                {
                    serverCellCode.Methods.Add(methodCellCode.Name, methodCellCode);
                }
            }

            return serverCellCode;
        }

        /// <summary>
        /// 从类型获取函数实例
        /// </summary>
        /// <typeparam name="TServer"></typeparam>
        /// <returns></returns>
        public static MethodInstance[] GetMethodInstances<TServer>() where TServer : IServerProvider
        {
            return GetMethodInstances(typeof(TServer));
        }

        /// <summary>
        /// 从类型获取函数实例
        /// </summary>
        /// <param name="serverType"></param>
        /// <returns></returns>
        public static MethodInstance[] GetMethodInstances(Type serverType)
        {
            if (!typeof(IServerProvider).IsAssignableFrom(serverType))
            {
                throw new RpcException($"服务类型必须从{nameof(IServerProvider)}派生。");
            }
            List<MethodInstance> instances = new List<MethodInstance>();

            MethodInfo[] methodInfos = serverType.GetMethods();

            foreach (MethodInfo method in methodInfos)
            {
                if (method.IsGenericMethod)
                {
                    continue;
                }
                IEnumerable<RpcAttribute> attributes = method.GetCustomAttributes<RpcAttribute>(true);
                if (attributes.Count() > 0)
                {
                    MethodInstance methodInstance = new MethodInstance(method);
                    methodInstance.Provider = null;
                    methodInstance.ProviderType = serverType;
                    methodInstance.RpcAttributes = attributes.ToArray();
                    methodInstance.DescriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
                    methodInstance.IsEnable = true;
                    methodInstance.Parameters = method.GetParameters();
                    foreach (var item in attributes)
                    {
                        if (item.Async)
                        {
                            methodInstance.AsyncType |= AsyncType.Async;
                        }
                        methodInstance.MethodFlags |= item.MethodFlags;
                    }
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (methodInstance.Parameters.Length == 0 || !typeof(ICallContext).IsAssignableFrom(methodInstance.Parameters[0].ParameterType))
                        {
                            throw new RpcException($"函数：{method}，标识包含{MethodFlags.IncludeCallContext}时，必须包含{nameof(ICallContext)}或其派生类参数，且为第一参数。");
                        }
                    }
                    List<string> names = new List<string>();
                    foreach (var parameterInfo in methodInstance.Parameters)
                    {
                        names.Add(parameterInfo.Name);
                    }
                    methodInstance.ParameterNames = names.ToArray();
                    ParameterInfo[] parameters = method.GetParameters();
                    List<Type> types = new List<Type>();
                    foreach (var parameter in parameters)
                    {
                        types.Add(parameter.ParameterType.GetRefOutType());
                    }
                    methodInstance.ParameterTypes = types.ToArray();
                    instances.Add(methodInstance);
                }
            }

            return instances.ToArray();
        }

        /// <summary>
        /// 获取函数名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public static string GetMethodName<T>(MethodInstance methodInstance) where T : RpcAttribute
        {
            string mname = methodInstance.GetAttribute<T>().MethodName;
            string methodName;
            if (string.IsNullOrEmpty(mname))
            {
                methodName = methodInstance.Name;
            }
            else
            {
                methodName = mname;
            }
            return methodName;
        }

        /// <summary>
        /// 获取类型代理名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool TryGetProxyTypeName(Type type, out string className)
        {
            return proxyType.TryGetValue(type, out className);
        }
    }
}