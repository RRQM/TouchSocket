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

#if Unsafe
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TouchSocket.Core
{
    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
    /// </summary>
    public sealed partial class TouchSocketBitConverter
    {
        #region ushort

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, ushort value)
        {
            Unsafe.As<byte, ushort>(ref buffer) = value;
            if (!this.IsSameOfSet())
            {
                this.ByteTransDataFormat2_Net6(ref buffer);
            }
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
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBytes(ref byte buffer, bool value)
        {
            Unsafe.As<byte, bool>(ref buffer) = value;
        }

        ///// <summary>
        ///// 将布尔数组转为字节数组。不足位补0.
        ///// </summary>
        ///// <param name="values"></param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //public void GetBytes(ref byte buffer,bool[] values)
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
            for (short i = 0; i < bools.Length; i++)
            {
                bools[i] = Convert.ToBoolean(buffer[offset + i / 8].GetBit(i));
            }
            return bools;
        }

        #endregion bool

        #region char

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

        #endregion Tool
    }
}
#endif