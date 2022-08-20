using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcStoreExtensions
    /// </summary>
    public static class RpcStoreExtensions
    {
        /// <summary>
        /// 注册所有服务
        /// </summary>
        /// <returns>返回搜索到的服务数</returns>
        public static int RegisterAllServer(this RpcStore rpcStore)
        {
            List<Type> types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    Type[] t1 = assembly.GetTypes().Where(p => typeof(IRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass).ToArray();
                    types.AddRange(t1);
                }
                catch
                {
                }
            }

            foreach (Type type in types)
            {
                rpcStore.RegisterServer(type);
            }
            return types.Count;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void RegisterServer<T>(this RpcStore rpcStore) where T : IRpcServer
        {
            rpcStore.RegisterServer(typeof(T));
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="rpcStore"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public static void RegisterServer(this RpcStore rpcStore, Type providerType)
        {
            rpcStore.RegisterServer(providerType, providerType);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <returns></returns>
        public static void RegisterServer<TFrom, TTo>(this RpcStore rpcStore) where TFrom : class, IRpcServer where TTo : TFrom
        {
            rpcStore.RegisterServer(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// 注册为单例服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <returns></returns>
        public static void RegisterServer<TFrom>(this RpcStore rpcStore, IRpcServer rpcServer) where TFrom : class, IRpcServer
        {
            rpcStore.RegisterServer(typeof(TFrom), rpcServer);
        }
    }
}
