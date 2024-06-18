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
using TouchSocket.Resources;

namespace TouchSocket.Core
{
    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
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
        /// 构造函数
        /// </summary>
        /// <param name="endianType"></param>
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
        /// 指定大小端。
        /// </summary>
        public EndianType EndianType => this.m_endianType;

        /// <summary>
        /// 按照枚举值选择默认的端序。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TouchSocketBitConverter GetBitConverter(EndianType endianType)
        {
            return endianType switch
            {
                EndianType.Little => LittleEndian,
                EndianType.Big => BigEndian,
                EndianType.LittleSwap => LittleSwapEndian,
                EndianType.BigSwap => BigSwapEndian,
                _ => throw new InvalidOperationException(TouchSocketCoreResource.InvalidParameter.Format(nameof(endianType))),
            };
        }

        /// <summary>
        /// 判断当前系统是否为设置的大小端
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSameOfSet()
        {
            return !(BitConverter.IsLittleEndian ^ (this.EndianType == EndianType.Little));
        }

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

        public unsafe int UnsafeWriteBytes<T>(ref byte source, T value) where T : unmanaged
        {
            var size = sizeof(T);
            
            Unsafe.As<byte, T>(ref source) = value;
            if (!this.IsSameOfSet())
            {
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
                    ThrowHelper.ThrowNotSupportedException(size.ToString());
                }
            }

            return size;
        }

        #region ushort

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

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
            if (buffer.Length - offset < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
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
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                bytes = this.ByteTransDataFormat8(bytes, 0);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, ulong value)
        {
            Unsafe.As<byte, ulong>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat8_Net6(ref buffer);
            }
        }

        /// <summary>
        ///  转换为指定端模式的Ulong数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ToUInt64(byte[] buffer, int offset)
        {
            if (buffer.Length - offset < 8)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            unsafe
            {
                fixed (byte* p = &buffer[offset])
                {
                    if (this.IsSameOfSet())
                    {
                        return Unsafe.Read<ulong>(p);
                    }
                    else
                    {
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
        /// 转换为指定端1字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// 将布尔数组转为字节数组。不足位补0.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public byte[] GetBytes(bool[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var numArray = new byte[values.Length % 8 == 0 ? values.Length / 8 : (values.Length / 8) + 1];
            for (var index = 0; index < values.Length; ++index)
            {
                if (values[index])
                {
                    numArray[index / 8] = numArray[index / 8].SetBit(index % 8, true);
                }
            }
            return numArray;
        }

        /// <summary>
        /// 转换为指定端1字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, bool value)
        {
            Unsafe.As<byte, bool>(ref buffer) = value;
        }

        /// <summary>
        /// 将布尔数组转为字节数组。不足位补0.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetBytes(ref byte buffer, bool[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            fixed (byte* p = &buffer)
            {
                for (var index = 0; index < values.Length; ++index)
                {
                    if (values[index])
                    {
                        p[index / 8] = p[index / 8].SetBit(index % 8, true);
                    }
                }
            }
        }

        ///// <summary>
        ///// 将布尔数组转为字节数组。不足位补0.
        ///// </summary>
        ///// <param name="values"></param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //public void GetBytes(ref byte buffer, bool[] values)
        //{
        //    if (values is null)
        //    {
        //        throw new ArgumentNullException(nameof(values));
        //    }

        //    var numArray = new byte[values.Length % 8 == 0 ? values.Length / 8 : (values.Length / 8) + 1];
        //    for (var index = 0; index < values.Length; ++index)
        //    {
        //        if (values[index])
        //        {
        //            numArray[index / 8] = numArray[index / 8].SetBit((short)(index % 8), 1);
        //        }
        //    }
        //    return numArray;
        //}

        /// <summary>
        ///  转换为指定端模式的bool数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ToBoolean(byte[] buffer, int offset)
        {
            return BitConverter.ToBoolean(buffer, offset);
        }

        /// <summary>
        /// 将指定的字节，按位解析为bool数组。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool[] ToBooleans(byte[] buffer, int offset, int length)
        {
            var bools = new bool[8 * length];
            for (var i = 0; i < bools.Length; i++)
            {
                bools[i] = Convert.ToBoolean(buffer[offset + i / 8].GetBit(i % 8));
            }
            return bools;
        }

        /// <summary>
        /// 将指定的字节，按位解析为bool数组。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool[] ToBooleans(ref byte buffer, int length)
        {
            fixed (byte* p = &buffer)
            {
                var bools = new bool[8 * length];
                for (var i = 0; i < bools.Length; i++)
                {
                    bools[i] = Convert.ToBoolean(p[i / 8].GetBit(i % 8));
                }
                return bools;
            }
        }

        public unsafe bool[] ToBooleans(ReadOnlySpan<byte> span, int length)
        {
            fixed (byte* p = &span[0])
            {
                var bools = new bool[8 * length];
                for (var i = 0; i < bools.Length; i++)
                {
                    bools[i] = Convert.ToBoolean(p[i / 8].GetBit(i % 8));
                }
                return bools;
            }
        }

        #endregion bool

        #region char

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(char value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, char value)
        {
            Unsafe.As<byte, char>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat2_Net6(ref buffer);
            }
        }

        /// <summary>
        ///  转换为指定端模式的Char数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ToChar(byte[] buffer, int offset)
        {
            if (buffer.Length - offset < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            unsafe
            {
                fixed (byte* p = &buffer[offset])
                {
                    if (this.IsSameOfSet())
                    {
                        return Unsafe.Read<char>(p);
                    }
                    else
                    {
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
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, short value)
        {
            Unsafe.As<byte, short>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat2_Net6(ref buffer);
            }
        }

        /// <summary>
        ///  转换为指定端模式的Short数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ToInt16(byte[] buffer, int offset)
        {
            if (buffer.Length - offset < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            unsafe
            {
                fixed (byte* p = &buffer[offset])
                {
                    if (this.IsSameOfSet())
                    {
                        return Unsafe.Read<short>(p);
                    }
                    else
                    {
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
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
        ///  转换为指定端模式的int数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
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
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                bytes = this.ByteTransDataFormat8(bytes, 0);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, long value)
        {
            Unsafe.As<byte, long>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat8_Net6(ref buffer);
            }
        }

        /// <summary>
        ///  转换为指定端模式的long数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToInt64(byte[] buffer, int offset)
        {
            if (buffer.Length - offset < 8)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            unsafe
            {
                fixed (byte* p = &buffer[offset])
                {
                    if (this.IsSameOfSet())
                    {
                        return Unsafe.Read<long>(p);
                    }
                    else
                    {
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
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                bytes = this.ByteTransDataFormat4(bytes, 0);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, uint value)
        {
            Unsafe.As<byte, uint>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat4_Net6(ref buffer);
            }
        }

        /// <summary>
        ///  转换为指定端模式的Uint数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToUInt32(byte[] buffer, int offset)
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
                        return Unsafe.Read<uint>(p);
                    }
                    else
                    {
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
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                bytes = this.ByteTransDataFormat4(bytes, 0);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, float value)
        {
            Unsafe.As<byte, float>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat4_Net6(ref buffer);
            }
        }

        /// <summary>
        ///  转换为指定端模式的float数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ToSingle(byte[] buffer, int offset)
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
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                bytes = this.ByteTransDataFormat8(bytes, 0);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, double value)
        {
            Unsafe.As<byte, double>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat8_Net6(ref buffer);
            }
        }

        /// <summary>
        ///  转换为指定端模式的double数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
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
                    if (this.IsSameOfSet())
                    {
                        return Unsafe.Read<double>(p);
                    }
                    else
                    {
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
        /// 转换为指定端16字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(decimal value)
        {
            var bytes = new byte[16];
            this.GetBytes(ref bytes[0], value);
            return bytes;
        }

        /// <summary>
        /// 转换为指定端16字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
        ///  转换为指定端模式的<see cref="decimal"/>数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
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
    }
}