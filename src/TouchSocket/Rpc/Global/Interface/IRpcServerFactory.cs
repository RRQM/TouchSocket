using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// IRpcServerFactory
    /// </summary>
    public interface IRpcServerFactory
    {
        /// <summary>
        /// 创建rpc实例
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public IRpcServer Create(ICallContext callContext,object[] ps); 
    }
}
