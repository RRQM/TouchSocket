//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core
{
    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
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

        private static EndianType m_defaultEndianType;

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
            this.EndianType = endianType;
        }

        /// <summary>
        /// 按照枚举值选择默认的端序。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TouchSocketBitConverter GetBitConverter(EndianType endianType)
        {
            switch (endianType)
            {
                case EndianType.Little:
                    return LittleEndian;

                case EndianType.Big:
                    return BigEndian;

                case EndianType.LittleSwap:
                    return LittleSwapEndian;

                case EndianType.BigSwap:
                    return BigSwapEndian;
                default:
                    throw new Exception("没有该选项");
            }
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
            get => m_defaultEndianType;
            set
            {
                m_defaultEndianType = value;
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
        public EndianType EndianType { get; private set; }

        /// <summary>
        /// 判断当前系统是否为设置的大小端
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSameOfSet()
        {
            return !(BitConverter.IsLittleEndian ^ (this.EndianType == EndianType.Little));
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
        public ushort ToUInt16(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToUInt16(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, 2);
                Array.Reverse(bytes);
                return BitConverter.ToUInt16(bytes, 0);
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
        ///  转换为指定端模式的Ulong数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ulong ToUInt64(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToUInt64(buffer, offset);
            }
            else
            {
                var bytes = this.ByteTransDataFormat8(buffer, offset);
                return BitConverter.ToUInt64(bytes, 0);
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
                    numArray[index / 8] = numArray[index / 8].SetBit((short)(index % 8), 1);
                }
            }
            return numArray;
        }

        /// <summary>
        ///  转换为指定端模式的bool数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
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
        public bool[] ToBooleans(byte[] buffer, int offset,int length)
        {
            var bools = new bool[8*length];
            for (short i = 0; i < bools.Length; i++)
            {
                bools[i]= Convert.ToBoolean(buffer[offset+i/8].GetBit(i));
            }
            return bools;
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
        ///  转换为指定端模式的Char数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public char ToChar(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToChar(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToChar(bytes, 0);
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
        ///  转换为指定端模式的Short数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public short ToInt16(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToInt16(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToInt16(bytes, 0);
            }
        }

        #endregion short

        #region int

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        ///  转换为指定端模式的int数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int ToInt32(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToInt32(buffer, offset);
            }
            else
            {
                var bytes = this.ByteTransDataFormat4(buffer, offset);
                return BitConverter.ToInt32(bytes, 0);
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
        ///  转换为指定端模式的long数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public long ToInt64(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToInt64(buffer, offset);
            }
            else
            {
                var bytes = this.ByteTransDataFormat8(buffer, offset);
                return BitConverter.ToInt64(bytes, 0);
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
        ///  转换为指定端模式的Uint数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public uint ToUInt32(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToUInt32(buffer, offset);
            }
            else
            {
                var bytes = this.ByteTransDataFormat4(buffer, offset);
                return BitConverter.ToUInt32(bytes, 0);
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
        ///  转换为指定端模式的float数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public float ToSingle(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToSingle(buffer, offset);
            }
            else
            {
                var bytes = this.ByteTransDataFormat4(buffer, offset);
                return BitConverter.ToSingle(bytes, 0);
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
        ///  转换为指定端模式的double数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public double ToDouble(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToDouble(buffer, offset);
            }
            else
            {
                var bytes = this.ByteTransDataFormat8(buffer, offset);
                return BitConverter.ToDouble(bytes, 0);
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
            var bytes = DecimalConver.ToBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的<see cref="decimal"/>数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public decimal ToDecimal(byte[] buffer, int offset)
        {
            var bytes = new byte[16];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (this.IsSameOfSet())
            {
                return DecimalConver.FromBytes(bytes);
            }
            else
            {
                Array.Reverse(bytes);
                return DecimalConver.FromBytes(bytes);
            }
        }

        #endregion decimal

        #region Tool

        /// <summary>反转多字节的数据信息</summary>
        /// <param name="value">数据字节</param>
        /// <param name="offset">起始索引，默认值为0</param>
        /// <returns>实际字节信息</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] ByteTransDataFormat4(byte[] value, int offset)
        {
            byte[] numArray = new byte[4];
            switch (EndianType)
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

        /// <summary>反转多字节的数据信息</summary>
        /// <param name="value">数据字节</param>
        /// <param name="offset">起始索引，默认值为0</param>
        /// <returns>实际字节信息</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] ByteTransDataFormat8(byte[] value, int offset)
        {
            byte[] numArray = new byte[8];
            switch (EndianType)
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

        #endregion Tool
    }
}