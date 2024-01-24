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

namespace TouchSocket.Core
{
    /// <summary>
    /// <see cref="decimal"/>与字节数组转换
    /// </summary>
    public static class DecimalConver
    {
        /// <summary>
        /// 将<see cref="decimal"/>对象转换为固定字节长度（16）数组。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(decimal value)
        {
            var bits = decimal.GetBits(value);
            var bytes = new byte[bits.Length * 4];
            for (var i = 0; i < bits.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    bytes[i * 4 + j] = (byte)(bits[i] >> (j * 8));
                }
            }
            return bytes;
        }

        /// <summary>
        /// 将固定字节长度（16）数组转换为<see cref="decimal"/>对象。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal FromBytes(byte[] array)
        {
            var bits = new int[array.Length / 4];
            for (var i = 0; i < bits.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    bits[i] |= array[i * 4 + j] << j * 8;
                }
            }
            return new decimal(bits);
        }
    }
}