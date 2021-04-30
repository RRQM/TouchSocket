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

        internal bool TryGet(int methodToken, out MethodInstance methodInstance)
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
