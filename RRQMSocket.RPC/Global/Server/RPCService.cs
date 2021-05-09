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
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            this.MethodMap = new MethodMap();
        }

        /// <summary>
        /// 获取函数映射图实例
        /// </summary>
        public MethodMap MethodMap { get; private set; }

        /// <summary>
        /// 获取RPC解析器集合
        /// </summary>
        public RPCParserCollection RPCParsers { get; private set; }

        /// <summary>
        /// 添加RPC解析器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parser"></param>
        public void AddRPCParser(string key, RPCParser parser)
        {
            this.RPCParsers.Add(key, parser);
            parser.RPCService = this;
            parser.RRQMExecuteMethod = PreviewExecuteMethod;
            parser.RRQMSetMethodMap(this.MethodMap);
        }

        /// <summary>
        /// 获取函数实例
        /// </summary>
        public MethodInstance[] MethodInstances { get; private set; }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public ServerProviderCollection ServerProviders { get; private set; }

        /// <summary>
        /// 设置服务方法可用性
        /// </summary>
        /// <param name="methodToken">方法名</param>
        /// <param name="enable">可用性</param>
        /// <exception cref="RRQMRPCException"></exception>
        public void SetMethodEnable(int methodToken, bool enable)
        {
            if (this.MethodMap.TryGet(methodToken, out MethodInstance methodInstance))
            {
                methodInstance.IsEnable = enable;
            }
            else
            {
                throw new RRQMRPCException("未找到该方法");
            }
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
        public void OpenRPCServer()
        {
            if (this.ServerProviders.Count == 0)
            {
                throw new RRQMRPCException("已注册服务数量为0");
            }

            if (this.RPCParsers.Count == 0)
            {
                throw new RRQMRPCException("请至少添加一种RPC解析器");
            }

            List<MethodInstance> methodInstances = new List<MethodInstance>();

            int nullReturnNullParameters = 10000000;
            int nullReturnExistParameters = 30000000;
            int ExistReturnNullParameters = 50000000;
            int ExistReturnExistParameters = 70000000;

            foreach (ServerProvider instance in this.ServerProviders)
            {
                MethodInfo[] methodInfos = instance.GetType().GetMethods();
                foreach (MethodInfo method in methodInfos)
                {
                    if (method.IsGenericMethod)
                    {
                        throw new RRQMRPCException("RPC方法中不支持泛型参数");
                    }
                    IEnumerable<RPCMethodAttribute> attributes = method.GetCustomAttributes<RPCMethodAttribute>(true);
                    if (attributes.Count() > 0)
                    {
                        MethodInstance methodInstance = new MethodInstance();
                        methodInstance.Provider = instance;
                        methodInstance.Method = method;
                        methodInstance.RPCAttributes = attributes.ToArray();
                        methodInstance.IsEnable = true;
                        ParameterInfo[] parameterInfos = method.GetParameters();
                        methodInstance.Parameters = new ReadOnlyDictionary<string, ParameterInfo>();
                        foreach (var parameterInfo in parameterInfos)
                        { 
                            methodInstance.Parameters.Add(parameterInfo.Name, parameterInfo);
                        }

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

                        if (method.ReturnType == typeof(void))
                        {
                            methodInstance.ReturnType = null;

                            if (parameters.Length == 0)
                            {
                                methodInstance.MethodToken = ++nullReturnNullParameters;
                            }
                            else
                            {
                                methodInstance.MethodToken = ++nullReturnExistParameters;
                            }
                        }
                        else
                        {
                            if (methodInstance.Async)
                            {
                                methodInstance.ReturnType = method.ReturnType.GetGenericArguments()[0];
                            }
                            else
                            {
                                methodInstance.ReturnType = method.ReturnType;
                            }


                            if (parameters.Length == 0)
                            {
                                methodInstance.MethodToken = ++ExistReturnNullParameters;
                            }
                            else
                            {
                                methodInstance.MethodToken = ++ExistReturnExistParameters;
                            }
                        }

                        methodInstances.Add(methodInstance);
                        this.MethodMap.Add(methodInstance);
                    }
                }
            }

            this.MethodInstances = methodInstances.ToArray();

            foreach (var parser in this.RPCParsers)
            {
                parser.RRQMInitializeServers(this.MethodInstances);
            }
        }

        private void PreviewExecuteMethod(RPCParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            if (methodInstance != null && methodInstance.Async)
            {
                Task.Run(() =>
                {
                    ExecuteMethod(true, parser, methodInvoker, methodInstance);
                });
            }
            else
            {
                ExecuteMethod(false, parser, methodInvoker, methodInstance);
            }
        }

        private void ExecuteMethod(bool isAsync, RPCParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            if (methodInvoker.Status == InvokeStatus.Ready && methodInstance != null)
            {
                try
                {
                    methodInstance.Provider.RPC(1, parser, methodInvoker, methodInstance);
                    if (isAsync)
                    {
                        dynamic task = methodInstance.Method.Invoke(methodInstance.Provider, methodInvoker.Parameters);
                        task.Wait();
                        methodInvoker.ReturnParameter = task.Result;
                    }
                    else
                    {
                        methodInvoker.ReturnParameter = methodInstance.Method.Invoke(methodInstance.Provider, methodInvoker.Parameters);
                    }
                    methodInstance.Provider.RPC(3, parser, methodInvoker, methodInstance);
                    methodInvoker.Status = InvokeStatus.Success;
                }
                catch (RRQMAbandonRPCException e)
                {
                    methodInvoker.Status = InvokeStatus.Abort;
                    methodInvoker.StatusMessage = "函数被阻止执行，信息：" + e.Message;
                }
                catch (TargetInvocationException e)
                {
                    methodInvoker.Status = InvokeStatus.InvocationException;
                    if (e.InnerException != null)
                    {
                        methodInvoker.StatusMessage = "函数内部发生异常，信息：" + e.InnerException.Message;
                    }
                    else
                    {
                        methodInvoker.StatusMessage = "函数内部发生异常，信息：未知";
                    }
                    methodInstance.Provider.RPC(1, parser, methodInvoker, methodInstance);
                }
                catch (Exception e)
                {
                    methodInvoker.Status = InvokeStatus.Exception;
                    methodInvoker.StatusMessage = e.Message;
                    methodInstance.Provider.RPC(1, parser, methodInvoker, methodInstance);
                }
            }

            parser.RRQMEndInvokeMethod(methodInvoker, methodInstance);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            foreach (var item in this.RPCParsers)
            {
                item.Dispose();
            }
            this.RPCParsers = null;
        }
    }
}