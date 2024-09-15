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

using System.IO;
using System.IO.Compression;

namespace TouchSocket.Core
{
    /// <summary>
    /// Gzip操作类
    /// </summary>
    public static partial class GZip
    {
        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static void Compress(Stream stream, byte[] buffer, int offset, int length)
        {
            using (var gZipStream = new GZipStream(stream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, offset, length);
                gZipStream.Close();
            }
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static void Compress(Stream stream, byte[] buffer)
        {
            Compress(stream, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] buffer, int offset, int length)
        {
            using (var byteBlock = new ByteBlock(length))
            {
                Compress(byteBlock.AsStream(), buffer, offset, length);
                return byteBlock.ToArray();
            }
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] buffer)
        {
            return Compress(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static void Decompress<TByteBlock>(ref TByteBlock byteBlock, byte[] data, int offset, int length)
            where TByteBlock : IByteBlock
        {
            using (var gZipStream = new GZipStream(new MemoryStream(data, offset, length), CompressionMode.Decompress))
            {
                var bytes = BytePool.Default.Rent(1024 * 64);
                try
                {
                    int r;
                    while ((r = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        byteBlock.Write(new System.ReadOnlySpan<byte>(bytes, 0, r));
                    }
                    gZipStream.Close();
                }
                finally
                {
                    BytePool.Default.Return(bytes);
                }
            }
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="data"></param>
        public static void Decompress<TByteBlock>(ref TByteBlock byteBlock, byte[] data)
            where TByteBlock : IByteBlock
        {
            Decompress(ref byteBlock, data, 0, data.Length);
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data, int offset, int length)
        {
            var byteBlock = new ByteBlock(length);
            try
            {
                Decompress(ref byteBlock, data, offset, length);
                return byteBlock.ToArray();
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            return Decompress(data, 0, data.Length);
        }
    }
}