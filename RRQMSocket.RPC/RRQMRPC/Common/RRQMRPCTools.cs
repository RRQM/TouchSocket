using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class RRQMRPCTools
    {
        /// <summary>
        /// 是否抛出异常
        /// </summary>
        /// <param name="context"></param>
        public static void ThrowRPCStatus(RpcContext context)
        {
            switch (context.Status)
            {
                case 0:
                    {
                        throw new RRQMRPCException($"返回状态异常，信息：{context.Message}");
                    }
                case 1:
                    {
                        return;//正常。
                    }
                case 2:
                    {
                        throw new RRQMRPCInvokeException($"未找到该公共方法，或该方法未标记{nameof(RPCAttribute)}");
                    }
                case 3:
                    {
                        throw new RRQMRPCException("该方法已被禁用");
                    }
                case 4:
                    {
                        throw new RRQMRPCException($"服务器已阻止本次行为，信息：{context.Message}");
                    }
                case 5:
                    {
                        throw new RRQMRPCInvokeException($"函数执行异常，详细信息：{context.Message}");
                    }
                case 6:
                    {
                        throw new RRQMRPCException($"函数异常，信息：{context.Message}");
                    }
                default:
                    throw new RRQMRPCException($"未知状态定义，信息：{context.Message}");
            }
        }
    }
}
