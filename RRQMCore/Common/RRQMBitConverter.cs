//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore
{
    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
    /// </summary>
    public class RRQMBitConverter
    {
        /// <summary>
        /// 以大端
        /// </summary>
        public static RRQMBitConverter BigEndian;

        /// <summary>
        /// 以小端
        /// </summary>
        public static RRQMBitConverter LittleEndian;

        static RRQMBitConverter()
        {
            BigEndian = new RRQMBitConverter(EndianType.Big);
            LittleEndian = new RRQMBitConverter(EndianType.Little);
            DefaultEndianType = EndianType.Little;
        }

        private static RRQMBitConverter @default;
        /// <summary>
        /// 以默认小端，可通过<see cref="RRQMBitConverter.DefaultEndianType"/>重新指定默认端。
        /// </summary>
        public static RRQMBitConverter Default
        {
            get { return @default; }
        }

        private static EndianType @defaultEndianType;
        /// <summary>
        /// 默认大小端切换。
        /// </summary>
        public static EndianType DefaultEndianType
        {
            get { return @defaultEndianType; }
            set 
            { 
                @defaultEndianType = value;
                switch (value)
                {
                    case EndianType.Little:
                        @default = LittleEndian;
                        break;
                    case EndianType.Big:
                        @default = BigEndian;
                        break;
                    default:
                        break;
                }
            }
        }


        private EndianType endianType;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endianType"></param>
        public RRQMBitConverter(EndianType endianType)
        {
            this.endianType = endianType;
        }

        /// <summary>
        /// 指定大小端。
        /// </summary>
        public EndianType EndianType
        {
            get { return endianType; }
        }

        /// <summary>
        /// 判断当前系统是否为设置的大小端
        /// </summary>
        /// <returns></returns>
        public bool IsSameOfSet()
        {
            return !(BitConverter.IsLittleEndian ^ (endianType == EndianType.Little));
        }

        #region ushort
        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
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
            byte[] bytes = new byte[2];
            Array.Copy(buffer, offset, bytes, 0, 2);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt16(bytes, 0);
        }
        #endregion

        #region ulong
        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
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
            byte[] bytes = new byte[8];
            Array.Copy(buffer, offset, bytes, 0, 8);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt64(bytes, 0);
        }
        #endregion

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
        ///  转换为指定端模式的bool数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool ToBoolean(byte[] buffer, int offset)
        {
            return BitConverter.ToBoolean(buffer, offset);
        }
        #endregion

        #region char
        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(char value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
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
            byte[] bytes = new byte[2];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToChar(bytes, 0);
        }
        #endregion

        #region short
        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
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
            byte[] bytes = new byte[2];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt16(bytes, 0);
        }
        #endregion

        #region int
        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
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
            byte[] bytes = new byte[4];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }
        #endregion
        #region long
        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
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
            byte[] bytes = new byte[8];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt64(bytes, 0);
        }
        #endregion
        #region uint
        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
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
            byte[] bytes = new byte[4];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt32(bytes, 0);
        }
        #endregion

        #region float
        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
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
            byte[] bytes = new byte[4];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToSingle(bytes, 0);
        }
        #endregion

        #region long
        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
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
            byte[] bytes = new byte[8];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToDouble(bytes, 0);
        }
        #endregion
    }
}
