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
using System.Buffers;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 提供了与TouchSocket库相关的字节序列和对象之间的转换功能。
/// </summary>
public sealed class TouchSocketBitConverter
{
    /// <summary>
    /// 以大端
    /// </summary>
    public static readonly TouchSocketBitConverter BigEndian;

    /// <summary>
    /// 以交换大端
    /// </summary>
    public static readonly TouchSocketBitConverter BigSwapEndian;

    /// <summary>
    /// 以小端
    /// </summary>
    public static readonly TouchSocketBitConverter LittleEndian;

    /// <summary>
    /// 以交换小端
    /// </summary>
    public static readonly TouchSocketBitConverter LittleSwapEndian;

    private static EndianType s_defaultEndianType;
    private readonly EndianType m_endianType;

    static TouchSocketBitConverter()
    {
        BigEndian = new TouchSocketBitConverter(EndianType.Big);
        LittleEndian = new TouchSocketBitConverter(EndianType.Little);
        BigSwapEndian = new TouchSocketBitConverter(EndianType.BigSwap);
        LittleSwapEndian = new TouchSocketBitConverter(EndianType.LittleSwap);
        DefaultEndianType = EndianType.Little;
    }

    /// <summary>
    /// 初始化 TouchSocketBitConverter 类的新实例。
    /// </summary>
    /// <param name="endianType">指定字节序类型，可以是大端字节序或小端字节序。</param>
    public TouchSocketBitConverter(EndianType endianType)
    {
        this.m_endianType = endianType;
    }

    /// <summary>
    /// 以默认小端，可通过<see cref="TouchSocketBitConverter.DefaultEndianType"/>重新指定默认端。
    /// </summary>
    public static TouchSocketBitConverter Default { get; private set; }

    /// <summary>
    /// 默认大小端切换。
    /// </summary>
    public static EndianType DefaultEndianType
    {
        get => s_defaultEndianType;
        set
        {
            s_defaultEndianType = value;
            switch (value)
            {
                case EndianType.Little:
                    Default = LittleEndian;
                    break;

                case EndianType.Big:
                    Default = BigEndian;
                    break;

                case EndianType.LittleSwap:
                    Default = LittleSwapEndian;
                    break;

                case EndianType.BigSwap:
                    Default = BigSwapEndian;
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 获取字节序类型
    /// </summary>
    public EndianType EndianType => this.m_endianType;

    /// <summary>
    /// 根据字节序类型获取相应的字节交换器
    /// </summary>
    /// <param name="endianType">字节序类型</param>
    /// <returns>对应的字节交换器</returns>
    /// <exception cref="InvalidOperationException">当字节序类型不支持时抛出</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TouchSocketBitConverter GetBitConverter(EndianType endianType)
    {
        switch (endianType)
        {
            case EndianType.Little: return LittleEndian;

            case EndianType.Big: return BigEndian;

            case EndianType.LittleSwap: return LittleSwapEndian;

            case EndianType.BigSwap: return BigSwapEndian;

            default:
                ThrowHelper.ThrowInvalidEnumArgumentException(endianType);
                return default;
        }
    }

    /// <summary>
    /// 获取指定值的字节表示形式，返回只读内存。转换时会考虑当前实例的字节序设置。
    /// </summary>
    /// <typeparam name="T">要转换的值的类型，必须是非托管类型。</typeparam>
    /// <param name="value">要转换为字节数组的值。</param>
    /// <returns>表示该值的字节只读内存。</returns>
    public ReadOnlyMemory<byte> GetBytes<T>(T value) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        var bytes = new byte[size];
        this.WriteBytes(bytes, value);
        return bytes;
    }

    /// <summary>
    /// 判断当前字节序是否与系统字节序相同
    /// </summary>
    /// <returns>如果字节序相同返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSameOfSet()
    {
        return !(BitConverter.IsLittleEndian ^ (this.EndianType == EndianType.Little));
    }

    /// <summary>
    /// 将字节跨度转换为指定类型
    /// </summary>
    /// <typeparam name="T">要转换成的类型</typeparam>
    /// <param name="span">要转换的字节跨度</param>
    /// <returns>转换后的值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当字节跨度长度不足以表示类型T时抛出</exception>
    /// <exception cref="NotSupportedException">当类型T不支持时抛出</exception>
    public unsafe T To<T>(ReadOnlySpan<byte> span) where T : unmanaged
    {
        var size = sizeof(T);
        if (span.Length < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(span.Length), span.Length, size);
        }
        fixed (byte* p = &span[0])
        {
            if (this.IsSameOfSet()||size==1)
            {
                return Unsafe.Read<T>(p);
            }
            else
            {
                if (size == 2)
                {
                    this.ByteTransDataFormat2_Net6(p);
                    var v = Unsafe.Read<T>(p);
                    this.ByteTransDataFormat2_Net6(p);
                    return v;
                }
                else if (size == 4)
                {
                    this.ByteTransDataFormat4_Net6(p);
                    var v = Unsafe.Read<T>(p);
                    this.ByteTransDataFormat4_Net6(p);
                    return v;
                }
                else if (size == 8)
                {
                    this.ByteTransDataFormat8_Net6(p);
                    var v = Unsafe.Read<T>(p);
                    this.ByteTransDataFormat8_Net6(p);
                    return v;
                }
                else if (size == 16)
                {
                    this.ByteTransDataFormat16_Net6(p);
                    var v = Unsafe.Read<T>(p);
                    this.ByteTransDataFormat16_Net6(p);
                    return v;
                }
                else
                {
                    ThrowHelper.ThrowNotSupportedException(size.ToString());
                    return default;
                }
            }
        }
    }

    /// <summary>
    /// 将指定值的字节表示形式写入到指定的字节跨度中。
    /// </summary>
    /// <typeparam name="T">要写入的值的类型，必须是非托管类型。</typeparam>
    /// <param name="span">要写入字节的目标跨度。</param>
    /// <param name="value">要写入的值。</param>
    /// <returns>写入的字节数。</returns>
    public unsafe int WriteBytes<T>(Span<byte> span, T value) where T : unmanaged
    {
        var size = sizeof(T);
        if (span.Length < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(span.Length), span.Length, size);
        }
        Unsafe.As<byte, T>(ref span[0]) = value;

        if (size == 1)
        {
            // 对于单字节类型，不需要转换
            return size;
        }

        if (!this.IsSameOfSet())
        {
            if (size == 2)
            {
                this.ByteTransDataFormat2_Net6(ref span[0]);
            }
            else if (size == 4)
            {
                this.ByteTransDataFormat4_Net6(ref span[0]);
            }
            else if (size == 8)
            {
                this.ByteTransDataFormat8_Net6(ref span[0]);
            }
            else if (size == 16)
            {
                this.ByteTransDataFormat16_Net6(ref span[0]);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException(size.ToString());
            }
        }

        return size;
    }

    #region Tool

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat16_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                switch (this.m_endianType)
                {
                    case EndianType.Big:
                        var span = new Span<byte>(p, 16);
                        span.Reverse();
                        break;

                    case EndianType.Little:
                        return;

                    default:
                    case EndianType.LittleSwap:
                    case EndianType.BigSwap:
                        throw new NotSupportedException();
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ByteTransDataFormat16_Net6(byte* p)
    {
        switch (this.m_endianType)
        {
            case EndianType.Big:
                var span = new Span<byte>(p, 16);
                span.Reverse();
                break;

            case EndianType.Little:
                return;

            default:
            case EndianType.LittleSwap:
            case EndianType.BigSwap:
                throw new NotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat2_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                Unsafe.WriteUnaligned(p, b);
                Unsafe.WriteUnaligned(p + 1, a);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ByteTransDataFormat2_Net6(byte* p)
    {
        var a = Unsafe.ReadUnaligned<byte>(p);
        var b = Unsafe.ReadUnaligned<byte>(p + 1);
        Unsafe.WriteUnaligned(p, b);
        Unsafe.WriteUnaligned(p + 1, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] ByteTransDataFormat4(byte[] value, int offset)
    {
        var numArray = new byte[4];
        switch (this.m_endianType)
        {
            case EndianType.Big:
                numArray[0] = value[offset + 3];
                numArray[1] = value[offset + 2];
                numArray[2] = value[offset + 1];
                numArray[3] = value[offset];
                break;

            case EndianType.BigSwap:
                numArray[0] = value[offset + 2];
                numArray[1] = value[offset + 3];
                numArray[2] = value[offset];
                numArray[3] = value[offset + 1];
                break;

            case EndianType.LittleSwap:
                numArray[0] = value[offset + 1];
                numArray[1] = value[offset];
                numArray[2] = value[offset + 3];
                numArray[3] = value[offset + 2];
                break;

            case EndianType.Little:
                numArray[0] = value[offset];
                numArray[1] = value[offset + 1];
                numArray[2] = value[offset + 2];
                numArray[3] = value[offset + 3];
                break;
        }
        return numArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat4_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                var c = Unsafe.ReadUnaligned<byte>(p + 2);
                var d = Unsafe.ReadUnaligned<byte>(p + 3);

                switch (this.m_endianType)
                {
                    case EndianType.Big:
                        Unsafe.WriteUnaligned(p, d);
                        Unsafe.WriteUnaligned(p + 1, c);
                        Unsafe.WriteUnaligned(p + 2, b);
                        Unsafe.WriteUnaligned(p + 3, a);
                        break;

                    case EndianType.BigSwap:
                        Unsafe.WriteUnaligned(p, c);
                        Unsafe.WriteUnaligned(p + 1, d);
                        Unsafe.WriteUnaligned(p + 2, a);
                        Unsafe.WriteUnaligned(p + 3, b);
                        break;

                    case EndianType.LittleSwap:
                        Unsafe.WriteUnaligned(p, b);
                        Unsafe.WriteUnaligned(p + 1, a);
                        Unsafe.WriteUnaligned(p + 2, d);
                        Unsafe.WriteUnaligned(p + 3, c);
                        break;

                    case EndianType.Little:
                        return;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ByteTransDataFormat4_Net6(byte* p)
    {
        var a = Unsafe.ReadUnaligned<byte>(p);
        var b = Unsafe.ReadUnaligned<byte>(p + 1);
        var c = Unsafe.ReadUnaligned<byte>(p + 2);
        var d = Unsafe.ReadUnaligned<byte>(p + 3);

        switch (this.m_endianType)
        {
            case EndianType.Big:
                Unsafe.WriteUnaligned(p, d);
                Unsafe.WriteUnaligned(p + 1, c);
                Unsafe.WriteUnaligned(p + 2, b);
                Unsafe.WriteUnaligned(p + 3, a);
                break;

            case EndianType.BigSwap:
                Unsafe.WriteUnaligned(p, c);
                Unsafe.WriteUnaligned(p + 1, d);
                Unsafe.WriteUnaligned(p + 2, a);
                Unsafe.WriteUnaligned(p + 3, b);
                break;

            case EndianType.LittleSwap:
                Unsafe.WriteUnaligned(p, b);
                Unsafe.WriteUnaligned(p + 1, a);
                Unsafe.WriteUnaligned(p + 2, d);
                Unsafe.WriteUnaligned(p + 3, c);
                break;

            case EndianType.Little:
                return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] ByteTransDataFormat8(byte[] value, int offset)
    {
        var numArray = new byte[8];
        switch (this.m_endianType)
        {
            case EndianType.Big:
                numArray[0] = value[offset + 7];
                numArray[1] = value[offset + 6];
                numArray[2] = value[offset + 5];
                numArray[3] = value[offset + 4];
                numArray[4] = value[offset + 3];
                numArray[5] = value[offset + 2];
                numArray[6] = value[offset + 1];
                numArray[7] = value[offset];
                break;

            case EndianType.BigSwap:
                numArray[0] = value[offset + 6];
                numArray[1] = value[offset + 7];
                numArray[2] = value[offset + 4];
                numArray[3] = value[offset + 5];
                numArray[4] = value[offset + 2];
                numArray[5] = value[offset + 3];
                numArray[6] = value[offset];
                numArray[7] = value[offset + 1];
                break;

            case EndianType.LittleSwap:
                numArray[0] = value[offset + 1];
                numArray[1] = value[offset];
                numArray[2] = value[offset + 3];
                numArray[3] = value[offset + 2];
                numArray[4] = value[offset + 5];
                numArray[5] = value[offset + 4];
                numArray[6] = value[offset + 7];
                numArray[7] = value[offset + 6];
                break;

            case EndianType.Little:
                numArray[0] = value[offset];
                numArray[1] = value[offset + 1];
                numArray[2] = value[offset + 2];
                numArray[3] = value[offset + 3];
                numArray[4] = value[offset + 4];
                numArray[5] = value[offset + 5];
                numArray[6] = value[offset + 6];
                numArray[7] = value[offset + 7];
                break;
        }
        return numArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat8_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                var c = Unsafe.ReadUnaligned<byte>(p + 2);
                var d = Unsafe.ReadUnaligned<byte>(p + 3);
                var e = Unsafe.ReadUnaligned<byte>(p + 4);
                var f = Unsafe.ReadUnaligned<byte>(p + 5);
                var g = Unsafe.ReadUnaligned<byte>(p + 6);
                var h = Unsafe.ReadUnaligned<byte>(p + 7);

                switch (this.m_endianType)
                {
                    case EndianType.Big:
                        Unsafe.WriteUnaligned(p, h);
                        Unsafe.WriteUnaligned(p + 1, g);
                        Unsafe.WriteUnaligned(p + 2, f);
                        Unsafe.WriteUnaligned(p + 3, e);
                        Unsafe.WriteUnaligned(p + 4, d);
                        Unsafe.WriteUnaligned(p + 5, c);
                        Unsafe.WriteUnaligned(p + 6, b);
                        Unsafe.WriteUnaligned(p + 7, a);
                        break;

                    case EndianType.BigSwap:
                        Unsafe.WriteUnaligned(p, g);
                        Unsafe.WriteUnaligned(p + 1, h);
                        Unsafe.WriteUnaligned(p + 2, e);
                        Unsafe.WriteUnaligned(p + 3, f);
                        Unsafe.WriteUnaligned(p + 4, c);
                        Unsafe.WriteUnaligned(p + 5, d);
                        Unsafe.WriteUnaligned(p + 6, a);
                        Unsafe.WriteUnaligned(p + 7, b);
                        break;

                    case EndianType.LittleSwap:
                        Unsafe.WriteUnaligned(p, b);
                        Unsafe.WriteUnaligned(p + 1, a);
                        Unsafe.WriteUnaligned(p + 2, d);
                        Unsafe.WriteUnaligned(p + 3, c);
                        Unsafe.WriteUnaligned(p + 4, f);
                        Unsafe.WriteUnaligned(p + 5, e);
                        Unsafe.WriteUnaligned(p + 6, h);
                        Unsafe.WriteUnaligned(p + 7, g);
                        break;

                    case EndianType.Little:
                        break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ByteTransDataFormat8_Net6(byte* p)
    {
        var a = Unsafe.ReadUnaligned<byte>(p);
        var b = Unsafe.ReadUnaligned<byte>(p + 1);
        var c = Unsafe.ReadUnaligned<byte>(p + 2);
        var d = Unsafe.ReadUnaligned<byte>(p + 3);
        var e = Unsafe.ReadUnaligned<byte>(p + 4);
        var f = Unsafe.ReadUnaligned<byte>(p + 5);
        var g = Unsafe.ReadUnaligned<byte>(p + 6);
        var h = Unsafe.ReadUnaligned<byte>(p + 7);

        switch (this.m_endianType)
        {
            case EndianType.Big:
                Unsafe.WriteUnaligned(p, h);
                Unsafe.WriteUnaligned(p + 1, g);
                Unsafe.WriteUnaligned(p + 2, f);
                Unsafe.WriteUnaligned(p + 3, e);
                Unsafe.WriteUnaligned(p + 4, d);
                Unsafe.WriteUnaligned(p + 5, c);
                Unsafe.WriteUnaligned(p + 6, b);
                Unsafe.WriteUnaligned(p + 7, a);
                break;

            case EndianType.BigSwap:
                Unsafe.WriteUnaligned(p, g);
                Unsafe.WriteUnaligned(p + 1, h);
                Unsafe.WriteUnaligned(p + 2, e);
                Unsafe.WriteUnaligned(p + 3, f);
                Unsafe.WriteUnaligned(p + 4, c);
                Unsafe.WriteUnaligned(p + 5, d);
                Unsafe.WriteUnaligned(p + 6, a);
                Unsafe.WriteUnaligned(p + 7, b);
                break;

            case EndianType.LittleSwap:
                Unsafe.WriteUnaligned(p, b);
                Unsafe.WriteUnaligned(p + 1, a);
                Unsafe.WriteUnaligned(p + 2, d);
                Unsafe.WriteUnaligned(p + 3, c);
                Unsafe.WriteUnaligned(p + 4, f);
                Unsafe.WriteUnaligned(p + 5, e);
                Unsafe.WriteUnaligned(p + 6, h);
                Unsafe.WriteUnaligned(p + 7, g);
                break;

            case EndianType.Little:
                break;
        }
    }

    #endregion Tool

    #region Convert

    /// <summary>
    /// 将指定类型的只读源数据批量转换为目标类型的只读内存，转换时会考虑默认字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="TSource">源类型，必须是非托管类型。</typeparam>
    /// <typeparam name="TTarget">目标类型，必须是非托管类型。</typeparam>
    /// <param name="sourceSpan">要转换的源数据只读跨度。</param>
    /// <returns>转换后的目标类型只读内存。</returns>
    public static ReadOnlyMemory<TTarget> ConvertValues<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan)
          where TSource : unmanaged
          where TTarget : unmanaged
    {
        return ConvertValues<TSource, TTarget>(sourceSpan, s_defaultEndianType);
    }

    /// <summary>
    /// 将指定类型的只读源数据批量转换为目标类型的只读内存，转换时会考虑指定字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="TSource">源类型，必须是非托管类型。</typeparam>
    /// <typeparam name="TTarget">目标类型，必须是非托管类型。</typeparam>
    /// <param name="sourceSpan">要转换的源数据只读跨度。</param>
    /// <param name="endianType">指定的字节序类型。</param>
    /// <returns>转换后的目标类型只读内存。</returns>
    public static ReadOnlyMemory<TTarget> ConvertValues<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan, EndianType endianType)
           where TSource : unmanaged
           where TTarget : unmanaged
    {
        return ConvertValues<TSource, TTarget>(sourceSpan, endianType, endianType);
    }

    /// <summary>
    /// 将指定类型的只读源数据批量转换为目标类型的只读内存，转换时会考虑源字节序和目标字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="TSource">源类型，必须是非托管类型。</typeparam>
    /// <typeparam name="TTarget">目标类型，必须是非托管类型。</typeparam>
    /// <param name="sourceSpan">要转换的源数据只读跨度。</param>
    /// <param name="sourceEndianType">源数据的字节序类型。</param>
    /// <param name="targetEndianType">目标数据的字节序类型。</param>
    /// <returns>转换后的目标类型只读内存。</returns>
    public static ReadOnlyMemory<TTarget> ConvertValues<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan, EndianType sourceEndianType, EndianType targetEndianType)
           where TSource : unmanaged
           where TTarget : unmanaged
    {
        var length = GetConvertedLength<TSource, TTarget>(sourceSpan.Length);
        Memory<TTarget> target = new TTarget[length];
        ConvertValues(sourceSpan, target.Span, sourceEndianType, targetEndianType);
        return target;
    }

    /// <summary>
    /// 将指定类型的只读源数据批量转换为目标类型的可写跨度，转换时会考虑默认字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="TSource">源类型，必须是非托管类型。</typeparam>
    /// <typeparam name="TTarget">目标类型，必须是非托管类型。</typeparam>
    /// <param name="sourceSpan">要转换的源数据只读跨度。</param>
    /// <param name="targetSpan">转换后的目标类型可写跨度。</param>
    /// <returns>实际转换的目标类型元素数量。</returns>
    public static int ConvertValues<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan, Span<TTarget> targetSpan)
           where TSource : unmanaged
           where TTarget : unmanaged
    {
        return ConvertValues(sourceSpan, targetSpan, s_defaultEndianType);
    }

    /// <summary>
    /// 将指定类型的只读源数据批量转换为目标类型的可写跨度，转换时会考虑指定字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="TSource">源类型，必须是非托管类型。</typeparam>
    /// <typeparam name="TTarget">目标类型，必须是非托管类型。</typeparam>
    /// <param name="sourceSpan">要转换的源数据只读跨度。</param>
    /// <param name="targetSpan">转换后的目标类型可写跨度。</param>
    /// <param name="endianType">指定的字节序类型。</param>
    /// <returns>实际转换的目标类型元素数量。</returns>
    public static int ConvertValues<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan, Span<TTarget> targetSpan, EndianType endianType)
           where TSource : unmanaged
           where TTarget : unmanaged
    {
        return ConvertValues(sourceSpan, targetSpan, endianType, endianType);
    }

    /// <summary>
    /// 将指定类型的只读源数据批量转换为目标类型的可写跨度，转换时会考虑源字节序和目标字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="TSource">源类型，必须是非托管类型。</typeparam>
    /// <typeparam name="TTarget">目标类型，必须是非托管类型。</typeparam>
    /// <param name="sourceSpan">要转换的源数据只读跨度。</param>
    /// <param name="targetSpan">转换后的目标类型可写跨度。</param>
    /// <param name="sourceEndianType">源数据的字节序类型。</param>
    /// <param name="targetEndianType">目标数据的字节序类型。</param>
    /// <returns>实际转换的目标类型元素数量。</returns>
    public static int ConvertValues<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan, Span<TTarget> targetSpan, EndianType sourceEndianType, EndianType targetEndianType)
           where TSource : unmanaged
           where TTarget : unmanaged
    {
        var sourceConverter = GetBitConverter(sourceEndianType);
        var targetConverter = GetBitConverter(targetEndianType);

        int length;
        // 如果源类型和目标类型相同，直接转换
        if (typeof(TSource) == typeof(TTarget))
        {
            length = Math.Min(sourceSpan.Length, targetSpan.Length);
            Span<byte> span = stackalloc byte[Unsafe.SizeOf<TSource>()];
            for (var i = 0; i < length; i++)
            {
                sourceConverter.WriteBytes(span, sourceSpan[i]);
                targetSpan[i] = targetConverter.To<TTarget>(span);
            }

            return length;
        }

        // 处理bool作为源类型的情况（按比特处理）
        if (typeof(TSource) == typeof(bool))
        {
            return ConvertFromBool(sourceSpan, targetSpan, targetConverter);
        }

        // 处理bool作为目标类型的情况（按比特处理）
        if (typeof(TTarget) == typeof(bool))
        {
            return ConvertToBool(sourceSpan, targetSpan, sourceConverter);
        }

        // 处理其他非bool类型的情况
        var sourceSize = Unsafe.SizeOf<TSource>();
        var targetSize = Unsafe.SizeOf<TTarget>();
        var sourceBytes = sourceSpan.Length * sourceSize;
        var targetBytes = targetSpan.Length * targetSize;
        var bytesToConvert = Math.Min(sourceBytes, targetBytes);

        length = bytesToConvert / targetSize;

        var buffer = ArrayPool<byte>.Shared.Rent(bytesToConvert);
        try
        {
            var writerSpan = new Span<byte>(buffer);

            for (var i = 0; i < bytesToConvert / sourceSize; i++)
            {
                var size = sourceConverter.WriteBytes(writerSpan, sourceSpan[i]);
                writerSpan = writerSpan.Slice(size);
            }
            var span = new ReadOnlySpan<byte>(buffer);
            for (var i = 0; i < length; i++)
            {
                targetSpan[i] = targetConverter.To<TTarget>(span);
                span = span.Slice(targetSize);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return length;
    }

    /// <summary>
    /// 计算从源类型到目标类型的转换长度。
    /// <para>
    /// 注意：<see langword="bool"/>会被视为1位，即1/8字节，而其他非托管类型会按字节计算。
    /// </para>
    /// </summary>
    /// <typeparam name="TSource">源类型，必须是非托管类型。</typeparam>
    /// <typeparam name="TTarget">目标类型，必须是非托管类型。</typeparam>
    /// <param name="length">源类型的元素数量。</param>
    /// <returns>目标类型的元素数量。</returns>
    /// <exception cref="InvalidOperationException">当目标类型的大小为零时抛出。</exception>
    /// <exception cref="ArgumentException">当源类型的比特数不能被目标类型的比特数整除时抛出。</exception>
    /// <exception cref="OverflowException">当转换后的长度超过 <see cref="int.MaxValue"/> 时抛出。</exception>
    public static int GetConvertedLength<TSource, TTarget>(int length)
       where TSource : unmanaged
       where TTarget : unmanaged
    {
        // 计算源类型的比特总数
        long sourceBits;
        if (typeof(TSource) == typeof(bool))
        {
            // bool类型的每个元素代表1位
            sourceBits = length;
        }
        else
        {
            // 其他类型按字节计算
            var sizeT1 = Unsafe.SizeOf<TSource>();
            sourceBits = (long)length * sizeT1 * 8;
        }

        // 计算目标类型的比特占用
        long targetBitSize;
        if (typeof(TTarget) == typeof(bool))
        {
            // bool类型每个元素代表1位
            targetBitSize = 1;
        }
        else
        {
            // 其他类型按字节计算
            targetBitSize = (long)Unsafe.SizeOf<TTarget>() * 8;
        }

        // 特殊情况处理
        if (sourceBits == 0)
        {
            return 0;
        }

        if (targetBitSize == 0)
        {
            throw new InvalidOperationException("Target type cannot have zero size");
        }

        // 计算目标长度并检查溢出

        var resultLength = (sourceBits + (targetBitSize - 1)) / targetBitSize;
        return resultLength > int.MaxValue
            ? throw new OverflowException($"Converted length ({resultLength}) exceeds maximum Span size")
            : (int)resultLength;
    }

    /// <summary>
    /// 从bool源类型转换
    /// </summary>
    private static int ConvertFromBool<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan, Span<TTarget> targetSpan, TouchSocketBitConverter converter)
        where TSource : unmanaged
        where TTarget : unmanaged
    {
        var targetSize = Unsafe.SizeOf<TTarget>();

        var byteCount = (sourceSpan.Length + 7) / 8;

        Span<byte> bytes = stackalloc byte[targetSize];
        var sourceIndex = 0;
        var length = Math.Min(byteCount / targetSize, targetSpan.Length);
        for (var i = 0; i < length; i++)
        {
            for (var j = 0; j < targetSize; j++)
            {
                byte value = 0;
                TSource source;

                for (var k = 7; k >= 0; k--)
                {
                    if (sourceIndex < sourceSpan.Length)
                    {
                        source = sourceSpan[sourceIndex++];
                        value = value.SetBit(k, Unsafe.As<TSource, bool>(ref source));
                    }
                    else
                    {
                        value = value.SetBit(k, false);
                    }
                }

                bytes[j] = value;
            }
            var target = converter.To<TTarget>(bytes);
            targetSpan[i] = target;
        }

        return length;
    }

    /// <summary>
    /// 转换为bool目标类型
    /// </summary>
    private static int ConvertToBool<TSource, TTarget>(ReadOnlySpan<TSource> sourceSpan, Span<TTarget> targetSpan, TouchSocketBitConverter converter)
        where TSource : unmanaged
        where TTarget : unmanaged
    {
        var sourceSize = Unsafe.SizeOf<TSource>();
        var length = Math.Min(sourceSize * sourceSpan.Length, targetSpan.Length / 8);

        Span<byte> bytes = stackalloc byte[sourceSize];

        var targetIndex = 0;

        for (var i = 0; i < length; i++)
        {
            var source = sourceSpan[i];
            converter.WriteBytes(bytes, source);

            for (var j = 0; j < bytes.Length; j++)
            {
                var sourceByte = bytes[j];

                for (var bit = 7; bit >= 0; bit--)
                {
                    var b = sourceByte.GetBit(bit);
                    targetSpan[targetIndex] = Unsafe.As<bool, TTarget>(ref b);
                    targetIndex++;
                }
            }
        }
        return targetIndex;
    }

    #endregion Convert

    #region ToValues

    /// <summary>
    /// 将指定的字节跨度批量转换为目标类型的只读内存，转换时会考虑指定字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="T">目标类型，必须是非托管类型。</typeparam>
    /// <param name="span">要转换的字节跨度。</param>
    /// <param name="endianType">指定的字节序类型。</param>
    /// <returns>转换后的目标类型只读内存。</returns>
    public static ReadOnlyMemory<T> ToValues<T>(ReadOnlySpan<byte> span, EndianType endianType)
       where T : unmanaged
    {
        var length = GetConvertedLength<byte, T>(span.Length);
        if (length == 0)
        {
            return ReadOnlyMemory<T>.Empty;
        }

        Memory<T> target = new T[length];
        ToValues(span, target.Span, endianType);
        return target;
    }

    /// <summary>
    /// 将指定的字节跨度批量转换为目标类型的可写跨度，转换时会考虑指定字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="T">目标类型，必须是非托管类型。</typeparam>
    /// <param name="span">要转换的字节跨度。</param>
    /// <param name="targetSpan">转换后的目标类型可写跨度。</param>
    /// <param name="endianType">指定的字节序类型。</param>
    /// <returns>实际转换的目标类型元素数量。</returns>
    public static int ToValues<T>(ReadOnlySpan<byte> span, Span<T> targetSpan, EndianType endianType)
        where T : unmanaged
    {
        return ConvertValues<byte, T>(span, targetSpan, endianType, endianType);
    }

    /// <summary>
    /// 将指定的字节跨度批量转换为目标类型的可写跨度，转换时会考虑默认字节序。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="T">目标类型，必须是非托管类型。</typeparam>
    /// <param name="span">要转换的字节跨度。</param>
    /// <param name="targetSpan">转换后的目标类型可写跨度。</param>
    /// <returns>实际转换的目标类型元素数量。</returns>
    public static int ToValues<T>(ReadOnlySpan<byte> span, Span<T> targetSpan)
        where T : unmanaged
    {
        return ToValues<T>(span, targetSpan, s_defaultEndianType);
    }

    /// <summary>
    /// 将指定的字节跨度批量转换为目标类型的只读内存，转换时会考虑当前实例的字节序设置。
    /// <para>
    /// 支持所有非托管类型，包括 <see langword="bool"/>，其中 <see langword="bool"/> 会按位处理。
    /// </para>
    /// </summary>
    /// <typeparam name="T">目标类型，必须是非托管类型。</typeparam>
    /// <param name="span">要转换的字节跨度。</param>
    /// <returns>转换后的目标类型只读内存。</returns>
    public ReadOnlyMemory<T> ToValues<T>(ReadOnlySpan<byte> span)
                where T : unmanaged
    {
        return ToValues<T>(span, this.m_endianType);
    }

    #endregion ToValues
}