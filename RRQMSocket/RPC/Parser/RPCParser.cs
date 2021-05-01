using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC解析器
    /// </summary>
    public abstract class RPCParser:IDisposable
    {
       
        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        /// <summary>
        /// 获取函数映射图
        /// </summary>
        protected MethodMap MethodMap { get;private set; }

        /// <summary>
        /// 包含此解析器的服务器实例
        /// </summary>
        public RPCService RPCService { get;internal set; }

        internal Action<RPCParser, MethodInvoker,MethodInstance> RRQMExecuteMethod;

        internal void RRQMInitializeServers(MethodInstance[] methodInstances)
        {
            InitializeServers(methodInstances);
        }
        
        internal void RRQMEndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            EndInvokeMethod(methodInvoker, methodInstance);
        } 
        
        internal void RRQMSetMethodMap(MethodMap  methodMap)
        {
            this.MethodMap = methodMap;
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="methodInstances"></param>
        protected abstract void InitializeServers(MethodInstance[] methodInstances);

        /// <summary>
        /// 在函数调用完成后调用
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected abstract void EndInvokeMethod(MethodInvoker methodInvoker, MethodInstance methodInstance);

        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected void ExecuteMethod(MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            RRQMExecuteMethod?.Invoke(this, methodInvoker, methodInstance);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public abstract void Dispose();
    }
}
