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
using RRQMCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 函数映射图
    /// </summary>
    public class MethodMap
    {
        internal MethodMap()
        {
            this.methodMap = new ConcurrentDictionary<int, MethodInstance>();
        }

        private ConcurrentDictionary<int, MethodInstance> methodMap;

        internal void Add(MethodInstance methodInstance)
        {
            this.methodMap.TryAdd(methodInstance.MethodToken, methodInstance);
        }

        /// <summary>
        /// 通过methodToken获取函数实例
        /// </summary>
        /// <param name="methodToken"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public bool TryGet(int methodToken, out MethodInstance methodInstance)
        {
            return this.methodMap.TryGetValue(methodToken, out methodInstance);
        }

        internal bool RemoveServer(Type type, out IServerProvider serverProvider, out MethodInstance[] methodInstances)
        {
            serverProvider = null;
            bool success = false;
            List<MethodInstance> keys = new List<MethodInstance>();
            foreach (var methodInstance in this.methodMap.Values)
            {
                if (methodInstance.Provider.GetType().FullName == type.FullName)
                {
                    success = true;
                    serverProvider = methodInstance.Provider;
                    keys.Add(methodInstance);
                }
            }

            foreach (var item in keys)
            {
                this.methodMap.TryRemove(item.MethodToken, out _);
            }
            methodInstances = keys.ToArray();
            return success;
        }

        /// <summary>
        /// 获取所有服务函数实例
        /// </summary>
        /// <returns></returns>
        public MethodInstance[] GetAllMethodInstances()
        {
            return this.methodMap.Values.ToArray();
        }
    }
}