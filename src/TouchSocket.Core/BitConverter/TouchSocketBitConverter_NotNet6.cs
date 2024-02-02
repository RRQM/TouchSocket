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

#if !Unsafe
using System;
using System.Runtime.CompilerServices;

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
        #endregion decimal
    }
}

#endif