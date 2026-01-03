// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Http;

/// <summary>
/// 服务器发送事件(SSE)读取器
/// </summary>
internal class InternalSseReader: SseReader
{
    private readonly HttpResponse m_response;
    private readonly StringBuilder m_dataBuilder = new StringBuilder();
    private string m_currentEvent;
    private string m_currentId;
    private TimeSpan? m_currentRetry;
    private string m_currentComment;

    /// <summary>
    /// 初始化SseReader
    /// </summary>
    /// <param name="response">HTTP响应对象</param>
    public InternalSseReader(HttpResponse response)
    {
        this.m_response = response;
    }

    /// <summary>
    /// HTTP响应对象
    /// </summary>
    public override HttpResponse Response => this.m_response;

    
    public override async Task<SseMessage> ReadAsync(CancellationToken cancellationToken = default)
    {
        this.m_dataBuilder.Clear();
        this.m_currentEvent = null;
        this.m_currentComment = null;
        var hasData = false;

        while (true)
        {
            using (var blockResult = await this.m_response.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                if (blockResult.Memory.IsEmpty && blockResult.IsCompleted)
                {
                    if (hasData)
                    {
                        return this.CreateMessage();
                    }
                    return null;
                }

                var memory = blockResult.Memory;
                var span = memory.Span;

                var lineStart = 0;
                for (var i = 0; i < span.Length; i++)
                {
                    if (span[i] == '\n')
                    {
                        var lineEnd = i;
                        if (lineEnd > lineStart && span[lineEnd - 1] == '\r')
                        {
                            lineEnd--;
                        }

                        var lineSpan = span.Slice(lineStart, lineEnd - lineStart);
                        if (lineSpan.IsEmpty)
                        {
                            if (hasData)
                            {
                                return this.CreateMessage();
                            }
                        }
                        else
                        {
                            this.ParseLine(lineSpan);
                            hasData = true;
                        }

                        lineStart = i + 1;
                    }
                }

                if (lineStart < span.Length)
                {
                    var remaining = span.Slice(lineStart);
                    if (remaining.IndexOf((byte)'\n') == -1)
                    {
                        continue;
                    }
                }

                if (blockResult.IsCompleted)
                {
                    if (hasData)
                    {
                        return this.CreateMessage();
                    }
                    return null;
                }
            }
        }
    }

   
    public override async IAsyncEnumerable<SseMessage> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var message = await this.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (message == null)
            {
                break;
            }
            yield return message;
        }
    }

    private void ParseLine(ReadOnlySpan<byte> lineSpan)
    {
        if (lineSpan.IsEmpty)
        {
            return;
        }

        if (lineSpan[0] == (byte)':')
        {
            var commentStart = 1;
            if (lineSpan.Length > 1 && lineSpan[1] == (byte)' ')
            {
                commentStart = 2;
            }
            this.m_currentComment = this.GetString(lineSpan.Slice(commentStart));
            return;
        }

        var colonIndex = lineSpan.IndexOf((byte)':');
        if (colonIndex == -1)
        {
            var fieldName = this.GetString(lineSpan);
            this.ProcessField(fieldName, string.Empty);
            return;
        }

        var fieldNameSpan = lineSpan.Slice(0, colonIndex);
        var fieldName2 = this.GetString(fieldNameSpan);

        var valueStart = colonIndex + 1;
        if (valueStart < lineSpan.Length && lineSpan[valueStart] == (byte)' ')
        {
            valueStart++;
        }

        var valueSpan = lineSpan.Slice(valueStart);
        var value = this.GetString(valueSpan);

        this.ProcessField(fieldName2, value);
    }

    private void ProcessField(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "event":
                this.m_currentEvent = value;
                break;
            case "data":
                if (this.m_dataBuilder.Length > 0)
                {
                    this.m_dataBuilder.Append('\n');
                }
                this.m_dataBuilder.Append(value);
                break;
            case "id":
                this.m_currentId = value;
                break;
            case "retry":
                if (int.TryParse(value, out var retry))
                {
                    this.m_currentRetry = TimeSpan.FromMilliseconds(retry);
                }
                break;
        }
    }

    private SseMessage CreateMessage()
    {
        var message = new SseMessage
        {
            EventType = this.m_currentEvent,
            Data = this.m_dataBuilder.ToString(),
            EventId = this.m_currentId,
            ReconnectionInterval = this.m_currentRetry,
            Comment = this.m_currentComment
        };

        this.m_dataBuilder.Clear();
        this.m_currentEvent = null;
        this.m_currentComment = null;

        return message;
    }

    private string GetString(ReadOnlySpan<byte> span)
    {
        return span.ToUtf8String();
    }
}
