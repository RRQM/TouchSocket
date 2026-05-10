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

using TouchSocket.Rpc;

namespace TouchSocket.Mqtt.Rpc;

/// <summary>
/// MqttRpc 调用上下文。
/// </summary>
public class MqttRpcCallContext : CallContext, IMqttRpcCallContext
{
    private IScopedResolver m_scopedResolver;

    /// <summary>
    /// 初始化 <see cref="MqttRpcCallContext"/> 的新实例。
    /// </summary>
    /// <param name="caller">调用者。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public MqttRpcCallContext(object caller, CancellationToken cancellationToken)
    {
        this.Caller = caller;
        this.Token = cancellationToken;
    }

    /// <summary>
    /// 获取请求 Id。
    /// </summary>
    public int? RequestId { get; internal set; }

    /// <summary>
    /// 获取响应主题。
    /// </summary>
    public string ResponseTopic { get; internal set; }

    /// <inheritdoc/>
    public override CancellationToken Token { get; }

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
        }
        base.SafetyDispose(disposing);
    }
}
