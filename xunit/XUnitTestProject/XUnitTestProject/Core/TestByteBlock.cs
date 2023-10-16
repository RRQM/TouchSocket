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
    
    public class TestByteBlock : UnitBase
    {
        [Fact]
        public void PackageShouldBeOk()
        {
            MyPackage myPackage = default;

            using (var byteBlock = new ByteBlock())
            {
                byteBlock.WritePackage(myPackage);
                byteBlock.SeekToStart();
                myPackage = byteBlock.ReadPackage<MyPackage>();
                Assert.Null(myPackage);

                byteBlock.Reset();
                myPackage = new MyPackage();
                byteBlock.WritePackage(myPackage);
                byteBlock.SeekToStart();
                myPackage = byteBlock.ReadPackage<MyPackage>();
                Assert.NotNull(myPackage);
            }
        }

        [Fact]
        public void ShouldCanRatioCapacity()
        {
            //测试申请内存
            var byteBlock = new ByteBlock(10);

            Assert.NotNull(byteBlock);
            Assert.Equal(0, byteBlock.Pos);
            Assert.Equal(0, byteBlock.Len);
            Assert.Equal(16, byteBlock.Capacity);

            //测试写入时动态扩容
            var data = new byte[20];
            new Random().NextBytes(data);
            byteBlock.Write(data);
            Assert.Equal(20, byteBlock.Pos);
            Assert.Equal(20, byteBlock.Len);
            Assert.Equal(32, byteBlock.Capacity);

            var data2 = new byte[100];
            new Random().NextBytes(data2);
            byteBlock.Write(data2);
            Assert.Equal(120, byteBlock.Pos);
            Assert.Equal(120, byteBlock.Len);
            Assert.Equal(128, byteBlock.Capacity);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        [InlineData(10000000)]
        [InlineData(100000000)]
        public void ShouldCanReadOverlength(int count)
        {
            var byteBlock = new ByteBlock(1024);
            for (var i = 0; i < count; i++)
            {
                byteBlock.Write(i);
            }

            byteBlock.Pos = 0;
            for (var i = 0; i < count; i++)
            {
                var value = byteBlock.ReadInt32();
                Assert.Equal(i, value);
            }
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        [InlineData(10000000)]
        [InlineData(100000000)]
        public void ShouldCanReadStringOverlength(int count)
        {
            var byteBlock = new ByteBlock(1024);
            for (var i = 0; i < count; i++)
            {
                byteBlock.Write(i.ToString());
            }

            byteBlock.Pos = 0;
            for (var i = 0; i < count; i++)
            {
                var value = byteBlock.ReadString();
                Assert.Equal(i.ToString(), value);
            }
        }

        [Fact]
        public void ShouldCanWriteAndRead()
        {
            var byteBlock = BytePool.Default.GetByteBlock(1024 * 1024);

            //开始写

            byte writeByte = 10;//Byte
            byteBlock.Write(writeByte);

            var writeChar = 'A';//Char
            byteBlock.Write(writeChar);

            var writeInt = int.MaxValue;//int
            byteBlock.Write(writeInt);

            var writeDouble = 3.14;//Double
            byteBlock.Write(writeDouble);

            var writeObject = new Test() { P1 = 10, P2 = "RRQM" };//object
            byteBlock.WriteObject(writeObject);

            byteBlock.WriteObject(null);//null object

            var writeBytes = new byte[1024];//byte[]包
            new Random().NextBytes(writeBytes);
            byteBlock.WriteBytesPackage(writeBytes);

            byteBlock.WriteBytesPackage(null);//null byte[]包

            //重置流位置，然后依次读
            byteBlock.Pos = 0;
            var newWriteByte = (byte)byteBlock.ReadByte();//byte
            Assert.Equal(writeByte, newWriteByte);

            var newWriteChar = byteBlock.ReadChar();//char
            Assert.Equal(writeChar, newWriteChar);

            var newWriteInt = byteBlock.ReadInt32();//int
            Assert.Equal(writeInt, newWriteInt);

            var newWriteDouble = byteBlock.ReadDouble();//Double
            Assert.Equal(writeDouble, newWriteDouble);

            var newWriteObject = byteBlock.ReadObject<Test>();//object
            Assert.Equal(writeObject.P1, newWriteObject.P1);
            Assert.Equal(writeObject.P2, newWriteObject.P2);

            var nullObject = byteBlock.ReadObject<object>();//null object
            Assert.Null(nullObject);

            var newWriteBytes = byteBlock.ReadBytesPackage();
            for (var i = 0; i < newWriteBytes.Length; i++)
            {
                Assert.Equal(writeBytes[i], newWriteBytes[i]);
            }

            var newNullWriteBytes = byteBlock.ReadBytesPackage();
            Assert.Null(newNullWriteBytes);
        }

        [Fact]
        public void ShouldCanWriteOverlengthPos()
        {
            var byteBlock = new ByteBlock(10);
            byteBlock.Clear();

            for (byte i = 0; i < 10; i++)
            {
                byteBlock.Write(i);
            }

            byteBlock.Pos = 20;
            for (byte i = 0; i < 10; i++)
            {
                byteBlock.Write(i);
            }

            Assert.Equal(byteBlock.Pos, byteBlock.Len);
            byteBlock.Pos = 0;
            for (byte i = 0; i < 10; i++)
            {
                Assert.Equal(i, byteBlock.ReadByte());
            }
            byteBlock.Pos = 20;
            for (byte i = 0; i < 10; i++)
            {
                Assert.Equal(i, byteBlock.ReadByte());
            }
        }

        [Fact]
        public void ShouldCanWriteOverlengthPosTwo()
        {
            var byteBlock = new ByteBlock(10);
            byteBlock.Pos = 10;
            byteBlock.Write(new byte[10]);
            Assert.Equal(20, byteBlock.Pos);
            Assert.Equal(20, byteBlock.Len);

            byteBlock.Pos = 0;
            byteBlock.Write(new byte[10]);
            Assert.Equal(10, byteBlock.Pos);
            Assert.Equal(20, byteBlock.Len);
        }

        [Serializable]
        public class Test
        {
            public int P1 { get; set; }
            public string P2 { get; set; }
        }

        internal class MyPackage : WaitRouterPackage
        {
            public int P1 { get; set; }
            public string P2 { get; set; }
        }
    }
}