using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识该接口方法将自动生成调用的扩展方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratorRpcMethodAttribute : RpcAttribute
    {

    }
}
