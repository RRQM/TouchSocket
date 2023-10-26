using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识该接口方法将自动生成调用的扩展方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [Obsolete("此配置已被弃用，要使用源生成Rpc，请使用对应的Rpc特性，例如：DmtpRpc",true)]
    public sealed class GeneratorRpcMethodAttribute : RpcAttribute
    {
    }
}