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
using System;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.IO;
using Xunit;

namespace XUnitTest.Core
{
    public class TestIO
    {
        [Theory]
        [InlineData(10, 1024 * 10)]
        [InlineData(1024, 1024 * 10)]
        [InlineData(10240, 1024 * 10)]
        [InlineData(1024000, 1024 * 10)]
        [InlineData(10240000, 1024 * 10)]
        public void BlockReadStreamShouldBeOk(int readBuffer, int writeBuffer)
        {
            for (int j = 0; j < 10; j++)
            {
                byte[] data = new byte[1024 * 1024 * 10];
                new Random().NextBytes(data);

                MyBlockReadStream stream = new MyBlockReadStream();

                Task task = Task.Run(() =>
                  {
                      using (ByteBlock byteBlock = new ByteBlock(data))
                      {
                          while (true)
                          {
                              int r = byteBlock.Read(out byte[] buffer, writeBuffer);
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

                ByteBlock block = new ByteBlock();
                byte[] buffer = new byte[readBuffer];
                while (true)
                {
                    int r = stream.Read(buffer, 0, buffer.Length);
                    if (r == 0)
                    {
                        break;
                    }
                    block.Write(buffer, 0, r);
                }

                Assert.Equal(data.Length, block.Len);

                for (int i = 0; i < data.Length; i++)
                {
                    Assert.Equal(data[i], block.Buffer[i]);
                }
                task.Wait();
            }
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