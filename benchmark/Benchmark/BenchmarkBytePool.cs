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
//------------------------------------------------------------------------------
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [MemoryDiagnoser]
    public class BenchmarkBytePool : BenchmarkBase
    {
        [Benchmark]
        public void CreateByteBlock()
        {
            for (var i = 0; i < this.Count; i++)
            {
                using (var byteBlock = new ByteBlock(1024 * 64))
                {
                }
            }
        }

        [Benchmark]
        public void CreateValueByteBlock()
        {
            for (var i = 0; i < this.Count; i++)
            {
                using (var byteBlock = new ValueByteBlock(1024 * 64))
                {
                }
            }
        }

        [Benchmark]
        public void CreateArrayPoolBytes()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var bytes = this.Rent(1024 * 64);
                this.Return(bytes);
            }
        }
        private byte[] Rent(int size)
        {
            return System.Buffers.ArrayPool<byte>.Shared.Rent(size);
        }
        private void Return(byte[] bytes)
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
        }


        [Benchmark]
        public void CreateBytePoolBytes()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var bytes = BytePool.Default.Rent(1024 * 64);
                BytePool.Default.Return(bytes);
            }
        }



        [Benchmark]
        public void RunByteBlock()
        {
            for (var i = 0; i < this.Count; i++)//10000
            {
                using (var byteBlock = new ByteBlock(1024 * 64))
                {
                    byteBlock.Write(10);
                    byteBlock.Write((byte)1);
                    byteBlock.Write(10.0d);
                    byteBlock.Write('I');
                    byteBlock.Write("love");
                    byteBlock.Write(DateTime.Now);
                    byteBlock.Write(DateTime.Now.TimeOfDay);
                    byteBlock.SeekToStart();
                    byteBlock.ReadInt32();
                    byteBlock.ReadByte();
                    byteBlock.ReadDouble();
                    byteBlock.ReadChar();
                    byteBlock.ReadString();
                    byteBlock.ReadDateTime();
                    byteBlock.ReadTimeSpan();
                }
            }
        }

        [Benchmark]
        public void RunValueByteBlock()
        {
            for (var i = 0; i < this.Count; i++)
            {
                using (var byteBlock = new ValueByteBlock(1024 * 64))
                {
                    byteBlock.Write(10);
                    byteBlock.Write((byte)1);
                    byteBlock.Write(10.0d);
                    byteBlock.Write('I');
                    byteBlock.Write("love");
                    byteBlock.Write(DateTime.Now);
                    byteBlock.Write(DateTime.Now.TimeOfDay);
                    byteBlock.SeekToStart();
                    byteBlock.ReadInt32();
                    byteBlock.ReadByte();
                    byteBlock.ReadDouble();
                    byteBlock.ReadChar();
                    byteBlock.ReadString();
                    byteBlock.ReadDateTime();
                    byteBlock.ReadTimeSpan();
                }
            }
        }

        [Benchmark]
        public void RunByteBlockRef()
        {
            using (var byteBlock = new ByteBlock(1024 * 64))
            {
                int a = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    ByteBlockRef(byteBlock, ref a);
                }

            }
        }

        private static void ByteBlockRef(ByteBlock valueByteBlock, ref int a)
        {
            if (a++ > 10000)
            {
                return;
            }
            ByteBlockRef(valueByteBlock, ref a);
        }

        [Benchmark]
        public void RunValueByteBlockRef()
        {
            using (var byteBlock = new ValueByteBlock(1024 * 64))
            {
                int a = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    ValueByteBlockRef(byteBlock, ref a);
                }

            }
        }

        private static void ValueByteBlockRef(ValueByteBlock valueByteBlock,ref int a)
        {
            if (a++>10000)
            {
                return;
            }
            ValueByteBlockRef(valueByteBlock,ref a);
        }
    }
}