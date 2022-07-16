using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcParserExtensions
    /// </summary>
    public static class RpcParserExtensions
    {
        /// <summary>
        /// 获取本地代理
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="namespace"></param>
        /// <param name="attrbuteTypes"></param>
        /// <returns></returns>
        public static string GetProxyCodes(this IRpcParser rpcParser, string @namespace, Type[] attrbuteTypes)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.GetProxyCodes(@namespace, attrbuteTypes);
        }

        /// <summary>
        ///  获取本地代理
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public static string GetProxyCodes(this IRpcParser rpcParser, string @namespace)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.GetProxyCodes(@namespace, null);
        }

        /// <summary>
        /// 注册所有服务
        /// </summary>
        /// <returns>返回搜索到的服务数</returns>
        public static int RegisterAllServer(this IRpcParser rpcParser)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.RegisterAllServer();
        }

        /// <summary>
        /// 注册所有服务
        /// </summary>
        /// <returns>返回注册实例</returns>
        public static IRpcServer RegisterServer<TFrom, TTo>(this IRpcParser rpcParser) where TFrom : class, IRpcServer where TTo : TFrom
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.RegisterServer<TFrom, TTo>();
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="providerInterfaceType"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public static IRpcServer RegisterServer(this IRpcParser rpcParser, Type providerInterfaceType, Type providerType)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.RegisterServer(providerInterfaceType, providerType);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="rpcParser"></param>
        /// <param name="serverProvider"></param>
        public static void RegisterServer<TFrom>(this IRpcParser rpcParser, IRpcServer serverProvider) where TFrom : class, IRpcServer
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            rpcParser.RpcStore.RegisterServer<TFrom>(serverProvider);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="providerInterfaceType"></param>
        /// <param name="serverProvider"></param>
        public static void RegisterServer(this IRpcParser rpcParser, Type providerInterfaceType, IRpcServer serverProvider)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            rpcParser.RpcStore.RegisterServer(providerInterfaceType, serverProvider);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IRpcServer RegisterServer<T>(this IRpcParser rpcParser) where T : IRpcServer
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.RegisterServer<T>();
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public static IRpcServer RegisterServer(this IRpcParser rpcParser, Type providerType)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.RegisterServer(providerType);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="serverProvider"></param>
        public static void RegisterServer(this IRpcParser rpcParser, IRpcServer serverProvider)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            rpcParser.RpcStore.RegisterServer(serverProvider);
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static int UnregisterServer(this IRpcParser rpcParser, IRpcServer provider)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.UnregisterServer(provider);
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="rpcParser"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public static int UnregisterServer(this IRpcParser rpcParser, Type providerType)
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.UnregisterServer(providerType);
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int UnregisterServer<T>(this IRpcParser rpcParser) where T : RpcServer
        {
            if (rpcParser.RpcStore == null)
            {
                throw new ArgumentNullException(nameof(rpcParser.RpcStore), $"RpcStore为空，这一般是该解析器没有完成初始化配置所导致的。");
            }
            return rpcParser.RpcStore.UnregisterServer<T>();
        }
    }
}