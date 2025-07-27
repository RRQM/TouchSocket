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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 固定包头适配器。
/// 按照指定的包头类型（Byte、Ushort、Int）进行数据包的长度解析和组包。
/// 支持最小包长度校验，自动处理半包、粘包等情况。
/// </summary>
public class FixedHeaderPackageAdapter : CacheDataHandlingAdapter
{
    /// <summary>
    /// 固定包头类型，决定包头长度（1/2/4字节），默认Int。
    /// </summary>
    public FixedHeaderType FixedHeaderType { get; set; } = FixedHeaderType.Int;

    /// <summary>
    /// 获取或设置包数据的最小值（默认为0）。用于校验包体长度。
    /// </summary>
    public int MinPackageSize { get; set; } = 0;

    /// <summary>
    /// 收到数据的预处理入口。自动处理缓存合并、半包等情况。
    /// </summary>
    /// <param name="reader">数据读取器</param>
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
    /// 发送数据的预处理入口。自动添加包头并校验长度。
    /// </summary>
    /// <param name="memory">待发送数据</param>
    /// <param name="token">取消令牌</param>
    protected override async Task PreviewSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        this.ThrowIfLengthValidationFailed(memory.Length);

        ByteBlock byteBlock;
        Span<byte> lenBytes;

        switch (this.FixedHeaderType)
        {
            case FixedHeaderType.Byte:
                {
                    var dataLen = (byte)(memory.Length);
                    byteBlock = new ByteBlock(dataLen + 1);
                    lenBytes = new byte[] { dataLen };
                    break;
                }
            case FixedHeaderType.Ushort:
                {
                    var dataLen = (ushort)(memory.Length);
                    byteBlock = new ByteBlock(dataLen + 2);
                    lenBytes = new byte[2];
                    TouchSocketBitConverter.Default.WriteBytes(lenBytes, dataLen);
                    break;
                }
            case FixedHeaderType.Int:
                {
                    var dataLen = memory.Length;
                    byteBlock = new ByteBlock(dataLen + 4);
                    lenBytes = new byte[4];
                    TouchSocketBitConverter.Default.WriteBytes(lenBytes, dataLen);
                    break;
                }
            default: throw new InvalidEnumArgumentException(TouchSocketCoreResource.InvalidParameter.Format(nameof(this.FixedHeaderType)));
        }

        try
        {
            byteBlock.Write(lenBytes);
            byteBlock.Write(memory.Span);
            await this.GoSendAsync(byteBlock.Memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 处理接收到的数据，根据包头类型解析包体长度，自动处理半包、粘包。
    /// </summary>
    /// <param name="reader">数据读取器</param>
    private async Task ProcessReader(IByteBlockReader reader)
    {
        while (true)
        {
            var canReadMemory = reader.Memory.Slice(reader.Position);

            if (canReadMemory.IsEmpty)
            {
                return;
            }
            var index = 0;
            var canReadSpan = canReadMemory.Span;
            int length;
            switch (this.FixedHeaderType)
            {
                case FixedHeaderType.Byte:
                    length = canReadSpan[index];
                    index += 1;
                    break;

                case FixedHeaderType.Ushort:
                    if (canReadSpan.Length < 2)
                    {
                        this.Cache(canReadSpan);
                        return;
                    }
                    length = TouchSocketBitConverter.Default.To<ushort>(canReadSpan.Slice(index, 2));
                    index += 2;
                    break;

                case FixedHeaderType.Int:
                    if (canReadSpan.Length < 4)
                    {
                        this.Cache(canReadSpan);
                        return;
                    }
                    length = TouchSocketBitConverter.Default.To<int>(canReadSpan.Slice(index, 4));
                    index += 4;
                    break;

                default:
                    throw ThrowHelper.CreateInvalidEnumArgumentException(this.FixedHeaderType);
            }

            this.ThrowIfLengthValidationFailed(length);
            var recedSurPlusLength = canReadSpan.Length - index;
            if (recedSurPlusLength >= length)
            {
                var byteBlockReader = new ByteBlockReader(canReadMemory.Slice(index, length));
                await this.GoReceivedAsync(byteBlockReader, null).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                reader.Position += (length + index);
            }
            else //半包
            {
                this.Cache(canReadSpan);
                return;
            }
        }
    }

    /// <summary>
    /// 校验包体长度是否合法，超出范围则抛出异常。
    /// </summary>
    /// <param name="length">包体长度</param>
    private void ThrowIfLengthValidationFailed(int length)
    {
        if (length < this.MinPackageSize || length > this.MaxPackageSize)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_BetweenAnd(nameof(length), length, this.MinPackageSize, this.MaxPackageSize);
        }
    }
}