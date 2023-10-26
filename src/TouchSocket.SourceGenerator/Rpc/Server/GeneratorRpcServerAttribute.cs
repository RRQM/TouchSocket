using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 标识将通过源生成器生成Rpc服务的调用委托。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class GeneratorRpcServerAttribute : Attribute
    {
    }
}