//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcStoreExtensions
    /// </summary>
    public static class RpcStoreExtension
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// DynamicallyAccessed
        /// </summary>
        public const DynamicallyAccessedMemberTypes DynamicallyAccessed = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicProperties;
#endif

        /// <summary>
        /// 注册<see cref="AppDomain"/>已加载程序集的所有Rpc服务
        /// </summary>
        /// <returns>返回搜索到的服务数</returns>
        public static void RegisterAllServer(this RpcStore rpcStore)
        {
            var types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                RegisterAllServer(rpcStore, assembly);
            }
        }

        /// <summary>
        /// 注册指定程序集的Rpc服务。
        /// </summary>
        /// <param name="rpcStore"></param>
        /// <param name="assembly"></param>
        public static void RegisterAllServer(this RpcStore rpcStore, Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(p => typeof(IRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass).ToArray())
            {
                rpcStore.RegisterServer(type);
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer<[DynamicallyAccessedMembers(DynamicallyAccessed)] T>(this RpcStore rpcStore) where T : IRpcServer
#else

        public static void RegisterServer<T>(this RpcStore rpcStore) where T : IRpcServer
#endif

        {
            rpcStore.RegisterServer(typeof(T));
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="rpcStore"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer(this RpcStore rpcStore, [DynamicallyAccessedMembers(DynamicallyAccessed)] Type providerType)
#else

        public static void RegisterServer(this RpcStore rpcStore, Type providerType)
#endif
        {
            rpcStore.RegisterServer(providerType, providerType);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer<[DynamicallyAccessedMembers(DynamicallyAccessed)] TFrom, [DynamicallyAccessedMembers(DynamicallyAccessed)] TTo>(this RpcStore rpcStore) where TFrom : class, IRpcServer where TTo : TFrom
#else

        public static void RegisterServer<TFrom, TTo>(this RpcStore rpcStore) where TFrom : class, IRpcServer where TTo : TFrom
#endif

        {
            rpcStore.RegisterServer(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// 注册为单例服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer<[DynamicallyAccessedMembers(DynamicallyAccessed)] TFrom>(this RpcStore rpcStore, [DynamicallyAccessedMembers(DynamicallyAccessed)] TFrom rpcServer) where TFrom : class, IRpcServer
#else

        public static void RegisterServer<TFrom>(this RpcStore rpcStore, TFrom rpcServer) where TFrom : class, IRpcServer
#endif

        {
            rpcStore.RegisterServer(typeof(TFrom), rpcServer);
        }
    }
}