using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMSocket.Http;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// Api结果转化器
    /// </summary>
    public abstract class ResultConverter
    {
        /// <summary>
        /// 在调用完成时转换结果
        /// </summary>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        protected abstract HttpResponse OnResultConverter(MethodInvoker methodInvoker, MethodInstance methodInstance);
    }
}
