//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.IO.Compression;
using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    public class TestGzip
    {
        [InlineData(1024)]
        [InlineData(1024 * 10)]
        [InlineData(1024 * 64)]
        [InlineData(1024 * 1024)]
        [Theory]
        public void NormalGzipShouldBeOk(int len)
        {
            var data = new byte[len];
            new Random().NextBytes(data);
            var compressData = Compress(data);
            var decompressData = Decompress(compressData);

            Assert.True(data.SequenceEqual(decompressData));
        }

        [InlineData(1024)]
        [InlineData(1024 * 10)]
        [InlineData(1024 * 64)]
        [InlineData(1024 * 1024)]
        [Theory]
        public void ByteBlockGzipShouldBeOk(int len)
        {
            var data = new byte[len];
            new Random().NextBytes(data);

            using (var byteBlock = new ByteBlock())
            {
                GZip.Compress(byteBlock, data, 0, data.Length);

                var decompressData1 = Decompress(byteBlock.ToArray());

                Assert.True(data.SequenceEqual(decompressData1));

                var decompressData2 = GZip.Decompress(byteBlock.ToArray());

                Assert.True(data.SequenceEqual(decompressData2));
            }
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static byte[] Compress(byte[] buffer)
        {
            using (var stream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    gZipStream.Write(buffer, 0, buffer.Length);
                    gZipStream.Close();
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] Decompress(byte[] data)
        {
            try
            {
                using var stream = new MemoryStream();
                using (var gZipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                {
                    var bytes = new byte[40960];
                    int n;
                    while ((n = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        stream.Write(bytes, 0, n);
                    }
                    gZipStream.Close();
                }

                return stream.ToArray();
            }
            catch
            {
                return null;
            }
        }
    }
}