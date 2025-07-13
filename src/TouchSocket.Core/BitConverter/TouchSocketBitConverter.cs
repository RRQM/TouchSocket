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
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 提供了与TouchSocket库相关的字节序列和对象之间的转换功能。
/// </summary>
public sealed partial class TouchSocketBitConverter
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
            if (this.IsSameOfSet())
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
    /// 不安全地将字节引用转换为指定类型
    /// </summary>
    /// <typeparam name="T">要转换成的类型</typeparam>
    /// <param name="source">要转换的字节引用</param>
    /// <returns>转换后的值</returns>
    /// <exception cref="NotSupportedException">当类型T不支持时抛出</exception>
    public unsafe T UnsafeTo<T>(ref byte source) where T : unmanaged
    {
        var size = sizeof(T);

        fixed (byte* p = &source)
        {
            if (this.IsSameOfSet())
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
    /// 不安全地将值直接写入字节数组中。
    /// </summary>
    /// <typeparam name="T">要写入的值的类型，必须是值类型。</typeparam>
    /// <param name="source">指向字节数组的引用，从这里开始写入。</param>
    /// <param name="value">要写入的值。</param>
    /// <returns>写入的字节数。</returns>
    /// <remarks>
    /// 此方法用于在字节数组中直接插入结构，主要用于性能考虑。
    /// 它绕过了C#的类型安全性，因此使用时必须确保T是固定大小的值类型。
    /// 如果数据格式不匹配或大小不支持，将抛出异常。
    /// </remarks>
    public unsafe int UnsafeWriteBytes<T>(ref byte source, T value) where T : unmanaged
    {
        // 获取要写入的值的类型大小。
        var size = sizeof(T);

        // 直接将值写入字节数组中。
        Unsafe.As<byte, T>(ref source) = value;

        // 如果当前数据格式不需要转换，则直接返回。
        if (!this.IsSameOfSet())
        {
            // 根据值的大小选择不同的字节转换方法。
            if (size == 2)
            {
                this.ByteTransDataFormat2_Net6(ref source);
            }
            else if (size == 4)
            {
                this.ByteTransDataFormat4_Net6(ref source);
            }
            else if (size == 8)
            {
                this.ByteTransDataFormat8_Net6(ref source);
            }
            else if (size == 16)
            {
                this.ByteTransDataFormat16_Net6(ref source);
            }
            else
            {
                // 如果类型大小不支持，则抛出异常。
                ThrowHelper.ThrowNotSupportedException(size.ToString());
            }
        }

        // 返回写入的字节数。
        return size;
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

    #region ushort

    /// <summary>
    /// 将ushort类型值转换为字节数组。
    /// </summary>
    /// <param name="value">要转换的ushort类型值。</param>
    /// <returns>转换后的字节数组。</returns>
    public byte[] GetBytes(ushort value)
    {
        // 使用BitConverter将ushort值转换为字节数组。
        var bytes = BitConverter.GetBytes(value);
        // 如果当前环境的字节序与目标字节序不同，则需要进行反转。
        if (!this.IsSameOfSet())
        {
            // 反转字节数组，以匹配目标字节序。
            Array.Reverse(bytes);
        }

        // 返回最终的字节数组。
        return bytes;
    }

    /// <summary>
    /// 转换为指定端模式的2字节转换为UInt16数据。
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ToUInt16(byte[] buffer, int offset)
    {
        var len = buffer.Length - offset;
        if (len < 2)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(len), len, 2);
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<ushort>(p);
                }
                else
                {
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<ushort>(p);
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion ushort

    #region ulong

    /// <summary>
    /// 将指定的ulong类型值转换为8字节的字节数组。
    /// 此方法主要用于处理数据转换，确保数据格式符合特定需求。
    /// </summary>
    /// <param name="value">需要转换的ulong类型值。</param>
    /// <returns>转换后的8字节字节数组。</returns>
    public byte[] GetBytes(ulong value)
    {
        // 使用系统方法将ulong值转换为字节数组。
        var bytes = BitConverter.GetBytes(value);
        // 如果当前实例的数据格式与预设的不一致，则需要进行数据格式转换。
        if (!this.IsSameOfSet())
        {
            // 调用私有方法对字节数组进行格式转换。
            bytes = this.ByteTransDataFormat8(bytes, 0);
        }

        // 返回最终的字节数组。
        return bytes;
    }

    /// <summary>
    /// 将ulong类型值转换为指定端的8字节
    /// </summary>
    /// <param name="buffer">指向存放转换后字节的缓冲区的引用</param>
    /// <param name="value">需要转换的ulong类型值</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, ulong value)
    {
        // 直接将字节缓冲区的首地址转换为ulong类型引用，并赋值，实现字节顺序的转换
        Unsafe.As<byte, ulong>(ref buffer) = value;

        // 如果当前实例的字节序与目标字节序不同，则需要进行字节顺序转换
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat8_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 转换为指定端模式的Ulong数据。
    /// </summary>
    /// <param name="buffer">包含要转换数据的字节数组。</param>
    /// <param name="offset">要转换数据的起始位置。</param>
    /// <returns>转换后的ulong数据。</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果offset参数导致转换的字节数不足8字节，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ToUInt64(byte[] buffer, int offset)
    {
        // 检查字节数组的长度是否足够转换8字节的ulong数据
        if (buffer.Length - offset < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            // 固定数组的起始地址，以便能够进行不安全的直接读取
            fixed (byte* p = &buffer[offset])
            {
                // 如果是相同集合，则直接读取ulong数据
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<ulong>(p);
                }
                else
                {
                    // 否则，先转换字节顺序，然后读取ulong数据，最后恢复字节顺序
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<ulong>(p);
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion ulong

    #region bool

    /// <summary>
    /// 将布尔值转换为指定端1字节
    /// </summary>
    /// <param name="value">要转换的布尔值</param>
    /// <returns>转换后的字节数组</returns>
    public byte[] GetBytes(bool value)
    {
        // 使用BitConverter类的GetBytes方法将布尔值转换为字节数组
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// 将布尔数组转为字节数组。不足位补0。
    /// </summary>
    /// <param name="values">待转换的布尔数组。</param>
    /// <returns>转换后的字节数组。</returns>
    public byte[] GetBytes(ReadOnlySpan<bool> values)
    {
        // 检查传入的布尔数组是否为空
        if (values.IsEmpty)
        {
            return [];
        }

        // 计算所需的字节数组的长度，如果布尔数组的长度不是8的倍数，则向上取整
        var numArray = new byte[values.Length % 8 == 0 ? values.Length / 8 : (values.Length / 8) + 1];

        // 遍历布尔数组，将布尔值转换为字节数组
        for (var index = 0; index < values.Length; ++index)
        {
            // 如果当前布尔值为true，则设置对应的字节位为1
            if (values[index])
            {
                numArray[index / 8] = numArray[index / 8].SetBit(index % 8, true);
            }
        }
        // 返回转换后的字节数组
        return numArray;
    }

    /// <summary>
    /// 将布尔值转换为指定端1字节
    /// </summary>
    /// <param name="buffer">指向用于存储转换结果的字节的引用</param>
    /// <param name="value">要转换的布尔值</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, bool value)
    {
        // 使用不安全代码直接将byte引用转换为bool引用，并赋值
        Unsafe.As<byte, bool>(ref buffer) = value;
    }

    /// <summary>
    /// 将布尔值数组转换为字节序列。
    /// </summary>
    /// <param name="buffer">指向目标字节缓冲区的引用。</param>
    /// <param name="values">待转换的布尔值数组。</param>
    /// <exception cref="ArgumentNullException">如果values参数为<see langword="null"/>，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void GetBytes(ref byte buffer, ReadOnlySpan<bool> values)
    {
        // 检查输入的布尔值数组是否为空
        if (values.IsEmpty)
        {
            return;
        }
        // 使用fixed语句固定缓冲区的起始地址，以便直接操作内存
        fixed (byte* p = &buffer)
        {
            // 遍历布尔值数组
            for (var index = 0; index < values.Length; ++index)
            {
                // 如果当前布尔值为true，则设置对应位的值为1
                if (values[index])
                {
                    p[index / 8] = p[index / 8].SetBit(index % 8, true);
                }
            }
        }
    }

    /// <summary>
    ///  转换为指定端模式的bool数据。
    /// </summary>
    /// <param name="buffer">包含转换为bool所需的字节的字节数组。</param>
    /// <param name="offset">从buffer中的哪个位置开始读取字节的偏移量。</param>
    /// <returns>从指定的字节数组和偏移量位置转换得到的bool值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ToBoolean(byte[] buffer, int offset)
    {
        return BitConverter.ToBoolean(buffer, offset);
    }

    /// <summary>
    /// 将指定的字节，按位解析为bool数组。
    /// </summary>
    /// <param name="buffer">包含待解析位的字节数组。</param>
    /// <param name="offset">在字节数组中开始解析的起始位置。</param>
    /// <param name="length">要解析的字节数。</param>
    /// <returns>返回一个bool数组，其中每个元素对应字节中的一个位。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool[] ToBooleansByBit(byte[] buffer, int offset, int length)
    {
        return this.ToBooleansByBit(new ReadOnlySpan<byte>(buffer, offset, length));
    }

    /// <summary>
    /// 将指定的字节，按位解析为bool数组。
    /// </summary>
    /// <param name="span"></param>
    /// <returns>返回一个bool数组，其中每个元素对应字节中的一个位。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool[] ToBooleansByBit(ReadOnlySpan<byte> span)
    {
        var bools = new bool[8 * span.Length];
        this.ToBooleansByBit(span, bools);
        return bools;
    }

    /// <summary>
    /// 将字节跨度按位解析为布尔值，并存储在布尔值跨度中。
    /// </summary>
    /// <param name="byteSpan">包含待解析字节的只读跨度。</param>
    /// <param name="boolSpan">存储解析结果的布尔值跨度。</param>
    /// <returns>解析的布尔值数量。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当布尔值跨度长度不足以存储解析结果时抛出。</exception>
    public int ToBooleansByBit(ReadOnlySpan<byte> byteSpan,Span<bool> boolSpan)
    {
        if (byteSpan.IsEmpty)
        {
            return 0;
        }
        var length = byteSpan.Length * 8;
       
        if (boolSpan.Length < length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(boolSpan.Length), boolSpan.Length, length);
        }

        for (var i = 0; i < boolSpan.Length; i++)
        {
            var byteIndex = i / 8;
            var bitIndex = i % 8;
            boolSpan[i] = byteSpan[byteIndex].GetBit(bitIndex);
        }
        return length;
    }

    /// <summary>
    /// 将指定的字节，按位解析为bool数组。
    /// </summary>
    /// <param name="buffer">指向待解析的字节缓冲区的引用。</param>
    /// <param name="length">要解析的字节数。</param>
    /// <returns>包含每个字节按位解析后的bool值的数组。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool[] ToBooleansByBit(ref byte buffer, int length)
    {
        fixed (byte* p=&buffer)
        {
            return this.ToBooleansByBit(new ReadOnlySpan<byte>(p, length));
        }
        
    }
    #endregion bool

    #region char

    /// <summary>
    /// 将指定字符转换为字节数组
    /// </summary>
    /// <param name="value">要转换的字符</param>
    /// <returns>字节数组</returns>
    public byte[] GetBytes(char value)
    {
        // 使用BitConverter将字符转换为字节数组
        var bytes = BitConverter.GetBytes(value);
        // 如果当前环境的字节顺序与目标端序不一致，则翻转字节数组
        if (!this.IsSameOfSet())
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    /// <summary>
    /// 将指定值转换为2字节，并存储到缓冲区中
    /// </summary>
    /// <param name="buffer">指向存储转换后字节的缓冲区的引用</param>
    /// <param name="value">要转换的字符值</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, char value)
    {
        // 直接将字符值写入缓冲区，利用类型转换提高性能
        Unsafe.As<byte, char>(ref buffer) = value;

        // 如果当前格式与目标格式不同，则进行额外的格式转换处理
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat2_Net6(ref buffer);
        }
    }

    /// <summary>
    ///  转换为指定端模式的Char数据。
    /// </summary>
    /// <param name="buffer">包含要转换数据的字节数组。</param>
    /// <param name="offset">要转换数据的起始位置。</param>
    /// <returns>转换后的Char数据。</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果offset参数导致无法转换出Char数据，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char ToChar(byte[] buffer, int offset)
    {
        // 检查数组长度是否足够从offset位置读取2个字节，因为Char数据占2个字节。
        if (buffer.Length - offset < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            // 使用固定指针直接访问数组中的字节，提高性能。
            fixed (byte* p = &buffer[offset])
            {
                // 如果是统一的字节序模式，则直接读取数据。
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<char>(p);
                }
                else
                {
                    // 否则，先转换数组中的数据格式，读取后再次转换回来。
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<char>(p);
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion char

    #region short

    /// <summary>
    /// 将16位整数转换为指定字节序的字节数组。
    /// </summary>
    /// <param name="value">要转换的16位整数。</param>
    /// <returns>包含转换后字节的字节数组。</returns>
    public byte[] GetBytes(short value)
    {
        var bytes = BitConverter.GetBytes(value);
        // 如果当前实例的字节序与设定的字节序不同，则需要反转字节数组。
        if (!this.IsSameOfSet())
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    /// <summary>
    /// 将16位整数转换为指定字节序，并存储在指定的字节缓冲区中。
    /// </summary>
    /// <param name="buffer">目标字节缓冲区。</param>
    /// <param name="value">要转换的16位整数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, short value)
    {
        // 使用不安全代码直接转换字节到16位整数。
        Unsafe.As<byte, short>(ref buffer) = value;
        // 如果当前实例的字节序与设定的字节序不同，则需要转换字节顺序。
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat2_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 从指定字节数组和偏移量处读取两个字节，并根据设定的字节序模式转换为16位整数。
    /// </summary>
    /// <param name="buffer">包含数据的字节数组。</param>
    /// <param name="offset">从字节数组中的哪个位置开始读取。</param>
    /// <returns>转换得到的16位整数。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ToInt16(byte[] buffer, int offset)
    {
        // 检查字节数组的长度是否足够从offset开始读取两个字节。
        if (buffer.Length - offset < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                // 如果当前实例的字节序与设定的字节序相同，则直接读取。
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<short>(p);
                }
                else
                {
                    // 否则，需要先转换字节序，再读取，然后恢复原来的字节序。
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<short>(p);
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion short

    #region int

    /// <summary>
    /// 将整数转换为指定字节序的4字节数组。
    /// </summary>
    /// <param name="value">要转换的整数。</param>
    /// <returns>转换后的字节数组。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetBytes(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!this.IsSameOfSet())
        {
            bytes = this.ByteTransDataFormat4(bytes, 0);
        }

        return bytes;
    }

    /// <summary>
    /// 将整数转换为指定字节序，并存储在给定的字节数组中。
    /// </summary>
    /// <param name="buffer">存储转换结果的字节数组。</param>
    /// <param name="value">要转换的整数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, int value)
    {
        Unsafe.As<byte, int>(ref buffer) = value;
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat4_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 从字节数组中根据指定字节序转换为int。
    /// </summary>
    /// <param name="buffer">包含要转换数据的字节数组。</param>
    /// <param name="offset">数据在字节数组中的起始位置。</param>
    /// <returns>转换得到的整数。</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果从offset开始不足4个字节，抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt32(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<int>(p);
                }
                else
                {
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<int>(p);
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion int

    #region long

    /// <summary>
    /// 将长整型值转换为按指定端序排列的8字节数组。
    /// </summary>
    /// <param name="value">要转换的长整型值。</param>
    /// <returns>一个包含转换后的8字节的数组。</returns>
    public byte[] GetBytes(long value)
    {
        var bytes = BitConverter.GetBytes(value);
        // 如果当前实例的端序与系统默认不同，则进行端序转换
        if (!this.IsSameOfSet())
        {
            bytes = this.ByteTransDataFormat8(bytes, 0);
        }

        return bytes;
    }

    /// <summary>
    /// 将长整型值转换为按指定端序排列的字节，并存储在缓冲区中。
    /// </summary>
    /// <param name="buffer">存储转换后数据的缓冲区。</param>
    /// <param name="value">要转换的长整型值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, long value)
    {
        Unsafe.As<byte, long>(ref buffer) = value;
        // 如果当前实例的端序与系统默认不同，则进行端序转换
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat8_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 从字节数组中读取按指定端序排列的long值。
    /// </summary>
    /// <param name="buffer">包含数据的字节数组。</param>
    /// <param name="offset">从数组的哪个位置开始读取数据。</param>
    /// <returns>读取到的长整型值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ToInt64(byte[] buffer, int offset)
    {
        // 确保从offset开始的buffer长度至少为8字节
        if (buffer.Length - offset < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                // 如果当前实例的端序与系统默认相同，则直接读取数据
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<long>(p);
                }
                else
                {
                    // 否则，先进行端序转换，然后读取数据，并且再次转换以恢复原状
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<long>(p);
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion long

    #region uint

    /// <summary>
    /// 将无符号整数转换为指定字节序的4字节数组。
    /// </summary>
    /// <param name="value">要转换的无符号整数。</param>
    /// <returns>表示该无符号整数的4字节数组，按指定字节序排列。</returns>
    public byte[] GetBytes(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        // 如果当前实例的字节序与系统默认不同，则进行字节序转换。
        if (!this.IsSameOfSet())
        {
            bytes = this.ByteTransDataFormat4(bytes, 0);
        }

        return bytes;
    }

    /// <summary>
    /// 将无符号整数转换为指定字节序，并存储在给定的字节数组中。
    /// </summary>
    /// <param name="buffer">存储转换结果的字节数组。</param>
    /// <param name="value">要转换的无符号整数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, uint value)
    {
        // 直接将无符号整数转换为字节，并考虑字节序转换。
        Unsafe.As<byte, uint>(ref buffer) = value;
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat4_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 从指定字节序的字节数组中转换出无符号整数。
    /// </summary>
    /// <param name="buffer">包含要转换数据的字节数组。</param>
    /// <param name="offset">数据在字节数组中的起始位置。</param>
    /// <returns>转换得到的无符号整数。</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果从offset开始不足4个字节，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ToUInt32(byte[] buffer, int offset)
    {
        // 确保从offset开始至少有4个字节。
        if (buffer.Length - offset < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                // 如果当前实例的字节序与系统默认相同，则直接读取。
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<uint>(p);
                }
                else
                {
                    // 否则，进行字节序转换，读取数据，再转换回来。
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<uint>(p);
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion uint

    #region float

    /// <summary>
    /// 将浮点数转换为指定字节序的4字节数组。
    /// </summary>
    /// <param name="value">要转换的浮点数。</param>
    /// <returns>转换后的字节数组。</returns>
    public byte[] GetBytes(float value)
    {
        var bytes = BitConverter.GetBytes(value);
        // 如果当前字节序与目标字节序不同，则进行字节序转换。
        if (!this.IsSameOfSet())
        {
            bytes = this.ByteTransDataFormat4(bytes, 0);
        }

        return bytes;
    }

    /// <summary>
    /// 将浮点数转换为指定字节序的字节数组，并存储在指定缓冲区中。
    /// </summary>
    /// <param name="buffer">存储转换结果的缓冲区。</param>
    /// <param name="value">要转换的浮点数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, float value)
    {
        // 直接将浮点数转换为字节，并应用字节序转换（如果需要）。
        Unsafe.As<byte, float>(ref buffer) = value;
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat4_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 从字节数组中按指定字节序读取浮点数。
    /// </summary>
    /// <param name="buffer">包含浮点数数据的字节数组。</param>
    /// <param name="offset">数据在数组中的起始位置。</param>
    /// <returns>读取到的浮点数。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ToSingle(byte[] buffer, int offset)
    {
        // 检查数组长度是否足够。
        if (buffer.Length - offset < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                // 根据当前和目标字节序是否相同，决定是否需要进行字节序转换。
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<float>(p);
                }
                else
                {
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<float>(p);
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion float

    #region long

    /// <summary>
    /// 将指定的双精度浮点数转换为对应字节端序的8字节数组。
    /// </summary>
    /// <param name="value">要转换的双精度浮点数。</param>
    /// <returns>对应的字节数组。</returns>
    public byte[] GetBytes(double value)
    {
        var bytes = BitConverter.GetBytes(value);
        // 如果当前实例的字节序与系统默认不同，则进行字节序转换。
        if (!this.IsSameOfSet())
        {
            bytes = this.ByteTransDataFormat8(bytes, 0);
        }

        return bytes;
    }

    /// <summary>
    /// 将指定的双精度浮点数转换为对应字节端序的8字节，并存储在指定的字节数组中。
    /// </summary>
    /// <param name="buffer">用于存储转换结果的字节数组。</param>
    /// <param name="value">要转换的双精度浮点数。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, double value)
    {
        Unsafe.As<byte, double>(ref buffer) = value;
        // 如果当前实例的字节序与系统默认不同，则进行字节序转换。
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat8_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 将指定偏移量的字节数组转换为对应字节端序的双精度浮点数。
    /// </summary>
    /// <param name="buffer">包含数据的字节数组。</param>
    /// <param name="offset">从数组的哪个位置开始读取数据。</param>
    /// <returns>转换得到的双精度浮点数。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToDouble(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                // 如果当前实例的字节序与系统默认相同，则直接读取数据。
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<double>(p);
                }
                else
                {
                    // 否则，先进行字节序转换，然后读取数据，并再次转换回来。
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<double>(p);
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion long

    #region decimal

    /// <summary>
    /// 将指定的 decimal 值转换为一个包含该值的字节数组。
    /// </summary>
    /// <param name="value">要转换的 decimal 值。</param>
    /// <returns>一个包含转换后的字节的字节数组。</returns>
    public byte[] GetBytes(decimal value)
    {
        var bytes = new byte[16];
        this.GetBytes(ref bytes[0], value);
        return bytes;
    }

    /// <summary>
    /// 将指定的 decimal 值转换为一个包含该值的字节数组，并将结果存储在指定的缓冲区中。
    /// </summary>
    /// <param name="buffer">存储转换后的字节的缓冲区。</param>
    /// <param name="value">要转换的 decimal 值。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBytes(ref byte buffer, decimal value)
    {
        Unsafe.As<byte, decimal>(ref buffer) = value;
        if (!this.IsSameOfSet())
        {
            this.ByteTransDataFormat16_Net6(ref buffer);
        }
    }

    /// <summary>
    /// 从指定偏移量处的字节数组中读取一个 decimal 值。
    /// </summary>
    /// <param name="buffer">包含要读取的 decimal 值的字节数组。</param>
    /// <param name="offset">从 buffer 的哪个位置开始读取。</param>
    /// <returns>从字节数组中读取到的 decimal 值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果从 offset 开始的 buffer 的长度小于 16，则抛出此异常。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal ToDecimal(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 16)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<decimal>(p);
                }
                else
                {
                    this.ByteTransDataFormat16_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<decimal>(p);
                    this.ByteTransDataFormat16_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    #endregion decimal

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

    #region 其他

    /// <summary>
    /// 计算从源类型到目标类型的转换长度。
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
            int sizeT1 = Unsafe.SizeOf<TSource>();
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

        // 检查是否可整除
        if (sourceBits % targetBitSize != 0)
        {
            throw new ArgumentException(
                $"Source bits ({sourceBits}) must be divisible by target type bit size ({targetBitSize})");
        }

        // 计算目标长度并检查溢出
        var resultLength = sourceBits / targetBitSize;
        if (resultLength > int.MaxValue)
        {
            throw new OverflowException($"Converted length ({resultLength}) exceeds maximum Span size");
        }

        return (int)resultLength;
    }

    #endregion
}
