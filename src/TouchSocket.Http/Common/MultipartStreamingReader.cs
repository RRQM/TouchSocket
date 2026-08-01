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

using System.Buffers;
using System.Collections.Specialized;

namespace TouchSocket.Http;

/// <summary>
/// 基于滑动窗口的 multipart 流式解析器。
/// 通过 <see cref="HttpBase.ReadAsync"/> 按需读取数据，不会将请求体整体加载到内存。
/// </summary>
internal sealed class MultipartStreamingReader : IDisposable
{
    // 每次从请求体读取的块大小
    private const int c_readChunkSize = 8192;

    private readonly HttpRequest m_request;

    // 分隔符: "\r\n--{boundary}"，部分数据末尾到下一部分之间的边界标记
    private readonly byte[] m_delimiter;

    // 内部缓冲区: 大小为 c_readChunkSize + delimiter.Length，确保每次读取后都有足够的"安全"数据可输出
    private byte[] m_buffer;
    private int m_bufferStart;
    private int m_bufferLength;
    private bool m_isEof;
    private bool m_partDone;
    private bool m_disposed;

    public MultipartStreamingReader(HttpRequest request, string boundaryString)
    {
        m_request = request;
        m_delimiter = Encoding.UTF8.GetBytes($"\r\n--{boundaryString}");
        m_buffer = ArrayPool<byte>.Shared.Rent(c_readChunkSize + m_delimiter.Length * 2);
    }

    private static readonly byte[] s_crlf = { (byte)'\r', (byte)'\n' };
    private static readonly byte[] s_crlfcrlf = { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
    private static readonly byte[] s_finalSuffix = { (byte)'-', (byte)'-' };
    private static readonly byte[] s_moreSuffix = { (byte)'\r', (byte)'\n' };

    /// <summary>
    /// 跳过请求体开头的 <c>--{boundary}\r\n</c> 行，定位到第一个 part 的头部。
    /// </summary>
    public async Task SkipToFirstBoundaryAsync(CancellationToken cancellationToken)
    {
        // 搜索第一行末尾的 \r\n（即 --{boundary}\r\n 中的 \r\n）
        while (true)
        {
            await EnsureDataAsync(2, cancellationToken).ConfigureDefaultAwait();

            if (m_bufferLength == 0 || m_isEof)
            {
                return;
            }

            var span = GetUnprocessedSpan();
            // IndexOfFirst 返回模式最后一字节的下标，未找到返回 -1
            var endIdx = span.IndexOfFirst(0, m_bufferLength, s_crlf);
            if (endIdx >= 0)
            {
                Advance(endIdx + 1);
                return;
            }

            if (m_isEof)
            {
                return;
            }

            // 未找到 \r\n：跳过除最后一字节（可能是 \r）之外的所有数据
            var skip = Math.Max(0, m_bufferLength - 1);
            if (skip > 0)
            {
                Advance(skip);
            }
            else
            {
                await ReadMoreAsync(cancellationToken).ConfigureDefaultAwait();
            }
        }
    }

    /// <summary>
    /// 读取当前 part 的头部，直到 <c>\r\n\r\n</c>，并返回解析后的键值对集合。
    /// </summary>
    public async Task<NameValueCollection> ReadPartHeadersAsync(CancellationToken cancellationToken)
    {
        const int maxHeaderSize = 1024 * 64;

        while (true)
        {
            var span = GetUnprocessedSpan();
            // IndexOfFirst 返回 \r\n\r\n 最后一字节（第二个 \n）的下标
            var endIdx = span.IndexOfFirst(0, m_bufferLength, s_crlfcrlf);
            if (endIdx >= 0)
            {
                var headerStart = endIdx - s_crlfcrlf.Length + 1;
                var headerText = Encoding.UTF8.GetString(span.Slice(0, headerStart).ToArray());
                Advance(endIdx + 1);
                m_partDone = false;
                return ParsePartHeaders(headerText);
            }

            if (m_isEof)
            {
                ThrowHelper.ThrowException("Multipart 数据格式错误：未找到部分头部结束符。");
            }

            if (m_bufferLength >= maxHeaderSize)
            {
                ThrowHelper.ThrowException($"Multipart 头部超出最大允许大小 {maxHeaderSize} 字节。");
            }

            await ReadMoreAsync(cancellationToken).ConfigureDefaultAwait();
        }
    }

    /// <summary>
    /// 读取当前 part 的数据块。返回实际读取字节数；返回 <see langword="0"/> 表示当前 part 数据已读完（遇到边界）。
    /// </summary>
    public async ValueTask<int> ReadPartDataAsync(Memory<byte> output, CancellationToken cancellationToken)
    {
        if (m_partDone || output.IsEmpty)
        {
            return 0;
        }

        var written = 0;

        while (written < output.Length)
        {
            if (m_bufferLength == 0)
            {
                if (m_isEof)
                {
                    m_partDone = true;
                    break;
                }
                await ReadMoreAsync(cancellationToken).ConfigureDefaultAwait();
                if (m_bufferLength == 0)
                {
                    m_partDone = true;
                    break;
                }
            }

            var span = GetUnprocessedSpan();
            // IndexOfFirst 返回 delimiter 最后一字节的下标，delimStart 为边界起始位置
            var delimEndIdx = span.IndexOfFirst(0, m_bufferLength, m_delimiter);

            if (delimEndIdx >= 0)
            {
                // 找到边界：输出边界前的所有数据
                var delimStart = delimEndIdx - m_delimiter.Length + 1;
                var canCopy = Math.Min(delimStart, output.Length - written);
                span.Slice(0, canCopy).CopyTo(output.Span.Slice(written));
                written += canCopy;
                Advance(canCopy);

                // 若已将边界前数据全部输出，则标记当前 part 结束
                if (canCopy == delimStart)
                {
                    m_partDone = true;
                }

                break;
            }
            else
            {
                // 未找到完整边界：安全输出的范围为 [0, bufferLength - (delimLen - 1))
                // 保留末尾 delimLen-1 字节，防止边界跨越读取窗口
                var safeCount = m_bufferLength - (m_delimiter.Length - 1);

                if (safeCount <= 0)
                {
                    if (m_isEof)
                    {
                        // 到达流末尾但没有找到边界（格式错误），输出剩余数据
                        var copy = Math.Min(m_bufferLength, output.Length - written);
                        span.Slice(0, copy).CopyTo(output.Span.Slice(written));
                        written += copy;
                        Advance(copy);
                        if (m_bufferLength == 0)
                        {
                            m_partDone = true;
                        }
                        break;
                    }
                    await ReadMoreAsync(cancellationToken).ConfigureDefaultAwait();
                    continue;
                }

                var copyCount = Math.Min(safeCount, output.Length - written);
                span.Slice(0, copyCount).CopyTo(output.Span.Slice(written));
                written += copyCount;
                Advance(copyCount);
            }
        }

        return written;
    }

    /// <summary>
    /// 在当前 part 数据读取完毕后，跳过边界标记并判断是否还有更多 part。
    /// </summary>
    /// <returns>如果还有更多 part 则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public async Task<bool> MoveToNextPartAsync(CancellationToken cancellationToken)
    {
        // 跳过 "\r\n--{boundary}"
        await EnsureDataAsync(m_delimiter.Length, cancellationToken).ConfigureDefaultAwait();
        if (m_bufferLength < m_delimiter.Length)
        {
            return false;
        }

        Advance(m_delimiter.Length);

        // 读取边界后的 2 字节后缀："\r\n"（还有更多 part）或 "--"（最终边界）
        await EnsureDataAsync(2, cancellationToken).ConfigureDefaultAwait();
        if (m_bufferLength < 2)
        {
            return false;
        }

        var span = GetUnprocessedSpan();
        if (span[0] == s_finalSuffix[0] && span[1] == s_finalSuffix[1])
        {
            // 最终边界 "--"
            return false;
        }

        if (span[0] == s_moreSuffix[0] && span[1] == s_moreSuffix[1])
        {
            // 还有更多 part "\r\n"
            Advance(2);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 丢弃当前 part 剩余的未读数据（用于文件回调未完整读取的情况）。
    /// </summary>
    public async Task DrainCurrentPartAsync(CancellationToken cancellationToken)
    {
        if (m_partDone)
        {
            return;
        }

        using var owner = MemoryPool<byte>.Shared.Rent(c_readChunkSize);
        while (true)
        {
            var read = await ReadPartDataAsync(owner.Memory, cancellationToken).ConfigureDefaultAwait();
            if (read == 0)
            {
                break;
            }
        }
    }

    private ReadOnlySpan<byte> GetUnprocessedSpan()
        => m_buffer.AsSpan(m_bufferStart, m_bufferLength);

    private void Advance(int count)
    {
        m_bufferStart += count;
        m_bufferLength -= count;
    }

    private async Task EnsureDataAsync(int minBytes, CancellationToken cancellationToken)
    {
        while (m_bufferLength < minBytes && !m_isEof)
        {
            await ReadMoreAsync(cancellationToken).ConfigureDefaultAwait();
        }
    }

    private async Task ReadMoreAsync(CancellationToken cancellationToken)
    {
        if (m_isEof)
        {
            return;
        }

        // 压缩缓冲区：将未处理的数据移至起始位置
        if (m_bufferStart > 0)
        {
            if (m_bufferLength > 0)
            {
                m_buffer.AsSpan(m_bufferStart, m_bufferLength).CopyTo(m_buffer);
            }
            m_bufferStart = 0;
        }

        var available = m_buffer.Length - m_bufferLength;
        if (available <= 0)
        {
            // 缓冲区已满（通常仅在读取超大头部时发生），扩展缓冲区
            var newBuffer = ArrayPool<byte>.Shared.Rent(m_buffer.Length * 2);
            m_buffer.AsSpan(0, m_bufferLength).CopyTo(newBuffer);
            ArrayPool<byte>.Shared.Return(m_buffer);
            m_buffer = newBuffer;
            available = m_buffer.Length - m_bufferLength;
        }

        var read = await m_request.ReadAsync(
            m_buffer.AsMemory(m_bufferLength, available),
            cancellationToken).ConfigureDefaultAwait();

        if (read == 0)
        {
            m_isEof = true;
        }
        else
        {
            m_bufferLength += read;
        }
    }

    private static NameValueCollection ParsePartHeaders(string headerText)
    {
        var result = new NameValueCollection();
        var items = headerText.Split(new[] { ";", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in items)
        {
            // 与 InternalFormCollection 保持相同的解析逻辑：先按 ':' 再按 '=' 分割
            var kv = item.Split(new[] { ':', '=' }, 2);
            if (kv.Length == 2)
            {
                var key = kv[0].Trim();
                var value = kv[1].Replace("\"", string.Empty).Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    result.Add(key, value);
                }
            }
        }
        return result;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!m_disposed)
        {
            m_disposed = true;
            if (m_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(m_buffer);
                m_buffer = null;
            }
        }
    }
}

/// <summary>
/// multipart part 段的流式实现，封装 <see cref="MultipartStreamingReader"/> 的数据读取。
/// </summary>
internal sealed class InternalStreamFormSection : IStreamFormSection
{
    private readonly MultipartStreamingReader m_reader;

    public InternalStreamFormSection(MultipartStreamingReader reader, NameValueCollection dataPair)
    {
        m_reader = reader;
        DataPair = dataPair;
    }

    /// <inheritdoc/>
    public string ContentDisposition => DataPair["Content-Disposition"];

    /// <inheritdoc/>
    public string ContentType => DataPair["Content-Type"];

    /// <inheritdoc/>
    public NameValueCollection DataPair { get; }

    /// <inheritdoc/>
    public string FileName => DataPair["filename"];

    /// <inheritdoc/>
    public string Name => DataPair["name"];

    /// <inheritdoc/>
    public bool IsFile => !this.FileName.IsNullOrEmpty();

    /// <inheritdoc/>
    public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(8192);
        try
        {
            while (true)
            {
                var read = await m_reader.ReadPartDataAsync(buffer.AsMemory(), cancellationToken).ConfigureDefaultAwait();
                if (read == 0)
                {
                    break;
                }
                await destination.WriteAsync(buffer, 0, read, cancellationToken).ConfigureDefaultAwait();
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <inheritdoc/>
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => m_reader.ReadPartDataAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await this.CopyToAsync(ms, cancellationToken).ConfigureDefaultAwait();
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }
}

/// <summary>
/// multipart/form-data 的流式表单读取器。
/// </summary>
internal sealed class InternalMultipartFormReader : IStreamingFormReader
{
    private readonly HttpRequest m_request;
    private readonly string m_boundaryString;
    private MultipartStreamingReader m_reader;
    private bool m_initialized;
    private bool m_done;
    private bool m_disposed;

    public InternalMultipartFormReader(HttpRequest request, string boundaryString)
    {
        m_request = request;
        m_boundaryString = boundaryString;
    }

    /// <inheritdoc/>
    public async Task<IStreamFormSection> ReadNextSectionAsync(CancellationToken cancellationToken = default)
    {
        if (m_done)
        {
            return null;
        }

        if (!m_initialized)
        {
            m_reader = new MultipartStreamingReader(m_request, m_boundaryString);
            await m_reader.SkipToFirstBoundaryAsync(cancellationToken).ConfigureDefaultAwait();
            m_initialized = true;
        }
        else
        {
            // 丢弃上一段未读完的数据，再移动到下一个 part
            await m_reader.DrainCurrentPartAsync(cancellationToken).ConfigureDefaultAwait();
            var hasMore = await m_reader.MoveToNextPartAsync(cancellationToken).ConfigureDefaultAwait();
            if (!hasMore)
            {
                m_done = true;
                return null;
            }
        }

        var headers = await m_reader.ReadPartHeadersAsync(cancellationToken).ConfigureDefaultAwait();
        return new InternalStreamFormSection(m_reader, headers);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!m_disposed)
        {
            m_disposed = true;
            m_reader?.Dispose();
        }
    }
}

/// <summary>
/// application/x-www-form-urlencoded 的表单读取器（整体加载后逐字段返回）。
/// </summary>
internal sealed class InternalUrlEncodedFormReader : IStreamingFormReader
{
    private readonly HttpRequest m_request;
    private readonly Encoding m_encoding;
    private List<IStreamFormSection> m_sections;
    private int m_index;

    public InternalUrlEncodedFormReader(HttpRequest request, Encoding encoding)
    {
        m_request = request;
        m_encoding = encoding;
    }

    /// <inheritdoc/>
    public async Task<IStreamFormSection> ReadNextSectionAsync(CancellationToken cancellationToken = default)
    {
        if (m_sections == null)
        {
            m_sections = new List<IStreamFormSection>();
            var bytes = await m_request.GetContentAsync(cancellationToken).ConfigureDefaultAwait();
            if (!bytes.IsEmpty)
            {
                var form = new InternalFormCollection(bytes, m_encoding);
                foreach (var kv in form)
                {
                    m_sections.Add(new InternalUrlEncodedSection(kv.Key, kv.Value));
                }
            }
        }

        if (m_index >= m_sections.Count)
        {
            return null;
        }

        return m_sections[m_index++];
    }

    /// <inheritdoc/>
    public void Dispose() { }
}

/// <summary>
/// urlencoded 文本字段的 <see cref="IStreamFormSection"/> 实现。
/// </summary>
internal sealed class InternalUrlEncodedSection : IStreamFormSection
{
    private byte[] m_encodedValue;
    private int m_position;

    public InternalUrlEncodedSection(string name, string value)
    {
        Name = name;
        var dataPair = new NameValueCollection();
        dataPair.Add("name", name);
        DataPair = dataPair;
        // 延迟编码
        m_encodedValue = null;
        ReadAsStringAsync_Value = value;
    }

    // 仅供 ReadAsStringAsync 快速返回，避免重新解码
    private string ReadAsStringAsync_Value { get; }

    /// <inheritdoc/>
    public string ContentDisposition => $"form-data; name=\"{Name}\"";

    /// <inheritdoc/>
    public string ContentType => "text/plain";

    /// <inheritdoc/>
    public NameValueCollection DataPair { get; }

    /// <inheritdoc/>
    public string FileName => null;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsFile => false;

    /// <inheritdoc/>
    public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        var bytes = GetEncodedBytes();
        await destination.WriteAsync(bytes, m_position, bytes.Length - m_position, cancellationToken).ConfigureDefaultAwait();
        m_position = bytes.Length;
    }

    /// <inheritdoc/>
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var bytes = GetEncodedBytes();
        var remaining = bytes.Length - m_position;
        if (remaining <= 0)
        {
            return EasyValueTask.FromResult(0);
        }
        var toCopy = Math.Min(remaining, buffer.Length);
        bytes.AsSpan(m_position, toCopy).CopyTo(buffer.Span);
        m_position += toCopy;
        return EasyValueTask.FromResult(toCopy);
    }

    /// <inheritdoc/>
    public Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(ReadAsStringAsync_Value);

    private byte[] GetEncodedBytes()
        => m_encodedValue ??= Encoding.UTF8.GetBytes(ReadAsStringAsync_Value);
}

/// <summary>
/// 无表单内容时的空读取器。
/// </summary>
internal sealed class InternalEmptyFormReader : IStreamingFormReader
{
    /// <inheritdoc/>
    public Task<IStreamFormSection> ReadNextSectionAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IStreamFormSection>(null);

    /// <inheritdoc/>
    public void Dispose() { }
}

