//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using RRQMCore.Exceptions;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 函数仓库
    /// </summary>
    public class MethodStore
    {
        internal MethodStore()
        {
            this.methodNamesKey = new Dictionary<string, MethodItem>();
            this.serverMethodKey = new Dictionary<string, InstanceMethod>();
        }

        private Dictionary<string, InstanceMethod> serverMethodKey;

        private Dictionary<string, MethodItem> methodNamesKey;

        /// <summary>
        /// 代理文件实例
        /// </summary>
        public RPCProxyInfo ProxyInfo { get; private set; }

        /// <summary>
        /// 获取服务类实例
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RRQMException"></exception>
        public ServerProvider[] GetServerProviders()
        {
            if (this.serverMethodKey == null)
            {
                throw new RRQMException("serverMethodKey为空");
            }
            List<ServerProvider> providers = new List<ServerProvider>();
            foreach (var item in serverMethodKey.Values)
            {
                if (!providers.Contains(item.instance))
                {
                    providers.Add(item.instance);
                }
            }

            return providers.ToArray();
        }

        internal void SetProxyInfo(RPCProxyInfo proxyInfo)
        {
            this.ProxyInfo = proxyInfo;
        }

        internal void AddInstanceMethod(InstanceMethod method)
        {
            serverMethodKey.Add(method.methodItem.Method, method);
        }

        internal InstanceMethod GetInstanceMethod(string method)
        {
            if (this.serverMethodKey.ContainsKey(method))
            {
                return this.serverMethodKey[method];
            }
            return null;
        }

        internal InstanceMethod[] GetAllInstanceMethod()
        {
            List<InstanceMethod> instances = new List<InstanceMethod>();
            foreach (InstanceMethod item in this.serverMethodKey.Values)
            {
                instances.Add(item);
            }

            return instances.ToArray();
        }

        internal void AddMethodItem(MethodItem methodItem)
        {
            methodNamesKey.Add(methodItem.Method, methodItem);
        }

        internal List<MethodItem> GetAllMethodItem()
        {
            List<MethodItem> mTs = new List<MethodItem>();
            foreach (var item in methodNamesKey.Values)
            {
                mTs.Add(item);
            }

            return mTs;
        }

        private bool initialized;
        private TypeInitializeDic typeDic;
        internal MethodItem GetMethodItem(string methodName)
        {
            if (!initialized)
            {
                InitializedType(null);
            }
            if (this.methodNamesKey != null && this.methodNamesKey.ContainsKey(methodName))
            {
                return this.methodNamesKey[methodName];
            }
            else
            {
                throw new RRQMRPCException("方法初始化失败");
            }
        }

        internal void InitializedType(TypeInitializeDic typeDic)
        {
          
            this.typeDic = typeDic;
            foreach (MethodItem item in this.methodNamesKey.Values)
            {
                if (item.ReturnTypeString != null)
                {
                    item.ReturnType = this.MethodGetType(item.ReturnTypeString);
                }

                item.ParameterTypes = new List<Type>();
                if (item.ParameterTypesString != null)
                {
                    for (int i = 0; i < item.ParameterTypesString.Count; i++)
                    {
                        item.ParameterTypes.Add(this.MethodGetType(item.ParameterTypesString[i]));
                    }
                }
            }
            this.initialized = true;
        }

        private Type MethodGetType(string typeName)
        {
            if (this.typeDic!=null&&typeDic.ContainsKey(typeName))
            {
                return this.typeDic[typeName];
            }
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
                if (type == null)
                {
                    throw new RRQMRPCException($"RPC初始化时找不到{typeName}的类型");
                }
            }

            return type;
        }

        private string GetAssemblyName(string typeName)
        {
            int index = typeName.LastIndexOf(".");
            return typeName.Substring(0, index);
        }
    }
}