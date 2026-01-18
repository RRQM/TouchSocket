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

using System.Buffers;

namespace TouchSocket.Http;

/// <summary>
/// 服务器发送事件(SSE)读取器
/// </summary>
internal class InternalSseReader : SseReader
{
    private readonly StringBuilder m_dataBuilder = new StringBuilder();
    private readonly HttpResponse m_response;
    private readonly SegmentedPipe m_lineBuffer = new SegmentedPipe(256);
    private string m_currentComment;
    private string m_currentEvent;
    private string m_lastEventId;
    private TimeSpan? m_retryInterval;
    private bool m_hasData;

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

    public override async IAsyncEnumerable<SseMessage> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        this.ResetMessageState();
        while (true)
        {
            var message = await this.ReadAsync(cancellationToken)
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (message == null)
            {
                break;
            }
            yield return message;
        }
    }

    public override async Task<SseMessage> ReadAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var processResult = this.ProcessBuffer();
            if (processResult != null)
            {
                this.ResetMessageState();
                return processResult;
            }

            using (var blockResult = await this.m_response.ReadAsync(cancellationToken)
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                if (!blockResult.Memory.IsEmpty)
                {
                    this.m_lineBuffer.Writer.Write(blockResult.Memory.Span);
                }

                if (blockResult.IsCompleted)
                {
                    var readResult = this.m_lineBuffer.Reader.Read();
                    if (!readResult.Buffer.IsEmpty)
                    {
                        this.ParseLineFromSequence(readResult.Buffer);
                        this.m_lineBuffer.Reader.AdvanceTo(readResult.Buffer.End);
                    }

                    if (this.m_hasData)
                    {
                        var message = this.CreateMessage();
                        this.ResetMessageState();
                        return message;
                    }
                    return null;
                }
            }
        }
    }

    private void ResetMessageState()
    {
        this.m_dataBuilder.Clear();
        this.m_currentEvent = null;
        this.m_currentComment = null;
        this.m_hasData = false;
    }

    private SseMessage CreateMessage()
    {
        if (this.m_dataBuilder.Length > 0 && this.m_dataBuilder[this.m_dataBuilder.Length - 1] == '\n')
        {
            this.m_dataBuilder.Length--;
        }

        var message = new SseMessage
        {
            EventType = this.m_currentEvent,
            Data = this.m_dataBuilder.ToString(),
            EventId = this.m_lastEventId,
            ReconnectionInterval = this.m_retryInterval,
            Comment = this.m_currentComment
        };

        return message;
    }

    private void ParseLineFromSequence(ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsSingleSegment)
        {
            this.ParseLine(sequence.First.Span);
        }
        else
        {
            using (var buffer = new ContiguousMemoryBuffer(sequence))
            {
                this.ParseLine(buffer.Memory.Span);
            }
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
            this.m_currentComment = lineSpan.Slice(commentStart).ToUtf8String();
            this.m_hasData = true;
            return;
        }

        var colonIndex = lineSpan.IndexOf((byte)':');
        if (colonIndex == -1)
        {
            var fieldName = lineSpan.ToUtf8String();
            this.ProcessField(fieldName, string.Empty);
            return;
        }

        var fieldNameSpan = lineSpan.Slice(0, colonIndex);
        var fieldName2 = fieldNameSpan.ToUtf8String();

        var valueStart = colonIndex + 1;
        if (valueStart < lineSpan.Length && lineSpan[valueStart] == (byte)' ')
        {
            valueStart++;
        }

        var valueSpan = lineSpan.Slice(valueStart);
        var value = valueSpan.ToUtf8String();

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
                this.m_hasData = true;
                break;

            case "id":
                if (!value.Contains('\0'))
                {
                    this.m_lastEventId = value;
                }
                break;

            case "retry":
                if (int.TryParse(value, out var retry) && retry >= 0)
                {
                    this.m_retryInterval = TimeSpan.FromMilliseconds(retry);
                }
                break;
        }
    }

    private SseMessage ProcessBuffer()
    {
        while (true)
        {
            var readResult = this.m_lineBuffer.Reader.Read();
            if (readResult.Buffer.IsEmpty)
            {
                break;
            }

            var position = readResult.Buffer.PositionOf((byte)'\n');
            if (position == null)
            {
                break;
            }

            var lineSequence = readResult.Buffer.Slice(0, position.Value);
            var lineLength = lineSequence.Length;

            if (lineLength > 0)
            {
                var lastPosition = readResult.Buffer.GetPosition(lineLength - 1);
                var lastByte = readResult.Buffer.Slice(lastPosition, 1).First.Span[0];
                if (lastByte == '\r')
                {
                    lineSequence = readResult.Buffer.Slice(0, lastPosition);
                }
            }

            this.ParseLineFromSequence(lineSequence);

            var nextPosition = readResult.Buffer.GetPosition(1, position.Value);
            this.m_lineBuffer.Reader.AdvanceTo(nextPosition);

            if (lineSequence.Length == 0)
            {
                if (this.m_hasData)
                {
                    return this.CreateMessage();
                }
            }
        }
        return null;
    }
}