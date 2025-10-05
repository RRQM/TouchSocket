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
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 表示一个TCP JsonRpc客户端。
/// </summary>
public class TcpJsonRpcClient : TcpClientBase, ITcpJsonRpcClient
{
    private readonly JsonRpcActor m_jsonRpcActor;

    /// <summary>
    /// 初始化 <see cref="TcpJsonRpcClient"/> 类的新实例。
    /// </summary>
    public TcpJsonRpcClient()
    {
        this.m_jsonRpcActor = new JsonRpcActor()
        {
            SendAction = this.SendAction,
            SerializerConverter = this.SerializerConverter
        };
    }

    #region JsonRpcActor

    private Task SendAction(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        return base.ProtectedSendAsync(memory, cancellationToken);
    }

    #endregion JsonRpcActor

    /// <summary>
    /// 获取JsonRpc的调用键。
    /// </summary>
    public ActionMap ActionMap => this.m_jsonRpcActor.ActionMap;

    /// <inheritdoc/>
    public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter { get; private set; }

    /// <inheritdoc/>
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        return this.TcpConnectAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<object> InvokeAsync(string invokeKey, Type returnType, InvokeOption invokeOption, params object[] parameters)
    {
        return this.m_jsonRpcActor.InvokeAsync(invokeKey, returnType, invokeOption, parameters);
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_jsonRpcActor.SafeDispose();
        }
        base.SafetyDispose(disposing);
    }

    /// <inheritdoc/>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);
        this.m_jsonRpcActor.Logger = this.Logger;
        this.m_jsonRpcActor.Resolver = this.Resolver;
        var rpcServerProvider = this.Resolver.Resolve<IRpcServerProvider>();
        if (rpcServerProvider is not null)
        {
            this.m_jsonRpcActor.SetRpcServerProvider(rpcServerProvider);
        }

        var jsonRpcOption = config.GetValue(JsonRpcConfigExtension.JsonRpcOptionProperty) ?? new JsonRpcOption();

        this.SerializerConverter = jsonRpcOption.SerializerConverter;
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        var jsonRpcMemory = ReadOnlyMemory<byte>.Empty;
        if (e.RequestInfo is IJsonRpcRequestInfo requestInfo)
        {
            jsonRpcMemory = requestInfo.GetJsonRpcMemory();
        }
        else if (e.RequestInfo is JsonPackage jsonPackage)
        {
            jsonRpcMemory = jsonPackage.Data;
        }
        else if (!e.Memory.IsEmpty)
        {
            jsonRpcMemory = e.Memory;
        }

        if (jsonRpcMemory.IsEmpty)
        {
            return;
        }
        var callContext = new TcpJsonRpcCallContext(this);
        await this.m_jsonRpcActor.InputReceiveAsync(jsonRpcMemory, callContext).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}