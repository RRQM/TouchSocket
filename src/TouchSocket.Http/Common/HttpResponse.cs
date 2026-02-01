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
/// Http响应
/// </summary>
public abstract class HttpResponse : HttpBase
{
    #region 字段

    private readonly HttpClientBase m_httpClientBase;
    private readonly HttpSessionClient m_httpSessionClient;
    private readonly bool m_isServer;
    private bool m_canWrite;

    private bool m_sentHeader;
    private long m_sentLength;
    #endregion 字段

    /// <summary>
    /// Http响应
    /// </summary>
    /// <param name="httpClientBase"></param>
    internal HttpResponse(HttpClientBase httpClientBase)
    {
        this.m_isServer = false;
        this.m_canWrite = false;
        this.m_httpClientBase = httpClientBase;
    }

    internal HttpResponse(HttpRequest request, HttpSessionClient httpSessionClient)
    {
        this.m_canWrite = true;
        this.m_isServer = true;
        this.m_httpSessionClient = httpSessionClient;
        this.ProtocolVersion = request.ProtocolVersion;
        this.Protocols = request.Protocols;
    }

    #region 属性

    /// <summary>
    /// 是否代理权限验证。
    /// </summary>
    public bool IsProxyAuthenticationRequired => this.StatusCode == 407;

    /// <summary>
    /// 是否重定向
    /// </summary>
    public bool IsRedirect => this.StatusCode == 301 || this.StatusCode == 302;

    /// <inheritdoc/>
    public override bool IsServer => this.m_isServer;

    /// <summary>
    /// 是否已经响应数据。
    /// </summary>
    public bool Responsed { get; private set; }

    /// <summary>
    /// 状态码，默认200
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// 状态消息，默认Success
    /// </summary>
    public string StatusMessage { get; set; } = "Success";

    #endregion 属性

    /// <summary>
    /// 构建数据并回应。
    /// <para>该方法仅在具有Client实例时有效。</para>
    /// </summary>
    public async Task AnswerAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfResponsed();

        var transport = this.GetITransport();
        var content = this.Content;
        if (content == null)
        {
            //issue：https://github.com/RRQM/TouchSocket/issues/117
            this.Headers.TryAdd(HttpHeaders.ContentLength, "0");
            var writer = new PipeBytesWriter(transport.Writer);
            this.BuildHeader(ref writer);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            content.InternalBuildingHeader(this.Headers);
            
            var writer = new PipeBytesWriter(transport.Writer);
            this.BuildHeader(ref writer);

            var result = content.InternalBuildingContent(ref writer);

            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            if (!result)
            {
                await content.InternalWriteContent(transport.Writer, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }

        // 100状态码不表示响应结束。后续可以继续发送响应内容。
        if (this.StatusCode != 100)
        {
            this.Responsed = true;
        }

    }

    /// <summary>
    /// 当传输模式是Chunk时，用于结束传输。
    /// </summary>
    public async Task CompleteChunkAsync(CancellationToken cancellationToken = default)
    {
        if (!this.m_canWrite)
        {
            return;
        }

        this.ThrowIfResponsed();
        this.m_canWrite = false;
        if (this.IsChunk)
        {
            var transport = this.GetITransport();
            var writer = new PipeBytesWriter(transport.Writer);
            TouchSocketHttpUtility.AppendHex(ref writer, 0);
            TouchSocketHttpUtility.AppendRn(ref writer);
            TouchSocketHttpUtility.AppendRn(ref writer);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.Responsed = true;
        }
    }

    #region Write

    /// <summary>
    /// 异步写入指定的只读内存数据。
    /// </summary>
    /// <param name="memory">要写入的只读内存数据。</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>一个任务，表示异步写入操作。</returns>
    public async Task WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        this.ThrowIfResponsed();

        var transport = this.GetITransport();
        var writer = new PipeBytesWriter(transport.Writer);

        if (!this.m_sentHeader)
        {
            this.BuildHeader(ref writer);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_sentHeader = true;
        }

        var count = memory.Length;

        if (this.IsChunk)
        {
            TouchSocketHttpUtility.AppendHex(ref writer, count);
            TouchSocketHttpUtility.AppendRn(ref writer);
            writer.Write(memory.Span);
            TouchSocketHttpUtility.AppendRn(ref writer);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_sentLength += count;
        }
        else
        {
            //issue：https://github.com/RRQM/TouchSocket/issues/118
            if (this.m_sentLength + count > this.ContentLength)
            {
                ThrowHelper.ThrowInvalidOperationException($"尝试写入的数据长度（{count}）超过了剩余可写入的内容长度（{this.ContentLength - this.m_sentLength}）。");
            }

            writer.Write(memory.Span);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_sentLength += count;
            if (this.m_sentLength == this.ContentLength)
            {
                this.m_canWrite = false;
                this.Responsed = true;
            }
        }
    }

    #endregion Write

    protected internal override void Reset()
    {
        if (this.m_isServer)
        {
            this.m_canWrite = true;
        }
        else
        {
            this.m_canWrite = false;
        }
        base.Reset();
        this.m_sentHeader = false;
        this.m_sentLength = 0;
        this.Responsed = false;
        this.StatusCode = 200;
        this.StatusMessage = "Success";
        this.Content = default;
    }

    /// <inheritdoc/>
    protected override void ReadRequestLine(ReadOnlySpan<byte> responseLineSpan)
    {
        var index = 0;

        // 1. 解析 (HTTP/1.1)
        var firstSpace = TouchSocketHttpUtility.FindNextWhitespace(responseLineSpan, index);
        if (firstSpace == -1)
        {
            return;
        }

        this.ParseProtocol(responseLineSpan.Slice(0, firstSpace));
        index = firstSpace + 1;

        // 2. 解析 Status Code
        var secondSpace = TouchSocketHttpUtility.FindNextWhitespace(responseLineSpan, index);
        if (secondSpace == -1)
        {
            this.ParseStatusCode(responseLineSpan.Slice(index));
            return;
        }

        this.ParseStatusCode(responseLineSpan.Slice(index, secondSpace - index));
        index = secondSpace + 1;

        // 3. 解析 Status Message (可能包含空格)
        if (index < responseLineSpan.Length)
        {
            this.StatusMessage = responseLineSpan.Slice(index).ToString(Encoding.UTF8);
        }
    }

    private static bool TryParseStatusCode(ReadOnlySpan<byte> span, out int code)
    {
        code = 0;
        if (span.IsEmpty)
        {
            return false;
        }

        // 处理符号
        var sign = 1;
        var index = 0;
        if (span[0] == (byte)'-' || span[0] == (byte)'+')
        {
            sign = span[0] == (byte)'-' ? -1 : 1;
            index++;
            // 符号后必须至少有一个数字
            if (index >= span.Length)
            {
                return false;
            }
        }

        // 检查剩余字符是否均为数字
        for (var i = index; i < span.Length; i++)
        {
            if (span[i] < (byte)'0' || span[i] > (byte)'9')
            {
                return false;
            }
        }

        // 用 long 累加避免中间溢出
        long result = 0;
        for (var i = index; i < span.Length; i++)
        {
            result = result * 10 + (span[i] - '0');
            // 提前检查是否已超出 int 范围
            if (sign == 1 && result > int.MaxValue)
            {
                return false;
            }
            if (sign == -1 && result > -(long)int.MinValue)
            {
                return false;
            }
        }

        // 应用符号并转换到 int
        code = (int)(sign * result);
        return true;
    }

    private void BuildHeader<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        var versionBytes = TouchSocketHttpUtility.GetHttpVersionBytes(this.ProtocolVersion);
        if (!versionBytes.IsEmpty)
        {
            writer.Write(versionBytes);
        }
        else
        {
            TouchSocketHttpUtility.AppendHTTP(ref writer);
            TouchSocketHttpUtility.AppendSlash(ref writer);
            TouchSocketHttpUtility.AppendUtf8String(ref writer, this.ProtocolVersion);
        }
        
        TouchSocketHttpUtility.AppendSpace(ref writer);
        
        var statusCodeBytes = TouchSocketHttpUtility.GetStatusCodeBytes(this.StatusCode);
        writer.Write(statusCodeBytes);
        
        TouchSocketHttpUtility.AppendSpace(ref writer);
        TouchSocketHttpUtility.AppendUtf8String(ref writer, this.StatusMessage);
        TouchSocketHttpUtility.AppendRn(ref writer);

        foreach (var header in this.Headers)
        {
            TouchSocketHttpUtility.AppendUtf8String(ref writer, header.Key);
            TouchSocketHttpUtility.AppendColon(ref writer);
            TouchSocketHttpUtility.AppendSpace(ref writer);
            TouchSocketHttpUtility.AppendUtf8String(ref writer, header.Value);
            TouchSocketHttpUtility.AppendRn(ref writer);
        }

        TouchSocketHttpUtility.AppendRn(ref writer);
    }

    private ITransport GetITransport()
    {
        return this.m_isServer ? this.m_httpSessionClient.InternalTransport : this.m_httpClientBase.InternalTransport;
    }

    private void ParseProtocol(ReadOnlySpan<byte> protocolSpan)
    {
        var slashIndex = protocolSpan.IndexOf((byte)'/');
        if (slashIndex > 0 && slashIndex < protocolSpan.Length - 1)
        {
            this.Protocols = new Protocol(protocolSpan.Slice(0, slashIndex).ToString(Encoding.UTF8));
            this.ProtocolVersion = protocolSpan.Slice(slashIndex + 1).ToString(Encoding.UTF8);
        }
    }

    private void ParseStatusCode(ReadOnlySpan<byte> codeSpan)
    {
        if (TryParseStatusCode(codeSpan, out var code))
        {
            this.StatusCode = code;
        }
    }

    private void ThrowIfResponsed()
    {
        if (this.Responsed)
        {
            throw new Exception("该对象已被响应。");
        }
    }
}