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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 终止符数据包处理适配器，支持以指定终止符（字符或字节数组）结尾的数据包解析。
/// 适用于流式数据协议，自动分包并处理缓存，支持保留终止符选项。
/// </summary>
public class TerminatorPackageAdapter : CacheDataHandlingAdapter
{
    /// <summary>
    /// 终止符字节序列。
    /// </summary>
    private readonly ReadOnlyMemory<byte> m_terminatorCode;

    /// <summary>
    /// 使用UTF8编码的终止符构造适配器。
    /// </summary>
    /// <param name="terminator">终止符字符串。</param>
    public TerminatorPackageAdapter(string terminator) : this(0, Encoding.UTF8.GetBytes(terminator))
    {
    }

    /// <summary>
    /// 使用指定编码的终止符构造适配器。
    /// </summary>
    /// <param name="terminator">终止符字符串。</param>
    /// <param name="encoding">编码方式。</param>
    public TerminatorPackageAdapter(string terminator, Encoding encoding)
        : this(0, encoding.GetBytes(terminator))
    {
    }

    /// <summary>
    /// 使用指定最小包长度和终止符字节数组构造适配器。
    /// </summary>
    /// <param name="minSize">最小包长度。</param>
    /// <param name="terminatorCode">终止符字节数组。</param>
    public TerminatorPackageAdapter(int minSize, byte[] terminatorCode)
    {
        this.MinSize = minSize;
        this.m_terminatorCode = terminatorCode;
    }

    /// <summary>
    /// 最小包长度，默认为0。
    /// </summary>
    public int MinSize { get; }

    /// <summary>
    /// 是否保留终止符在解析后的数据包中。
    /// </summary>
    public bool ReserveTerminatorCode { get; set; }

    /// <summary>
    /// 预处理接收到的数据，自动分包并处理缓存。
    /// </summary>
    /// <param name="reader">数据读取器。</param>
    protected override async Task PreviewReceivedAsync(IByteBlockReader reader)
    {
        ReaderExtension.SeekToStart(ref reader);
        var canReadSpan = reader.Span.Slice(reader.Position);
        if (this.TryCombineCache(canReadSpan, out var byteBlock))
        {
            using (byteBlock)
            {
                await this.ProcessReader(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                return;
            }
        }

        await this.ProcessReader(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 预处理发送的数据，自动添加终止符。
    /// </summary>
    /// <param name="memory">待发送数据。</param>
    /// <param name="token">取消令牌。</param>
    protected override async Task PreviewSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        if (memory.Length > this.MaxPackageSize)
        {
            throw new Exception(TouchSocketCoreResource.ValueMoreThan.Format(nameof(memory.Length), this.MaxPackageSize));
        }
        var dataLen = memory.Length + this.m_terminatorCode.Length;
        var byteBlock = new ByteBlock(dataLen);
        byteBlock.Write(memory.Span);
        byteBlock.Write(this.m_terminatorCode.Span);

        try
        {
            await this.GoSendAsync(byteBlock.Memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 处理数据读取，查找终止符并分包。
    /// </summary>
    /// <param name="reader">数据读取器。</param>
    private async Task ProcessReader(IByteBlockReader reader)
    {
        while (true)
        {
            var canReadMemory = reader.Memory.Slice(reader.Position);
            if (canReadMemory.IsEmpty)
            {
                return;
            }

            var index = canReadMemory.Span.IndexOf(this.m_terminatorCode.Span);
            if (index < 0)
            {
                this.Cache(canReadMemory.Span);
                return;
            }
            var length = index + this.m_terminatorCode.Length;
            var byteBlockReader = new ByteBlockReader(canReadMemory.Slice(0, this.ReserveTerminatorCode ? length : index));

            await this.GoReceivedAsync(byteBlockReader, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            reader.Advance(length);
        }
    }
}