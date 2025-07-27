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

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace TouchSocket.Core;

public static partial class ReaderExtension
{
    public static void SeekToStart<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        reader.BytesRead = 0;
    }
    public static void SeekToEnd<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        reader.BytesRead = reader.TotalSequence.Length;
    }
    public static long IndexOf<TReader>(ref TReader reader, ReadOnlySpan<byte> span)
        where TReader : IBytesReader
    {
        var sequence = reader.TotalSequence.Slice(reader.BytesRead);
        return sequence.IndexOf(span);
    }

    public static ByteBlock ReadByteBlock<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var len = (int)ReadVarUInt32(ref reader) - 1;

        if (len < 0)
        {
            return default;
        }

        var byteBlock = new ByteBlock(len);
        byteBlock.Write(reader.GetSpan(len).Slice(0, len));
        byteBlock.SeekToStart();
        reader.Advance(len);
        return byteBlock;
    }

    public static ReadOnlySpan<byte> ReadByteSpan<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var length = (int)ReadVarUInt32(ref reader);
        var span = reader.GetSpan(length).Slice(0, length);
        reader.Advance(length);
        return span;
    }

    public static bool ReadIsNull<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var status = ReadValue<TReader, byte>(ref reader);
        return status == 0 || (status == 1 ? false : throw new Exception("标识既非Null，也非NotNull，可能是流位置发生了错误。"));
    }

    public static TPackage ReadPackage<TReader, TPackage>(ref TReader reader)
        where TReader : IBytesReader
        where TPackage : class, IPackage, new()
    {
        if (ReadIsNull(ref reader))
        {
            return default;
        }
        else
        {
            var package = new TPackage();
            package.Unpackage(ref reader);
            return package;
        }
    }

    public static string ReadString<TReader>(ref TReader reader, FixedHeaderType headerType = FixedHeaderType.Int)
        where TReader : IBytesReader
    {
        int len;
        switch (headerType)
        {
            case FixedHeaderType.Byte:
                len = ReadValue<TReader, byte>(ref reader);
                if (len == byte.MaxValue)
                {
                    return null;
                }
                break;

            case FixedHeaderType.Ushort:
                len = ReadValue<TReader, ushort>(ref reader);
                if (len == ushort.MaxValue)
                {
                    return null;
                }
                break;

            case FixedHeaderType.Int:
            default:
                len = ReadValue<TReader, int>(ref reader);
                if (len == int.MaxValue)
                {
                    return null;
                }
                break;
        }

        var span = reader.GetSpan(len).Slice(0, len);
        var str = span.ToString(Encoding.UTF8);
        reader.Advance(len);
        return str;
    }

    public static ReadOnlySpan<byte> ReadToSpan<TReader>(ref TReader reader, int length)
                            where TReader : IBytesReader
    {
        var span = reader.GetSpan(length).Slice(0, length);
        reader.Advance(length);
        return span;
    }

    public static T ReadValue<TReader, T>(ref TReader reader, EndianType endianType)
        where T : unmanaged
        where TReader : IBytesReader
    {
        var size = Unsafe.SizeOf<T>();
        var span = reader.GetSpan(size);
        var value = TouchSocketBitConverter.GetBitConverter(endianType).To<T>(span);
        reader.Advance(size);
        return value;
    }

    public static T ReadValue<TReader, T>(ref TReader reader)
        where T : unmanaged
         where TReader : IBytesReader
    {
        var size = Unsafe.SizeOf<T>();
        var span = reader.GetSpan(size);
        var value = TouchSocketBitConverter.Default.To<T>(span);
        reader.Advance(size);
        return value;
    }

    public static uint ReadVarUInt32<TReader>(ref TReader reader)
                where TReader : IBytesReader
    {
        uint value = 0;
        var byteLength = 0;
        while (true)
        {
            var b = ReadValue<TReader, byte>(ref reader);
            var temp = (b & 0x7F); //取每个字节的后7位
            temp <<= (7 * byteLength); //向左移位，越是后面的字节，移位越多
            value += (uint)temp; //把每个字节的值加起来就是最终的值了
            byteLength++;
            if (b <= 0x7F)
            { //127=0x7F=0b01111111，小于等于说明msb=0，即最后一个字节
                break;
            }
        }
        return value;
    }
}