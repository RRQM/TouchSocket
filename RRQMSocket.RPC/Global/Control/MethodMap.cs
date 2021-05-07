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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 函数映射图
    /// </summary>
    public class MethodMap
    {
        internal MethodMap()
        {
            this.methodMap = new Dictionary<int, MethodInstance>();
        }
        private Dictionary<int, MethodInstance> methodMap;

        internal void Add(MethodInstance methodInstance)
        {
            this.methodMap.Add(methodInstance.MethodToken, methodInstance);
        }

        /// <summary>
        /// 通过methodToken获取函数实例
        /// </summary>
        /// <param name="methodToken"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public bool TryGet(int methodToken, out MethodInstance methodInstance)
        {
            if (this.methodMap.ContainsKey(methodToken))
            {
                methodInstance = this.methodMap[methodToken];
                return true;
            }
            methodInstance = null;
            return false;
        }
    }
}
