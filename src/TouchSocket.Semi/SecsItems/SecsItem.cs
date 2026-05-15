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

using System.Runtime.CompilerServices;
using TouchSocket.Core;

namespace TouchSocket.Semi;

/// <summary>
/// 表示 SECS-II 数据项的抽象基类。
/// </summary>
public abstract class SecsItem : IPackage
{
    /// <summary>
    /// 获取数据项的字节长度。
    /// </summary>
    public uint Length { get; protected set; }

    /// <summary>
    /// 获取数据项的 SECS-II 格式类型。
    /// </summary>
    public abstract SecsFormat SecsFormat { get; }

    /// <summary>
    /// 将第一个字节转换为 <see cref="SecsFormat"/>。
    /// </summary>
    /// <param name="b">原始字节。</param>
    public static SecsFormat ConvertToSecsFormat(byte b)
    {
        var result = (byte)((b & 0b11111100) >> 2);
        return (SecsFormat)result;
    }

    /// <summary>
    /// 根据 <see cref="SecsFormat"/> 创建对应的 <see cref="SecsItem"/> 实例。
    /// </summary>
    /// <param name="secsFormat">格式类型。</param>
    public static SecsItem CreateSecsItem(SecsFormat secsFormat)
    {
        return secsFormat switch
        {
            SecsFormat.List => new ListSecsItem(),
            SecsFormat.Binary => new BinarySecsItem(),
            SecsFormat.Boolean => new BooleanSecsItem(),
            SecsFormat.ASCII => new ASCIISecsItem(),
            SecsFormat.JIS8 => new JIS8SecsItem(),
            SecsFormat.I8 => new I8SecsItem(),
            SecsFormat.I1 => new I1SecsItem(),
            SecsFormat.I2 => new I2SecsItem(),
            SecsFormat.I4 => new I4SecsItem(),
            SecsFormat.F8 => new F8SecsItem(),
            SecsFormat.F4 => new F4SecsItem(),
            SecsFormat.U8 => new U8SecsItem(),
            SecsFormat.U1 => new U1SecsItem(),
            SecsFormat.U2 => new U2SecsItem(),
            SecsFormat.U4 => new U4SecsItem(),
            _ => throw new NotSupportedException($"不支持的 SecsFormat：{secsFormat}。")
        };
    }

    /// <summary>
    /// 从字节中提取低 2 位，用于确定长度字节数。
    /// </summary>
    /// <param name="b">原始字节。</param>
    public static byte ExtractLengthByteCount(byte b)
    {
        return (byte)(b & 0b00000011);
    }

    /// <summary>
    /// 从读取器中读取并解析一个 <see cref="SecsItem"/>。
    /// </summary>
    /// <typeparam name="TReader">字节读取器类型。</typeparam>
    /// <param name="reader">字节读取器。</param>
    public static SecsItem ReadSecsItem<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var firstByte = reader.GetSpan(1)[0];
        var secsFormat = ConvertToSecsFormat(firstByte);
        var secsItem = CreateSecsItem(secsFormat);
        secsItem.Unpackage(ref reader);
        return secsItem;
    }

    /// <inheritdoc/>
    public virtual void Package<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
    }

    /// <inheritdoc/>
    public virtual void Unpackage<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var firstByte = reader.GetSpan(1)[0];
        reader.Advance(1);
        var lenCount = ExtractLengthByteCount(firstByte);
        this.Length = ReadLength(ref reader, lenCount);
    }

    private static uint ReadLength<TReader>(ref TReader reader, byte lenCount)
        where TReader : IBytesReader
    {
        switch (lenCount)
        {
            case 0:
                return 0;
            case 1:
                {
                    var v = reader.GetSpan(1)[0];
                    reader.Advance(1);
                    return v;
                }
            case 2:
                {
                    var v = TouchSocketBitConverter.BigEndian.To<ushort>(reader.GetSpan(2));
                    reader.Advance(2);
                    return v;
                }
            case 3:
                {
                    var span = reader.GetSpan(3);
                    var v = (uint)((span[0] << 16) | (span[1] << 8) | span[2]);
                    reader.Advance(3);
                    return v;
                }
            default:
                throw new NotSupportedException($"不支持的长度字节数：{lenCount}。");
        }
    }

    /// <summary>
    /// 写入数据项头字节（Format + LenCount）及长度字段。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型。</typeparam>
    /// <param name="writer">字节写入器。</param>
    /// <param name="format">格式类型。</param>
    /// <param name="length">数据长度。</param>
    protected static void WriteHeader<TWriter>(ref TWriter writer, SecsFormat format, uint length)
        where TWriter : IBytesWriter
    {
        byte lenCount;
        if (length <= 0xFF)
        {
            lenCount = 1;
            WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)(((byte)format << 2) | lenCount));
            WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)length);
        }
        else if (length <= 0xFFFF)
        {
            lenCount = 2;
            WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)(((byte)format << 2) | lenCount));
            WriterExtension.WriteValue<TWriter, ushort>(ref writer, (ushort)length, EndianType.Big);
        }
        else
        {
            lenCount = 3;
            WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)(((byte)format << 2) | lenCount));
            var span = writer.GetSpan(3);
            span[0] = (byte)(length >> 16);
            span[1] = (byte)(length >> 8);
            span[2] = (byte)length;
            writer.Advance(3);
        }
    }
}
