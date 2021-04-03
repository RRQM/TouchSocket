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
using RRQMCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        private RPCProxyInfo proxyInfo;
        private string proxyToken;

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

        internal void SetProxyInfo(RPCProxyInfo proxyInfo, string proxyToken)
        {
            this.proxyInfo = proxyInfo;
            this.proxyToken = proxyToken;
        }

        internal RPCProxyInfo GetProxyInfo(string proxyToken)
        {
            RPCProxyInfo proxyInfo = new RPCProxyInfo();
            if (this.proxyToken == proxyToken)
            {
                proxyInfo.AssemblyData = this.proxyInfo.AssemblyData;
                proxyInfo.AssemblyName = this.proxyInfo.AssemblyName;
                proxyInfo.Codes = this.proxyInfo.Codes;
                proxyInfo.Version = this.proxyInfo.Version;
                proxyInfo.Status = 1;
            }
            else
            {
                proxyInfo.Status = 2;
                proxyInfo.Message = "令箭不正确";
            }

            return proxyInfo;
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

        internal MethodItem[] GetAllMethodItem()
        {
            List<MethodItem> mTs = new List<MethodItem>();
            foreach (var item in methodNamesKey.Values)
            {
                mTs.Add(item);
            }

            return mTs.ToArray();
        }

        private bool initialized;

        internal MethodItem GetMethodItem(string methodName)
        {
            if (!this.initialized)
            {
                InitializedType();
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

        internal void InitializedType()
        {
            foreach (MethodItem item in this.methodNamesKey.Values)
            {
                if (item.ReturnTypeString != null)
                {
                    item.ReturnType = this.MethodGetType(item.ReturnTypeString);
                }

                item.ParameterTypes = new Type[item.ParameterTypesString.Length];
                for (int i = 0; i < item.ParameterTypesString.Length; i++)
                {
                    item.ParameterTypes[i] = this.MethodGetType(item.ParameterTypesString[i]);
                }
            }
            this.initialized = true;
        }

        private Type MethodGetType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                Assembly assembly = Assembly.Load(GetAssemblyName(typeName));
                type = assembly.GetType(typeName);
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