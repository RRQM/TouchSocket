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

namespace RRQMCore.Helper
{
    /// <summary>
    /// 编码大小端辅助。
    /// 代码借鉴于网络。
    /// </summary>
    public static class EndianHelper
    {
        /// <summary>
        /// 判断当前系统是否为设置的大小端
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public static bool IsSameOfSet(this EndianType endianType)
        {
            return !(BitConverter.IsLittleEndian ^ (endianType == EndianType.Little));
        }

        /// <summary>
        /// 转换为指定端字节
        /// </summary>
        /// <param name="value"></param>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public static byte[] ConvertToBytes(this ulong value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!endianType.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// 转换为指定端字节
        /// </summary>
        /// <param name="value"></param>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public static byte[] ConvertToBytes(this ushort value, EndianType endianType)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!endianType.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的Ushort数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public static ushort ToUshort(this byte[] buffer, int offset, EndianType endianType)
        {
            byte[] bytes = new byte[2];
            Array.Copy(buffer, offset, bytes, 0, 2);
            if (!endianType.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        ///  转换为指定端模式的Ulong数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public static ulong ToUlong(this byte[] buffer, int offset, EndianType endianType)
        {
            byte[] bytes = new byte[8];
            Array.Copy(buffer, offset, bytes, 0, 8);
            if (!endianType.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
