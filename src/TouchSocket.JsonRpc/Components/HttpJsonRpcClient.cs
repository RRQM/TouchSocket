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

using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc;

/// <summary>
/// 表示一个HTTP JSON-RPC客户端。
/// </summary>
public class HttpJsonRpcClient : HttpClientBase, IHttpJsonRpcClient
{
    private readonly JsonRpcActor m_jsonRpcActor;

    /// <summary>
    /// 初始化 <see cref="HttpJsonRpcClient"/> 类的新实例。
    /// </summary>
    public HttpJsonRpcClient()
    {
        this.SerializerConverter.Add(new JsonStringToClassSerializerFormatter<JsonRpcActor>());
        this.m_jsonRpcActor = new JsonRpcActor()
        {
            SendAction = this.SendAction,
            SerializerConverter = this.SerializerConverter
        };
    }

    /// <summary>
    /// 获取序列化转换器。
    /// </summary>
    public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter { get; } = new TouchSocketSerializerConverter<string, JsonRpcActor>();

    #region JsonRpcActor

    private async Task SendAction(ReadOnlyMemory<byte> memory)
    {
        var request = new HttpRequest();
        request.Method = HttpMethod.Post;
        request.URL = (this.RemoteIPHost.PathAndQuery);
        request.SetContent(memory);

        using (var responseResult = await base.ProtectedRequestAsync(request).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            var response = responseResult.Response;

            if (response.IsSuccess())
            {
                await this.m_jsonRpcActor.InputReceiveAsync(await response.GetContentAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext), default);
            }
        }
    }

    #endregion JsonRpcActor

    /// <summary>
    /// 异步连接到服务器。
    /// </summary>
    /// <param name="millisecondsTimeout">超时时间（毫秒）。</param>
    /// <param name="token">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        return this.TcpConnectAsync(millisecondsTimeout, token);
    }

    /// <summary>
    /// 异步调用远程方法。
    /// </summary>
    /// <param name="invokeKey">调用键。</param>
    /// <param name="returnType">返回类型。</param>
    /// <param name="invokeOption">调用选项。</param>
    /// <param name="parameters">参数。</param>
    /// <returns>表示异步操作的任务，包含调用结果。</returns>
    public Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
    {
        return this.m_jsonRpcActor.InvokeAsync(invokeKey, returnType, invokeOption, parameters);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            m_jsonRpcActor.SafeDispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 加载配置。
    /// </summary>
    /// <param name="config">配置。</param>
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);
        this.m_jsonRpcActor.Logger = this.Logger;
    }
}