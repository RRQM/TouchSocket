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

internal class InternalSseWriter : SseWriter
{
    private readonly HttpResponse m_response;
   
    public InternalSseWriter(HttpResponse response)
    {
        response.IsChunk = true;
        this.m_response = response;
    }
    
    public override async Task WriteAsync(SseMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            return;
        }

        var writer = new SegmentedBytesWriter();
        try
        {
            var hasContent = false;

            if (!string.IsNullOrEmpty(message.Comment))
            {
                TouchSocketHttpUtility.AppendColon(ref writer);
                TouchSocketHttpUtility.AppendSpace(ref writer);
                TouchSocketHttpUtility.AppendUtf8String(ref writer, message.Comment);
                TouchSocketHttpUtility.AppendRn(ref writer);
                hasContent = true;
            }

            if (!string.IsNullOrEmpty(message.EventId) && !message.EventId.Contains('\0'))
            {
                TouchSocketHttpUtility.AppendUtf8String(ref writer, "id");
                TouchSocketHttpUtility.AppendColon(ref writer);
                TouchSocketHttpUtility.AppendSpace(ref writer);
                TouchSocketHttpUtility.AppendUtf8String(ref writer, message.EventId);
                TouchSocketHttpUtility.AppendRn(ref writer);
                hasContent = true;
            }

            if (!string.IsNullOrEmpty(message.EventType) && message.EventType != "message")
            {
                TouchSocketHttpUtility.AppendUtf8String(ref writer, "event");
                TouchSocketHttpUtility.AppendColon(ref writer);
                TouchSocketHttpUtility.AppendSpace(ref writer);
                TouchSocketHttpUtility.AppendUtf8String(ref writer, message.EventType);
                TouchSocketHttpUtility.AppendRn(ref writer);
                hasContent = true;
            }

            if (message.ReconnectionInterval.HasValue)
            {
                TouchSocketHttpUtility.AppendUtf8String(ref writer, "retry");
                TouchSocketHttpUtility.AppendColon(ref writer);
                TouchSocketHttpUtility.AppendSpace(ref writer);
                TouchSocketHttpUtility.AppendUtf8String(ref writer, ((int)message.ReconnectionInterval.Value.TotalMilliseconds).ToString());
                TouchSocketHttpUtility.AppendRn(ref writer);
                hasContent = true;
            }

            if (!string.IsNullOrEmpty(message.Data))
            {
                var lines = message.Data.Split('\n');
                foreach (var line in lines)
                {
                    TouchSocketHttpUtility.AppendUtf8String(ref writer, "data");
                    TouchSocketHttpUtility.AppendColon(ref writer);
                    TouchSocketHttpUtility.AppendSpace(ref writer);
                    TouchSocketHttpUtility.AppendUtf8String(ref writer, line.TrimEnd('\r'));
                    TouchSocketHttpUtility.AppendRn(ref writer);
                }
                hasContent = true;
            }

            if (hasContent)
            {
                TouchSocketHttpUtility.AppendRn(ref writer);

                foreach (var item in writer.Sequence)
                {
                    await this.m_response.WriteAsync(item, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        finally
        {
            writer.Dispose();
        }
    }

    public override Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        return this.m_response.CompleteChunkAsync(cancellationToken);
    }
}
