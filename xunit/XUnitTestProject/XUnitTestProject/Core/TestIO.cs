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
using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    public class TestIO
    {
        [Theory]
        [InlineData(10, 1024 * 10)]
        [InlineData(1024, 1024 * 10)]
        [InlineData(10240, 1024 * 10)]
        [InlineData(1024000, 1024 * 10)]
        [InlineData(10240000, 1024 * 10)]
        public async Task BlockReadStreamShouldBeOk(int readBuffer, int writeBuffer)
        {
            for (var j = 0; j < 10; j++)
            {
                var data = new byte[1024 * 1024 * 10];
                new Random().NextBytes(data);

                var stream = new MyBlockReadStream();

                var task = Task.Run(() =>
                  {
                      using (var byteBlock = new ByteBlock(data))
                      {
                          while (true)
                          {
                              var r = byteBlock.Read(out var buffer, writeBuffer);
                              if (r == 0)
                              {
                                  if (stream.InputData(new byte[0], 0, 0))
                                  {
                                      break;
                                  }
                              }
                              if (!stream.InputData(buffer, 0, buffer.Length))
                              {
                                  throw new Exception();
                              }
                          }
                      }
                  });

                var block = new ByteBlock();
                var buffer = new byte[readBuffer];
                while (true)
                {
                    var r = stream.Read(buffer, 0, buffer.Length);
                    if (r == 0)
                    {
                        break;
                    }
                    block.Write(buffer, 0, r);
                }

                Assert.Equal(data.Length, block.Len);

                for (var i = 0; i < data.Length; i++)
                {
                    Assert.Equal(data[i], block.Buffer[i]);
                }
                await task;
            }
        }

        [Fact]
        public void GetRelativePathShouldBeOk()
        {
            var relativeTo = "C:\\Users\\Demo";
            var path = "C:\\Users\\Demo\\ABC\\1.txt";

            Assert.Equal(Path.GetRelativePath(relativeTo, path), FileUtility.GetRelativePath(relativeTo, path));
            Assert.Equal("ABC\\1.txt", FileUtility.GetRelativePath(relativeTo, path));
        }
    }

    internal class MyBlockReadStream : BlockReadStream
    {
        public override bool CanWrite => throw new NotImplementedException();

        public override bool CanRead => true;

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        internal bool InputData(byte[] buffer, int offset, int length)
        {
            return this.Input(buffer, offset, length);
        }
    }
}