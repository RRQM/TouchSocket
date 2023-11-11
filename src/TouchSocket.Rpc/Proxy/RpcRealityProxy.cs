using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 透明代理
    /// </summary>
    public abstract class RpcRealityProxy : RealProxy
    {
        protected static IRpcClient RpcClient;

        public RpcRealityProxy() { }
        private RpcRealityProxy(Type t) : base(t) { }

        /// <summary>
        /// 调用过程
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected sealed override IMessage Invoke(IMessage msg)
        {
            //方法信息
            IMethodMessage methodMessage = msg as IMethodMessage;
            IMethodCallMessage methodCall = msg as IMethodCallMessage;
            MethodInfo method = methodCall.MethodBase as MethodInfo;

            //获取调用方法
            var rpcOption = method.GetCustomAttribute<RpcOptionAttribute>() as RpcOptionAttribute;
            var rpcRoute = method.GetCustomAttribute<RpcRouteAttribute>() as RpcRouteAttribute;
            var invokeKey = rpcRoute == null ? method.Name : rpcRoute.Route;
            var client = BuilderClient();
            var option = BuilderOption(rpcOption == null ? default : rpcOption.Option, rpcOption == null ? 5000 : rpcOption.Timeout);

            //获取参数信息
            object result = default;
            var args = methodCall.Args.ToArray();
            var argsTypes = method.GetParameters().Select(x => x.ParameterType.GetRefOutType()).ToArray();

            OnBefore(invokeKey, ref args);

            //开始执行调用
            if (method.ReturnType == typeof(Task))
            {
                client.Invoke(invokeKey, option, ref args, argsTypes);
                result = Task.CompletedTask;
            }
            else if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var returnType = method.ReturnType.GetGenericArguments()[0];
                var taskValue = client.Invoke(returnType, invokeKey, option, ref args, argsTypes);
                var taskMethod = new Method(typeof(Task).GetMethod("FromResult").MakeGenericMethod(returnType));
                result = taskMethod.Invoke(default, taskValue);
            }
            else if (method.ReturnType == typeof(void))
            {
                client.Invoke(invokeKey, option, ref args, argsTypes);
            }
            else
            {
                result = client.Invoke(method.ReturnType, invokeKey, option, ref args, argsTypes);
            }

            OnAfter(invokeKey, ref args, ref result);

            return new ReturnMessage(result, args, args.Length, methodCall.LogicalCallContext, methodCall);
        }

        /// <summary>
        /// 创建代理对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>()
        {
            var proxy = new RpcRealityProxy(typeof(T));
            return (T)proxy.GetTransparentProxy();
        }

        /// <summary>
        /// 创建代理对象
        /// </summary>
        /// <returns></returns>
        public static RpcRealityProxy Create()
        {
            return new RpcRealityProxy();
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="method">方法名称</param>
        /// <param name="args">参数集合</param>
        public virtual T Invoke<T>(string method, params object[] args)
        {
            var client = BuilderClient();
            OnBefore(method, ref args);
            object result = client.InvokeT<T>(method, BuilderOption(), args);
            OnAfter(method, ref args, ref result);
            return (T)result;
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="method">方法名称</param>
        /// <param name="args">参数集合</param>
        /// <returns></returns>
        public virtual async Task<T> InvokeAsync<T>(string method, params object[] args)
        {
            var client = BuilderClient();
            OnBefore(method, ref args);
            object result = await client.InvokeTAsync<T>(method, BuilderOption(), args);
            OnAfter(method, ref args, ref result);
            return (T)result;
        }

        /// <summary>
        /// 方法调用前
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual void OnBefore(string method, ref object[] args)
        {

        }

        /// <summary>
        /// 方法调用后
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual void OnAfter(string method, ref object[] args, ref object result)
        {

        }

        /// <summary>
        /// 构建RPC客户端
        /// </summary>
        /// <returns></returns>
        protected abstract override IRpcClient BuilderClient();

        /// <summary>
        /// 构建RPC选项值
        /// </summary>
        /// <param name="rpcOption"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected abstract override InvokeOption BuilderOption(IInvokeOption rpcOption = InvokeOption.WaitInvoke, int timeout = 5000);

    }
}
