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

using System.Text.Json;
using TouchSocket.Rpc;

namespace TouchSocket.Mcp;

/// <summary>
/// MCP 调用上下文的抽象基类。
/// </summary>
public abstract class McpCallContextBase : CallContext, IMcpCallContext
{
    private IScopedResolver m_scopedResolver;
    private CancellationTokenSource m_cancellation;
    private CancellationToken m_token;

    /// <summary>
    /// 初始化 <see cref="McpCallContextBase"/>。
    /// </summary>
    /// <param name="caller">调用者对象。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    protected McpCallContextBase(object caller, CancellationToken cancellationToken)
    {
        this.Caller = caller;
        this.m_token = cancellationToken;
    }

    /// <inheritdoc/>
    public JsonElement? McpRequestId { get; private set; }

    /// <inheritdoc/>
    public override CancellationToken Token => this.m_token;

    internal Func<ReadOnlyMemory<byte>, CancellationToken, Task> SendAction { get; set; }

    internal void EnableCancellation()
    {
        if (this.m_cancellation is not null)
        {
            return;
        }
        this.m_cancellation = CancellationTokenSource.CreateLinkedTokenSource(this.m_token);
        this.m_token = this.m_cancellation.Token;
    }

    internal void Cancel()
    {
        try
        {
            this.m_cancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 请求完成和取消通知可以同时到达。
        }
    }

    /// <summary>
    /// 获取当前处理的 MCP 请求。
    /// </summary>
    internal McpRequest McpRequest { get; private set; }

    internal void SetMcpRequest(McpRequest request)
    {
        this.EnableCancellation();
        this.McpRequest = request;
        this.McpRequestId = request.Id;
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
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_scopedResolver.SafeDispose();
            this.m_cancellation.SafeDispose();
        }
        base.SafetyDispose(disposing);
    }

}
