//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc;

/// <summary>
/// JsonRpc调用上下文
/// </summary>
public abstract class JsonRpcCallContextBase : CallContext, IJsonRpcCallContext
{
    private IScopedResolver m_scopedResolver;

    /// <inheritdoc/>
    public int? JsonRpcId { get; protected set; }

    ///// <summary>
    ///// JsonRpc上下文
    ///// </summary>
    //public InternalJsonRpcRequest JsonRpcContext { get; internal set; }

    internal void SetJsonRpcRequest(InternalJsonRpcRequest jsonRpcRequest)
    {
        this.JsonRpcId = jsonRpcRequest.Id;
    }

    internal void SetRpcMethod(RpcMethod rpcMethod)
    {
        this.RpcMethod = rpcMethod;
    }

    internal void SetParameters(object[] parameters)
    {
        this.Parameters = parameters;
    }

    internal void SetResolver(IResolver resolver)
    {
        this.Resolver = resolver;
    }

    internal void SetResolver(IScopedResolver scopedResolver)
    {
        this.Resolver = scopedResolver.Resolver;
        this.m_scopedResolver = scopedResolver;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }
        if (disposing)
        {
            this.m_scopedResolver.SafeDispose();
        }
        base.Dispose(disposing);
    }
}