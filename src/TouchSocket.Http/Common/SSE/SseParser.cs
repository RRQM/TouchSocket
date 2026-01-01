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

namespace TouchSocket.Http
{
    /// <summary>
    /// SSE（Server-Sent Events）解析器。
    /// 符合W3C标准：https://html.spec.whatwg.org/multipage/server-sent-events.html
    /// </summary>
    public class SseParser : IDisposable
    {
        private readonly Encoding m_encoding;
        private byte[] m_buffer;
        private int m_bufferPos;
        private SseMessage m_currentMessage;
        private bool m_isDisposed;

        /// <summary>
        /// 初始化一个SSE解析器实例，使用指定的编码。
        /// </summary>
        /// <param name="encoding">字符编码，默认为UTF-8。</param>
        public SseParser(Encoding encoding = null)
        {
            this.m_encoding = encoding ?? Encoding.UTF8;
            this.m_buffer = new byte[4096];
            this.m_bufferPos = 0;
        }

        /// <summary>
        /// 处理接收到的字节数据，并返回解析完成的事件列表。
        /// </summary>
        /// <param name="data">字节数据。</param>
        /// <returns>解析完成的事件列表。</returns>
        public List<SseMessage> ProcessBytes(ReadOnlyMemory<byte> data)
        {
            if (this.m_isDisposed)
                throw new ObjectDisposedException(nameof(SseParser));

            var messages = new List<SseMessage>();

            if (data.IsEmpty)
            {
                // 处理空数据，检查是否有待处理的消息
                if (this.m_currentMessage != null && !string.IsNullOrEmpty(this.m_currentMessage.Data))
                {
                    messages.Add(this.m_currentMessage);
                    this.m_currentMessage = null;
                }
                return messages;
            }

            var span = data.Span;
            int offset = 0;

            while (offset < span.Length)
            {
                // 查找换行符
                int newlineIndex = -1;
                int skipCount = 1; // 默认跳过1个字符（LF或CR）

                for (int i = offset; i < span.Length && newlineIndex == -1; i++)
                {
                    byte b = span[i];

                    if (b == 0x0A) // LF (\n)
                    {
                        newlineIndex = i;
                        break;
                    }
                    else if (b == 0x0D) // CR (\r)
                    {
                        newlineIndex = i;
                        // 检查是否为CRLF
                        if (i + 1 < span.Length && span[i + 1] == 0x0A)
                        {
                            skipCount = 2;
                        }
                        break;
                    }
                }

                int lineLength = (newlineIndex == -1) ? span.Length - offset : newlineIndex - offset;

                // 确保缓冲区足够大
                this.EnsureBufferCapacity(this.m_bufferPos + lineLength);

                // 复制数据到缓冲区
                span.Slice(offset, lineLength).CopyTo(this.m_buffer.AsSpan(this.m_bufferPos));
                this.m_bufferPos += lineLength;

                if (newlineIndex != -1)
                {
                    // 处理一行数据
                    var message = this.ProcessLine(this.m_buffer, this.m_bufferPos);
                    if (message != null)
                    {
                        messages.Add(message);
                    }

                    this.m_bufferPos = 0;
                    offset = newlineIndex + skipCount;
                }
                else
                {
                    // 没有换行符，等待更多数据
                    offset = span.Length;
                }
            }

            return messages;
        }

        /// <summary>
        /// 处理一行数据。
        /// </summary>
        private SseMessage ProcessLine(byte[] buffer, int count)
        {
            // 空行表示事件结束
            if (count == 0)
            {
                if (this.m_currentMessage != null && !string.IsNullOrEmpty(this.m_currentMessage.Data))
                {
                    var message = this.m_currentMessage;
                    this.m_currentMessage = null;
                    return message;
                }
                return null;
            }

            // 注释行（以冒号开头）
            if (buffer[0] == 0x3A) // ':'
            {
                // 跳过冒号和可能存在的空格
                int start = 1;
                while (start < count && buffer[start] == 0x20) // 空格
                {
                    start++;
                }

                if (start < count)
                {
                    string comment = this.m_encoding.GetString(buffer, start, count - start);
                    return new SseMessage
                    {
                        IsComment = true,
                        Data = comment
                    };
                }
                return null;
            }

            // 查找字段分隔符（冒号）
            int colonIndex = -1;
            for (int i = 0; i < count && colonIndex == -1; i++)
            {
                if (buffer[i] == 0x3A) // ':'
                {
                    colonIndex = i;
                    break;
                }
            }

            string fieldName;
            string fieldValue;

            if (colonIndex != -1)
            {
                // 字段名（冒号之前的部分）
                fieldName = this.m_encoding.GetString(buffer, 0, colonIndex);

                // 字段值（冒号之后的部分，跳过可选的一个空格）
                int valueStart = colonIndex + 1;
                if (valueStart < count && buffer[valueStart] == 0x20) // 空格
                {
                    valueStart++;
                }

                fieldValue = (valueStart < count)
                    ? this.m_encoding.GetString(buffer, valueStart, count - valueStart)
                    : string.Empty;
            }
            else
            {
                // 没有冒号，整行作为字段名，字段值为空
                fieldName = this.m_encoding.GetString(buffer, 0, count);
                fieldValue = string.Empty;
            }

            // 根据字段名处理
            if (this.m_currentMessage == null)
            {
                this.m_currentMessage = new SseMessage();
            }

            switch (fieldName)
            {
                case "event":
                    this.m_currentMessage.EventType = fieldValue;
                    break;

                case "data":
                    if (this.m_currentMessage.Data == null)
                    {
                        this.m_currentMessage.Data = fieldValue;
                    }
                    else
                    {
                        this.m_currentMessage.Data += "\n" + fieldValue;
                    }
                    break;

                case "id":
                    // 如果id字段值为空，表示重置last event ID
                    this.m_currentMessage.Id = fieldValue;
                    break;

                case "retry":
                    if (int.TryParse(fieldValue, out int retry) && retry > 0)
                    {
                        this.m_currentMessage.Retry = retry;
                    }
                    break;

                default:
                    // 忽略未知字段
                    break;
            }

            return null;
        }

        /// <summary>
        /// 确保缓冲区容量。
        /// </summary>
        private void EnsureBufferCapacity(int requiredCapacity)
        {
            if (this.m_buffer.Length < requiredCapacity)
            {
                int newSize = Math.Max(requiredCapacity, this.m_buffer.Length * 2);
                Array.Resize(ref this.m_buffer, newSize);
            }
        }

        /// <summary>
        /// 重置解析器状态。
        /// </summary>
        public void Reset()
        {
            this.m_bufferPos = 0;
            this.m_currentMessage = null;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            if (!this.m_isDisposed)
            {
                this.m_buffer = null;
                this.m_currentMessage = null;
                this.m_isDisposed = true;
            }
        }
    }
}