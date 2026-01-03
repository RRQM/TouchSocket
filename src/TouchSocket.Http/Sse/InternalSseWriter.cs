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

        var byteBlock = new ByteBlock(1024);
        try
        {
            if (!string.IsNullOrEmpty(message.Comment))
            {
                TouchSocketHttpUtility.AppendColon(ref byteBlock);
                TouchSocketHttpUtility.AppendSpace(ref byteBlock);
                TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, message.Comment);
                TouchSocketHttpUtility.AppendRn(ref byteBlock);
            }

            if (!string.IsNullOrEmpty(message.EventId))
            {
                TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, "id");
                TouchSocketHttpUtility.AppendColon(ref byteBlock);
                TouchSocketHttpUtility.AppendSpace(ref byteBlock);
                TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, message.EventId);
                TouchSocketHttpUtility.AppendRn(ref byteBlock);
            }

            if (!string.IsNullOrEmpty(message.EventType) && message.EventType != "message")
            {
                TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, "event");
                TouchSocketHttpUtility.AppendColon(ref byteBlock);
                TouchSocketHttpUtility.AppendSpace(ref byteBlock);
                TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, message.EventType);
                TouchSocketHttpUtility.AppendRn(ref byteBlock);
            }

            if (message.ReconnectionInterval.HasValue)
            {
                TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, "retry");
                TouchSocketHttpUtility.AppendColon(ref byteBlock);
                TouchSocketHttpUtility.AppendSpace(ref byteBlock);
                TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, ((int)message.ReconnectionInterval.Value.TotalMilliseconds).ToString());
                TouchSocketHttpUtility.AppendRn(ref byteBlock);
            }

            if (!string.IsNullOrEmpty(message.Data))
            {
                var lines = message.Data.Split('\n');
                foreach (var line in lines)
                {
                    TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, "data");
                    TouchSocketHttpUtility.AppendColon(ref byteBlock);
                    TouchSocketHttpUtility.AppendSpace(ref byteBlock);
                    TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, line.TrimEnd('\r'));
                    TouchSocketHttpUtility.AppendRn(ref byteBlock);
                }
            }

            TouchSocketHttpUtility.AppendRn(ref byteBlock);

            await this.m_response.WriteAsync(byteBlock.Memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    public override Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        return this.m_response.CompleteChunkAsync(cancellationToken);
    }
}
