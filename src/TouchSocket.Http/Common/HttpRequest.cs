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

using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// HTTP请求定义
/// </summary>
public class HttpRequest : HttpBase
{
    private readonly bool m_isServer;
    private readonly InternalHttpParams m_query = new InternalHttpParams();
    private string m_relativeURL = "/";
    private string m_url = "/";

    /// <summary>
    /// HttpRequest类的构造函数。
    /// </summary>
    /// <remarks>
    /// 初始化HttpRequest对象的基本属性。
    /// </remarks>
    public HttpRequest()
    {
        this.m_isServer = false;
    }

    internal HttpRequest(HttpSessionClient httpSessionClient)
    {
        this.m_isServer = true;
    }

    /// <inheritdoc/>
    public override bool IsServer => this.m_isServer;

    /// <summary>
    /// 保持连接。
    /// <para>
    /// 一般的，当是http1.1时，如果没有显式的Connection: close，即返回<see langword="true"/>。当是http1.0时，如果没有显式的Connection: Keep-Alive，即返回<see langword="false"/>。
    /// </para>
    /// </summary>
    public bool KeepAlive
    {
        get
        {
            var keepAlive = this.Headers.Get(HttpHeaders.Connection);
            return this.ProtocolVersion == "1.0"
                ? !keepAlive.IsEmpty && keepAlive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase)
                : keepAlive.IsEmpty || keepAlive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase);
        }
        set
        {
            if (this.ProtocolVersion == "1.0")
            {
                if (value)
                {
                    this.Headers.Add(HttpHeaders.Connection, "Keep-Alive");
                }
                else
                {
                    this.Headers.Add(HttpHeaders.Connection, "close");
                }
            }
            else
            {
                if (!value)
                {
                    this.Headers.Add(HttpHeaders.Connection, "close");
                }
            }
        }
    }

    /// <summary>
    /// HTTP请求方式。
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// url参数
    /// </summary>
    public IHttpParams Query => this.m_query;

    /// <summary>
    /// 相对路径（不含参数）
    /// </summary>
    public string RelativeURL => this.m_relativeURL;

    /// <summary>
    /// Url全地址，包含参数
    /// </summary>
    public string URL
    {
        get => this.m_url;
        set
        {
            this.m_url = value;
            this.ParseUrl(this.m_url.AsSpan());
        }
    }

    /// <inheritdoc/>
    public override ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override ValueTask<HttpReadOnlyMemoryBlockResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 设置代理Host
    /// </summary>
    /// <param name="host">代理服务器的地址</param>
    /// <returns>返回当前HttpRequest实例，以支持链式调用</returns>
    public HttpRequest SetProxyHost(string host)
    {
        // 将URL属性设置为指定的代理服务器地址
        this.URL = host;
        // 返回当前实例，以支持链式调用
        return this;
    }

    internal void BuildHeader<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        WriterExtension.WriteNormalString(ref writer, this.Method.ToString(), Encoding.UTF8);//Get
        TouchSocketHttpUtility.AppendSpace(ref writer);//空格
        TouchSocketHttpUtility.AppendUtf8String(ref writer, this.RelativeURL);
        if (this.m_query.Count > 0)
        {
            TouchSocketHttpUtility.AppendQuestionMark(ref writer);
            var i = 0;
            foreach (var item in this.m_query.Keys)
            {
                WriterExtension.WriteNormalString(ref writer, item, Encoding.UTF8);
                TouchSocketHttpUtility.AppendEqual(ref writer);
                var value = this.m_query[item];
                if (!value.IsEmpty)
                {
                    WriterExtension.WriteNormalString(ref writer, Uri.EscapeDataString(value), Encoding.UTF8);
                }

                if (++i < this.m_query.Count)
                {
                    TouchSocketHttpUtility.AppendAnd(ref writer);
                }
            }
        }
        TouchSocketHttpUtility.AppendSpace(ref writer);//空格
        TouchSocketHttpUtility.AppendHTTP(ref writer);//HTTP
        TouchSocketHttpUtility.AppendSlash(ref writer);//斜杠
        WriterExtension.WriteNormalString(ref writer, this.ProtocolVersion, Encoding.UTF8);//1.1
        TouchSocketHttpUtility.AppendRn(ref writer);//换行

        foreach (var headerKey in this.Headers.Keys)
        {
            WriterExtension.WriteNormalString(ref writer, headerKey, Encoding.UTF8);//key
            TouchSocketHttpUtility.AppendColon(ref writer);//冒号
            TouchSocketHttpUtility.AppendSpace(ref writer);//空格
            WriterExtension.WriteNormalString(ref writer, this.Headers[headerKey], Encoding.UTF8);//value
            TouchSocketHttpUtility.AppendRn(ref writer);//换行
        }

        TouchSocketHttpUtility.AppendRn(ref writer);
    }

    /// <inheritdoc/>
    protected internal override void Reset()
    {
        base.Reset();

        this.m_relativeURL = "/";
        this.m_url = "/";

        this.m_query.Clear();
    }

    /// <inheritdoc/>
    protected override void ReadRequestLine(ReadOnlySpan<byte> requestLineSpan)
    {
        var start = 0;

        var methodEnd = TouchSocketHttpUtility.FindNextWhitespace(requestLineSpan, start);
        if (methodEnd == -1)
        {
            throw new Exception("Invalid HTTP request line: " + requestLineSpan.ToString(Encoding.UTF8));
        }

        var methodSpan = requestLineSpan.Slice(start, methodEnd - start);
        this.Method = ParseHttpMethodFast(methodSpan);
        start = TouchSocketHttpUtility.SkipSpaces(requestLineSpan, methodEnd + 1);

        var urlEnd = TouchSocketHttpUtility.FindNextWhitespace(requestLineSpan, start);
        if (urlEnd == -1)
        {
            this.URL = TouchSocketHttpUtility.UnescapeDataString(requestLineSpan.Slice(start));
            return;
        }

        this.URL = TouchSocketHttpUtility.UnescapeDataString(requestLineSpan.Slice(start, urlEnd - start));
        start = TouchSocketHttpUtility.SkipSpaces(requestLineSpan, urlEnd + 1);

        var protocolSpan = requestLineSpan.Slice(start);
        var slashIndex = protocolSpan.IndexOf((byte)'/');
        if (slashIndex > 0 && slashIndex < protocolSpan.Length - 1)
        {
            this.Protocols = new Protocol(protocolSpan.Slice(0, slashIndex).ToString(Encoding.UTF8));
            this.ProtocolVersion = protocolSpan.Slice(slashIndex + 1).ToString(Encoding.UTF8);
        }
    }

    private static HttpMethod ParseHttpMethodFast(ReadOnlySpan<byte> span)
    {
        switch (span.Length)
        {
            case 3:
                if (EqualsIgnoreCaseAscii(span, "GET")) return HttpMethod.Get;
                if (EqualsIgnoreCaseAscii(span, "PUT")) return HttpMethod.Put;
                break;
            case 4:
                if (EqualsIgnoreCaseAscii(span, "POST")) return HttpMethod.Post;
                break;
            case 6:
                if (EqualsIgnoreCaseAscii(span, "DELETE")) return HttpMethod.Delete;
                break;
            case 7:
                if (EqualsIgnoreCaseAscii(span, "CONNECT")) return HttpMethod.Connect;
                break;
        }
        return new HttpMethod(span.ToString(Encoding.UTF8));
    }

    private static bool EqualsIgnoreCaseAscii(ReadOnlySpan<byte> span, string token)
    {
        if (span.Length != token.Length)
        {
            return false;
        }
        for (var i = 0; i < span.Length; i++)
        {
            var b = span[i];
            if (b >= (byte)'a' && b <= (byte)'z')
            {
                b = (byte)(b - 32);
            }
            var c = (byte)token[i];
            if (c >= (byte)'a' && c <= (byte)'z')
            {
                c = (byte)(c - 32);
            }
            if (b != c)
            {
                return false;
            }
        }
        return true;
    }

    private static void GetParameters(ReadOnlySpan<char> querySpan, InternalHttpParams parameters)
    {
        while (!querySpan.IsEmpty)
        {
            // 查找下一个键值对
            var ampIndex = querySpan.IndexOf('&');
            var kvSpan = ampIndex >= 0 ? querySpan.Slice(0, ampIndex) : querySpan;

            // 处理有效的非空对
            if (!kvSpan.IsEmpty)
            {
                TouchSocketHttpUtility.ProcessKeyValuePair(kvSpan, parameters);
            }

            // 如果没有更多配对，则退出循环
            if (ampIndex < 0)
            {
                break;
            }

            // 移动到下一对（跳过“&”）
            querySpan = querySpan.Slice(ampIndex + 1);
        }
    }

    private void ParseUrl(ReadOnlySpan<char> url)
    {
        var queryIndex = url.IndexOf('?');
        if (queryIndex >= 0)
        {
            // 提取相对URL和查询部分
            this.m_relativeURL = url.Slice(0, queryIndex).ToString();
            var querySpan = url.Slice(queryIndex + 1);

            // 清除现有查询参数并解析新参数
            this.m_query.Clear();
            GetParameters(querySpan, this.m_query);
        }
        else
        {
            //清除所有现有参数
            this.m_relativeURL = url.ToString();
            this.m_query.Clear();
        }
    }

    #region SSE支持（客户端读取）

    /// <summary>
    /// 开始读取SSE事件流。
    /// </summary>
    /// <param name="onMessage">消息回调函数。</param>
    /// <param name="onComment">注释回调函数（可选）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>读取任务。</returns>
    /// <exception cref="ArgumentNullException">当onMessage为null时抛出。</exception>
    /// <exception cref="InvalidOperationException">当响应不是有效的SSE流时抛出。</exception>
    public async Task ReadSSEAsync(Func<SseMessage, Task> onMessage, Func<string, Task> onComment = null, CancellationToken cancellationToken = default)
    {
        if (onMessage == null)
            throw new ArgumentNullException(nameof(onMessage));

        if (this.ContentStatus == ContentCompletionStatus.ContentCompleted)
            throw new InvalidOperationException("内容已读取完成，无法读取SSE事件流");

        // 验证是否为SSE响应
        if (!this.IsSseResponse())
        {
            throw new InvalidOperationException("响应不是有效的SSE流（Content-Type应为text/event-stream）");
        }

        using var parser = new SseParser();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var blockResult = await ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (blockResult.IsCompleted)
                {
                    // 处理最后的消息
                    var finalMessages = parser.ProcessBytes(Array.Empty<byte>());
                    await this.ProcessSseEventsAsync(finalMessages, onMessage, onComment, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    break;
                }

                // 处理接收到的数据
                var messages = parser.ProcessBytes(blockResult.Memory);
                await this.ProcessSseEventsAsync(messages, onMessage, onComment, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 正常的取消操作
        }
        catch (Exception ex)
        {
            // 记录错误
            System.Diagnostics.Debug.WriteLine($"读取SSE时出错: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 开始读取SSE事件流，支持按事件类型注册处理器。
    /// </summary>
    /// <param name="eventHandlers">事件处理器字典，key为事件类型，value为处理函数。</param>
    /// <param name="onComment">注释回调函数（可选）。</param>
    /// <param name="onDefaultMessage">默认消息处理器，用于处理没有指定处理器的事件（可选）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>读取任务。</returns>
    public async Task ReadSSEAsync(Dictionary<string, Func<SseMessage, Task>> eventHandlers, Func<string, Task> onComment = null, Func<SseMessage, Task> onDefaultMessage = null, CancellationToken cancellationToken = default)
    {
        if (eventHandlers == null)
            throw new ArgumentNullException(nameof(eventHandlers));

        if (this.ContentStatus == ContentCompletionStatus.ContentCompleted)
            throw new InvalidOperationException("内容已读取完成，无法读取SSE事件流");

        // 验证是否为SSE响应
        if (!this.IsSseResponse())
        {
            throw new InvalidOperationException("响应不是有效的SSE流（Content-Type应为text/event-stream）");
        }

        using var parser = new SseParser();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var blockResult = await this.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (blockResult.IsCompleted)
                {
                    // 处理最后的消息
                    var finalMessages = parser.ProcessBytes(Array.Empty<byte>());
                    await this.ProcessSseEventsWithHandlersAsync(finalMessages, eventHandlers, onComment, onDefaultMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    break;
                }

                // 处理接收到的数据
                var messages = parser.ProcessBytes(blockResult.Memory);
                await this.ProcessSseEventsWithHandlersAsync(messages, eventHandlers, onComment, onDefaultMessage, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 正常的取消操作
        }
        catch (Exception ex)
        {
            // 记录错误
            System.Diagnostics.Debug.WriteLine($"读取SSE时出错: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 处理SSE事件。
    /// </summary>
    private async Task ProcessSseEventsAsync(List<SseMessage> events, Func<SseMessage, Task> onMessage, Func<string, Task> onComment, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (evt.IsComment)
                {
                    if (onComment != null)
                    {
                        await onComment(evt.Data).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
                else
                {
                    await onMessage(evt).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
            catch (Exception ex)
            {
                // 记录回调中的异常，但不中断处理
                System.Diagnostics.Debug.WriteLine($"处理SSE事件回调时出错: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 处理SSE事件，使用事件处理器字典。
    /// </summary>
    private async Task ProcessSseEventsWithHandlersAsync(List<SseMessage> events, Dictionary<string, Func<SseMessage, Task>> eventHandlers, Func<string, Task> onComment, Func<SseMessage, Task> onDefaultMessage, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (evt.IsComment)
                {
                    if (onComment != null)
                    {
                        await onComment(evt.Data).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
                else
                {
                    var eventType = evt.GetEventTypeOrDefault();

                    // 查找对应的处理器
                    if (eventHandlers.TryGetValue(eventType, out var handler))
                    {
                        await handler(evt).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    else if (onDefaultMessage != null)
                    {
                        // 使用默认处理器
                        await onDefaultMessage(evt).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录回调中的异常，但不中断处理
                System.Diagnostics.Debug.WriteLine($"处理SSE事件回调时出错: {ex.Message}");
            }
        }
    }

    #endregion
}