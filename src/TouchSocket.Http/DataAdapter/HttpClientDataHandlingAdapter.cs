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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// Http客户端数据处理适配器
/// </summary>
internal sealed class HttpClientDataHandlingAdapter : SingleStreamDataHandlingAdapter
{
    private readonly AsyncManualResetEvent m_resetEvent = new AsyncManualResetEvent();
    private HttpResponse m_httpResponse;
    private HttpResponse m_httpResponseRoot;
    private long m_surLen;
    private Task m_task;

    // 优化：预分配的状态变量，避免重复计算
    private bool m_isProcessingChunk;

    private bool m_isWaitingForContent;

    /// <inheritdoc/>
    public SingleStreamDataHandlingAdapter WarpAdapter { get; set; }

    /// <inheritdoc/>
    public override void OnLoaded(object owner)
    {
        if (owner is not HttpClientBase clientBase)
        {
            throw new Exception($"此适配器必须适用于{nameof(HttpClientBase)}");
        }
        this.m_httpResponseRoot = new HttpResponse(clientBase);
        base.OnLoaded(owner);
    }

    public void SetCompleteLock()
    {
        this.m_resetEvent.Set();
    }

    /// <inheritdoc/>
    protected override async Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            // 优化：提前检查包装适配器，减少重复访问
            var adapter = this.WarpAdapter;
            if (adapter != null)
            {
                await adapter.ReceivedInputAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return;
            }

            // 优化：使用状态标记减少条件判断
            if (this.m_httpResponse == null)
            {
                if (!await this.ProcessNewResponseAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    return;
                }
            }
            else
            {
                if (!await this.ProcessExistingResponseAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    return;
                }
            }
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_resetEvent.Set();
            // 优化：重置状态标记
            this.m_isProcessingChunk = false;
            this.m_isWaitingForContent = false;
        }
        base.SafetyDispose(disposing);
    }

    /// <summary>
    /// 处理新的HTTP响应
    /// </summary>
    private async ValueTask<bool> ProcessNewResponseAsync<TReader>(TReader reader)
        where TReader : class, IBytesReader
    {
        this.m_httpResponseRoot.ResetHttp();
        this.m_httpResponse = this.m_httpResponseRoot;

        if (!this.m_httpResponse.ParsingHeader(ref reader))
        {
            this.m_httpResponse = null;

            // 优化：等待之前的任务完成
            if (this.m_task != null)
            {
                await this.m_task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                await this.m_resetEvent.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                this.m_task = null;
            }
            return false;
        }

        // 优化：预先计算状态，减少后续判断
        var contentLength = this.m_httpResponse.ContentLength;
        this.m_isProcessingChunk = this.m_httpResponse.IsChunk;
        this.m_isWaitingForContent = this.m_isProcessingChunk || contentLength > reader.BytesRemaining;

        if (this.m_isWaitingForContent)
        {
            this.m_surLen = contentLength;
            this.m_task = this.GoReceivedAsync(ReadOnlyMemory<byte>.Empty, this.m_httpResponse);
        }
        else
        {
            // 优化：直接读取内容，避免数组分配
            await this.ProcessCompleteContentAsync(reader, contentLength).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        return true;
    }

    /// <summary>
    /// 处理现有的HTTP响应
    /// </summary>
    private async ValueTask<bool> ProcessExistingResponseAsync<TReader>(TReader reader)
        where TReader : class, IBytesReader
    {
        if (this.m_isProcessingChunk)
        {
            return await this.ProcessChunkedContentAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else if (this.m_surLen > 0)
        {
            return await this.ProcessRemainingContentAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            await this.CompleteResponseAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return true;
        }
    }

    /// <summary>
    /// 处理完整内容（内容长度已知且数据充足）
    /// </summary>
    private async Task ProcessCompleteContentAsync<TReader>(TReader reader, long contentLength)
        where TReader : class, IBytesReader
    {
        var len = (int)contentLength;
        var content = reader.GetMemory(len);
        reader.Advance(len);
        this.m_httpResponse.InternalSetContent(content);

        this.m_httpResponse.CompleteInput();
        await this.GoReceivedAsync(ReadOnlyMemory<byte>.Empty, this.m_httpResponse).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        await this.m_resetEvent.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_httpResponse = null;
        this.m_isProcessingChunk = false;
        this.m_isWaitingForContent = false;
    }

    /// <summary>
    /// 处理分块传输内容
    /// </summary>
    private async ValueTask<bool> ProcessChunkedContentAsync<TReader>(TReader reader)
        where TReader : class, IBytesReader
    {
        var result = await this.ReadChunk(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        switch (result)
        {
            case FilterResult.Cache:
                return false;

            case FilterResult.Success:
                this.m_httpResponse.CompleteInput();
                await this.CompleteResponseAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                break;

            case FilterResult.GoOn:
            default:
                break;
        }

        return true;
    }

    /// <summary>
    /// 处理剩余内容
    /// </summary>
    private async ValueTask<bool> ProcessRemainingContentAsync<TReader>(TReader reader)
        where TReader : class, IBytesReader
    {
        if (reader.BytesRemaining <= 0)
        {
            return true;
        }

        var len = (int)Math.Min(this.m_surLen, reader.BytesRemaining);
        await this.m_httpResponse.InternalInputAsync(reader.GetMemory(len)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_surLen -= len;
        reader.Advance(len);

        if (this.m_surLen == 0)
        {
            this.m_httpResponse.CompleteInput();
            await this.CompleteResponseAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        return true;
    }

    /// <summary>
    /// 完成响应处理
    /// </summary>
    private async Task CompleteResponseAsync()
    {
        this.m_httpResponse = null;
        this.m_isProcessingChunk = false;
        this.m_isWaitingForContent = false;

        if (this.m_task != null)
        {
            await this.m_task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_task = null;
        }

        await this.m_resetEvent.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async ValueTask<FilterResult> ReadChunk<TReader>(TReader reader)
        where TReader : class, IBytesReader
    {
        var position = reader.BytesRead;

        // 优化：使用ReadOnlySpan直接查找，避免重复的IndexOf调用
        var crlfIndex = (int)reader.Sequence.IndexOf(TouchSocketHttpUtility.CRLF);
        if (crlfIndex <= 0)
        {
            return FilterResult.Cache;
        }

        var headerLength = crlfIndex;

        // 优化：直接使用Span进行十六进制解析，避免字符串分配
        var hexSpan = reader.GetSpan(headerLength);
        if (!TryParseHexLength(hexSpan, out var count))
        {
            reader.Advance(1); // 跳过无效字符
            return FilterResult.GoOn;
        }

        reader.Advance(headerLength + 2); // 跳过长度和CRLF

        if (count > 0)
        {
            if (count > reader.BytesRemaining)
            {
                reader.BytesRead = position;
                return FilterResult.Cache;
            }

            await this.m_httpResponse.InternalInputAsync(reader.GetMemory(count)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            reader.Advance(count + 2); // 跳过内容和结尾CRLF
            return FilterResult.GoOn;
        }
        else
        {
            reader.Advance(2); // 跳过结尾CRLF
            return FilterResult.Success;
        }
    }

    /// <summary>
    /// 优化：直接从字节解析十六进制，避免字符串分配
    /// </summary>
    private static bool TryParseHexLength(ReadOnlySpan<byte> hexBytes, out int result)
    {
        result = 0;

        if (hexBytes.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < hexBytes.Length; i++)
        {
            var b = hexBytes[i];
            int digit;

            if (b >= '0' && b <= '9')
            {
                digit = b - '0';
            }
            else if (b >= 'A' && b <= 'F')
            {
                digit = b - 'A' + 10;
            }
            else if (b >= 'a' && b <= 'f')
            {
                digit = b - 'a' + 10;
            }
            else
            {
                return false; // 无效字符
            }

            // 检查溢出
            if (result > (int.MaxValue - digit) / 16)
            {
                return false;
            }

            result = result * 16 + digit;
        }

        return true;
    }
}