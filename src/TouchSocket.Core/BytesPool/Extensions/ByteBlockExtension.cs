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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace TouchSocket.Core;

/// <summary>
/// 提供字节块扩展方法的静态类。
/// </summary>
public static class ByteBlockExtension
{
    /// <summary>
    /// 将值类型的字节块转换为普通的字节块。
    /// </summary>
    /// <param name="valueByteBlock">要转换的值类型字节块。</param>
    /// <returns>一个新的字节块对象。</returns>
    public static ByteBlock AsByteBlock(this in ValueByteBlock valueByteBlock)
    {
        var byteBlock = new ByteBlock(valueByteBlock.TotalMemory.Slice(0, valueByteBlock.Length));
        byteBlock.Position = valueByteBlock.Position;
        byteBlock.SetLength(valueByteBlock.Length);
        return byteBlock;
    }

    public static Stream AsReadStream<TReader>(this TReader reader) where TReader : class, IByteBlockReader
    {
        return new ReadOnlyStream(reader);
    }

    /// <summary>
    /// 将字节块转换为字节块流。
    /// </summary>
    /// <param name="byteBlock">要转换的字节块。</param>
    /// <param name="releaseTogether">是否在释放字节块时一起释放关联的资源，默认为<see langword="true"/>。</param>
    /// <returns>一个新的字节块流对象。</returns>
    public static Stream AsStream(this ByteBlock byteBlock, bool releaseTogether = true)
    {
        return new ByteBlockStream(byteBlock, releaseTogether);
    }

    public static Stream AsWriteStream<TWriter>(this TWriter writer) where TWriter : class, IByteBlockWriter
    {
        return new WriteOnlyStream(writer);
    }

    #region ToArray

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="offset">起始偏移量。</param>
    /// <param name="length">要转换为数组的长度。</param>
    /// <returns>包含指定长度的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset, int length) where TByteBlock : IByteBlockCore
    {
        return byteBlock.Span.Slice(offset, length).ToArray();
    }

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组，从指定偏移量开始，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="offset">起始偏移量。</param>
    /// <returns>从指定偏移量到字节块末尾的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, offset, byteBlock.Length - offset);
    }

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组，从索引0开始，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <returns>整个字节块的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, 0, byteBlock.Length);
    }

    /// <summary>
    /// 将指定的字节块从当前位置<see cref="IBytesCore.Position"/>转换为【新】字节数组，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <returns>从当前位置到字节块末尾的【新】字节数组。</returns>
    public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockReader
    {
        return ToArray(byteBlock, byteBlock.Position, byteBlock.CanReadLength);
    }

    /// <summary>
    /// 将指定的字节块从当前位置<see cref="IByteBlock.Position"/>转换为【新】字节数组，指定长度。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="length">要转换为数组的长度。</param>
    /// <returns>从当前位置开始，指定长度的【新】字节数组。</returns>
    public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock, int length) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, byteBlock.Position, length);
    }

    #endregion ToArray

    #region Write

    public static void WriteByte(this ByteBlock byteBlock, byte value)
    {
        WriterExtension.WriteValue<ByteBlock, byte>(ref byteBlock, value);
    }

    public static void WriteDateTime(this ByteBlock byteBlock, DateTime value)
    {
        WriterExtension.WriteValue<ByteBlock, DateTime>(ref byteBlock, value);
    }

    public static void WriteDecimal(this ByteBlock byteBlock, decimal value)
    {
        WriterExtension.WriteValue<ByteBlock, decimal>(ref byteBlock, value);
    }

    public static void WriteDecimal(this ByteBlock byteBlock, decimal value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, decimal>(ref byteBlock, value, endianType);
    }

    public static void WriteDouble(this ByteBlock byteBlock, double value)
    {
        WriterExtension.WriteValue<ByteBlock, double>(ref byteBlock, value);
    }

    public static void WriteDouble(this ByteBlock byteBlock, double value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, double>(ref byteBlock, value, endianType);
    }

    public static void WriteFloat(this ByteBlock byteBlock, float value)
    {
        WriterExtension.WriteValue<ByteBlock, float>(ref byteBlock, value);
    }

    public static void WriteFloat(this ByteBlock byteBlock, float value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, float>(ref byteBlock, value, endianType);
    }

    public static void WriteGuid(this ByteBlock byteBlock, Guid value)
    {
        WriterExtension.WriteValue<ByteBlock, Guid>(ref byteBlock, value);
    }

    public static void WriteInt16(this ByteBlock byteBlock, short value)
    {
        WriterExtension.WriteValue<ByteBlock, short>(ref byteBlock, value);
    }

    public static void WriteInt16(this ByteBlock byteBlock, short value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, short>(ref byteBlock, value, endianType);
    }

    public static void WriteInt32(this ByteBlock byteBlock, int value)
    {
        WriterExtension.WriteValue<ByteBlock, int>(ref byteBlock, value);
    }

    public static void WriteInt32(this ByteBlock byteBlock, int value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, int>(ref byteBlock, value, endianType);
    }

    public static void WriteInt64(this ByteBlock byteBlock, long value)
    {
        WriterExtension.WriteValue<ByteBlock, long>(ref byteBlock, value);
    }

    public static void WriteInt64(this ByteBlock byteBlock, long value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, long>(ref byteBlock, value, endianType);
    }

    public static void WriteSByte(this ByteBlock byteBlock, sbyte value)
    {
        WriterExtension.WriteValue<ByteBlock, sbyte>(ref byteBlock, value);
    }

    public static void WriteString(this ByteBlock byteBlock, string value)
    {
        WriterExtension.WriteString(ref byteBlock, value);
    }

    public static void WriteString(this ByteBlock byteBlock, string value, FixedHeaderType headerType)
    {
        WriterExtension.WriteString(ref byteBlock, value, headerType);
    }

    public static void WriteTimeSpan(this ByteBlock byteBlock, TimeSpan value)
    {
        WriterExtension.WriteValue<ByteBlock, TimeSpan>(ref byteBlock, value);
    }

    public static void WriteUInt16(this ByteBlock byteBlock, ushort value)
    {
        WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, value);
    }

    public static void WriteUInt16(this ByteBlock byteBlock, ushort value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, value, endianType);
    }

    public static void WriteUInt32(this ByteBlock byteBlock, uint value)
    {
        WriterExtension.WriteValue<ByteBlock, uint>(ref byteBlock, value);
    }

    public static void WriteUInt32(this ByteBlock byteBlock, uint value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, uint>(ref byteBlock, value, endianType);
    }

    public static void WriteUInt64(this ByteBlock byteBlock, ulong value)
    {
        WriterExtension.WriteValue<ByteBlock, ulong>(ref byteBlock, value);
    }

    public static void WriteUInt64(this ByteBlock byteBlock, ulong value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, ulong>(ref byteBlock, value, endianType);
    }

    public static void WriteIsNull<T>(this ByteBlock byteBlock, T t)
         where T : class
    {
        WriterExtension.WriteIsNull<ByteBlock, T>(ref byteBlock, t);
    }

    public static void WriteIsNull<T>(this ByteBlock byteBlock, T? t)
        where T : struct
    {
        WriterExtension.WriteIsNull<ByteBlock, T>(ref byteBlock, t);
    }

    public static void WriteNotNull(this ByteBlock byteBlock)
    {
        WriterExtension.WriteNotNull<ByteBlock>(ref byteBlock);
    }

    public static void WriteNull(this ByteBlock byteBlock)
    {
        WriterExtension.WriteNull<ByteBlock>(ref byteBlock);
    }

    public static int WriteVarUInt32(this ByteBlock byteBlock, uint value)
    {
        return WriterExtension.WriteVarUInt32<ByteBlock>(ref byteBlock, value);
    }
    public static void WriteNormalString(this ByteBlock byteBlock, string value, Encoding encoding)
    {
        WriterExtension.WriteNormalString<ByteBlock>(ref byteBlock, value, encoding);
    }

    public static void WriteByteSpan(this ByteBlock byteBlock, scoped ReadOnlySpan<byte> span)
    {
        WriterExtension.WriteByteSpan<ByteBlock>(ref byteBlock, span);
    }

    public static void WriteByteBlock(this ByteBlock byteBlock, ByteBlock value)
    {
        WriterExtension.WriteByteBlock<ByteBlock>(ref byteBlock, value);
    }

    public static void WritePackage<TPackage>(this ByteBlock byteBlock, TPackage value)
        where TPackage : class, IPackage
    {
        WriterExtension.WritePackage(ref byteBlock, value);
    }
    #endregion Write

    #region Read

    public static byte ReadByte(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, byte>(ref byteBlock);
    }

    public static DateTime ReadDateTime(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, DateTime>(ref byteBlock);
    }

    public static decimal ReadDecimal(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, decimal>(ref byteBlock);
    }

    public static decimal ReadDecimal(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, decimal>(ref byteBlock, endianType);
    }

    public static double ReadDouble(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, double>(ref byteBlock);
    }

    public static double ReadDouble(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, double>(ref byteBlock, endianType);
    }

    public static float ReadFloat(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, float>(ref byteBlock);
    }

    public static float ReadFloat(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, float>(ref byteBlock, endianType);
    }

    public static Guid ReadGuid(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, Guid>(ref byteBlock);
    }

    public static short ReadInt16(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, short>(ref byteBlock);
    }

    public static short ReadInt16(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, short>(ref byteBlock, endianType);
    }

    public static int ReadInt32(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, int>(ref byteBlock);
    }

    public static int ReadInt32(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, int>(ref byteBlock, endianType);
    }

    public static long ReadInt64(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, long>(ref byteBlock);
    }

    public static long ReadInt64(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, long>(ref byteBlock, endianType);
    }

    public static sbyte ReadSByte(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, sbyte>(ref byteBlock);
    }

    public static string ReadString(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadString(ref byteBlock);
    }

    public static string ReadString(this ByteBlock byteBlock, FixedHeaderType headerType)
    {
        return ReaderExtension.ReadString(ref byteBlock, headerType);
    }

    public static TimeSpan ReadTimeSpan(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, TimeSpan>(ref byteBlock);
    }

    public static ushort ReadUInt16(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, ushort>(ref byteBlock);
    }

    public static ushort ReadUInt16(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, ushort>(ref byteBlock, endianType);
    }

    public static uint ReadUInt32(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, uint>(ref byteBlock);
    }

    public static uint ReadUInt32(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, uint>(ref byteBlock, endianType);
    }

    public static ulong ReadUInt64(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, ulong>(ref byteBlock);
    }

    public static ulong ReadUInt64(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, ulong>(ref byteBlock, endianType);
    }

    public static bool ReadIsNull(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadIsNull<ByteBlock>(ref byteBlock);
    }

    public static uint ReadVarUInt32(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadVarUInt32<ByteBlock>(ref byteBlock);
    }

    public static ByteBlock ReadByteBlock(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadByteBlock(ref byteBlock);
    }
    public static TPackage ReadPackage<TPackage>(this ByteBlock byteBlock)
        where TPackage : class, IPackage, new()
    {
        return ReaderExtension.ReadPackage<ByteBlock, TPackage>(ref byteBlock);
    }
    #endregion Read
}