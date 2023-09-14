using System;
using System.Text;
using TouchSocket.Core;

namespace BytePoolConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            NewBytePool();
            BaseWriteRead();
            PrimitiveWriteRead();
            ObjectWriteRead();
            BytesPackageWriteRead();
            IPackageWriteRead();
            IPackageWriteRead();
            Console.ReadKey();
        }

        static void IPackageWriteRead()
        {
            using (var byteBlock = new ByteBlock())
            {
                byteBlock.WritePackage(new MyPackage()
                {
                    Property = 10
                });

                byteBlock.SeekToStart();

                var myPackage = byteBlock.ReadPackage<MyPackage>();
            }
        }

        static void BytesPackageWriteRead()
        {
            using (var byteBlock = new ByteBlock())
            {
                byteBlock.WriteBytesPackage(Encoding.UTF8.GetBytes("TouchSocket"));

                byteBlock.SeekToStart();

                byte[] bytes = byteBlock.ReadBytesPackage();

                byteBlock.SeekToStart();

                //使用下列方式即可高效完成读取
                if (byteBlock.TryReadBytesPackageInfo(out int pos, out int len))
                {
                    var str = Encoding.UTF8.GetString(byteBlock.Buffer, pos, len);
                }
            }
        }

        static void ObjectWriteRead()
        {
            using (var byteBlock = new ByteBlock())
            {
                //将实例写入，实际上是序列化
                byteBlock.WriteObject(new MyClass(), SerializationType.FastBinary);

                byteBlock.SeekToStart();

                //读取实例，实际上是反序列化
                var myClass = byteBlock.ReadObject<MyClass>();
            }
        }

        static void PrimitiveWriteRead()
        {
            using (var byteBlock = new ByteBlock())
            {
                byteBlock.Write(byte.MaxValue);//写入byte类型
                byteBlock.Write(int.MaxValue);//写入int类型
                byteBlock.Write(long.MaxValue);//写入long类型
                byteBlock.Write("RRQM");//写入字符串类型

                byteBlock.SeekToStart();//读取时，先将游标移动到初始写入的位置，然后按写入顺序，依次读取

                byte byteValue = (byte)byteBlock.ReadByte();
                int intValue = byteBlock.ReadInt32();
                long longValue = byteBlock.ReadInt64();
                string stringValue = byteBlock.ReadString();
            }
        }

        static void BaseWriteRead()
        {
            using (var byteBlock = new ByteBlock())
            {
                byteBlock.Write(new byte[] { 0, 1, 2, 3 });//将字节数组写入

                byteBlock.SeekToStart();//将游标重置

                var buffer = new byte[byteBlock.Len];//定义一个数组容器
                var r = byteBlock.Read(buffer);//读取数据到容器，并返回读取的长度r
            }
        }

        static void NewBytePool()
        {
            BytePool bytePool = new BytePool(maxArrayLength: 1024 * 1024, maxArraysPerBucket: 50)
            {
                AutoZero = false,//在回收内存时，是否清空内存
                MaxBucketsToTry = 5//最大梯度跨度
            };
            Console.WriteLine($"内存池容量={bytePool.Capacity}");
            Console.WriteLine($"内存池实际尺寸={bytePool.GetPoolSize()}");
        }

        private static void Performance()
        {
            var count = 1000000;
            var timeSpan1 = TimeMeasurer.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    var buffer = new byte[1024];
                }
            });

            var timeSpan2 = TimeMeasurer.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    var byteBlock = new ByteBlock(1024);
                    byteBlock.Dispose();
                }
            });

            Console.WriteLine($"直接实例化：{timeSpan1}");
            Console.WriteLine($"内存池实例化：{timeSpan2}");
        }
    }

    class MyPackage : PackageBase
    {
        public int Property { get; set; }

        public override void Package(in ByteBlock byteBlock)
        {
            byteBlock.Write(this.Property);
        }

        public override void Unpackage(in ByteBlock byteBlock)
        {
            this.Property = byteBlock.ReadInt32();
        }
    }

    class MyClass
    {
        public int Property { get; set; }
    }
}