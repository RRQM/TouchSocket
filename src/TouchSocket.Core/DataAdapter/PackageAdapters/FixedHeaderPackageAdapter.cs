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
using System.Threading.Tasks;
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 固定包头适配器。
/// 按照指定的包头类型（Byte、Ushort、Int）进行数据包的长度解析和组包。
/// 支持最小包长度校验，自动处理半包、粘包等情况。
/// </summary>
public class FixedHeaderPackageAdapter : SingleStreamDataHandlingAdapter
{
    /// <summary>
    /// 固定包头类型，决定包头长度（1/2/4字节），默认Int。
    /// </summary>
    public FixedHeaderType FixedHeaderType { get; set; } = FixedHeaderType.Int;

    /// <summary>
    /// 获取或设置包数据的最小值（默认为0）。用于校验包体长度。
    /// </summary>
    public int MinPackageSize { get; set; } = 0;

    public override void SendInput<TWriter>(ref TWriter writer, in ReadOnlyMemory<byte> memory)
    {
        this.ThrowIfLengthValidationFailed(memory.Length);
        Span<byte> lenBytes = stackalloc byte[4];

        switch (this.FixedHeaderType)
        {
            case FixedHeaderType.Byte:
                {
                    var dataLen = (byte)(memory.Length);
                    lenBytes[0] = dataLen;
                    lenBytes = lenBytes.Slice(0, 1);
                    break;
                }
            case FixedHeaderType.Ushort:
                {
                    var dataLen = (ushort)(memory.Length);
                    TouchSocketBitConverter.Default.WriteBytes(lenBytes, dataLen);
                    lenBytes = lenBytes.Slice(0, 2);
                    break;
                }
            case FixedHeaderType.Int:
                {
                    var dataLen = memory.Length;
                    TouchSocketBitConverter.Default.WriteBytes(lenBytes, dataLen);
                    break;
                }
            default: throw new InvalidEnumArgumentException(TouchSocketCoreResource.InvalidParameter.Format(nameof(this.FixedHeaderType)));
        }

        writer.Write(lenBytes);
        writer.Write(memory.Span);
    }

    /// <summary>
    /// 收到数据的预处理入口。自动处理缓存合并、半包等情况。
    /// </summary>
    /// <param name="reader">数据读取器</param>
    protected override async Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            var index = 0;
            int length;
            switch (this.FixedHeaderType)
            {
                case FixedHeaderType.Byte:
                    {
                        var headerSpan = reader.GetSpan(1);
                        length = headerSpan[index];
                        index += 1;
                        break;
                    }

                case FixedHeaderType.Ushort:
                    {
                        if (reader.BytesRemaining < 2)
                        {
                            return;
                        }
                        var headerSpan = reader.GetSpan(2);
                        length = TouchSocketBitConverter.Default.To<ushort>(headerSpan);
                        index += 2;
                        break;
                    }

                case FixedHeaderType.Int:
                    {
                        if (reader.BytesRemaining < 4)
                        {
                            return;
                        }
                        var headerSpan = reader.GetSpan(4);
                        length = TouchSocketBitConverter.Default.To<int>(headerSpan);
                        index += 4;
                        break;
                    }

                default:
                    throw ThrowHelper.CreateInvalidEnumArgumentException(this.FixedHeaderType);
            }

            this.ThrowIfLengthValidationFailed(length);
            var recedSurPlusLength = (int)(reader.BytesRemaining - index);
            if (recedSurPlusLength >= length)
            {
                var memory = reader.GetMemory(index + length).Slice(index, length);
                await this.GoReceivedAsync(memory, null).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                reader.Advance(index + length);
            }
            else //半包
            {
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