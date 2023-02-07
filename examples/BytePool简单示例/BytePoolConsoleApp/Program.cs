using System;
using TouchSocket.Core;

namespace BytePoolConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ReadKey();

            BytePool bytePool = new BytePool();
            BytePool defaultBytePool = BytePool.Default;

            BytePool.Default.AddSizeKey(1024 * 1024);
            //BytePool.AutoZero = true;
            for (int i = 0; i < 5; i++)
            {
                byte[] data = BytePool.Default.GetByteCore(1024 * 10, true);
                BytePool.Default.Recycle(data);
                using (ByteBlock byteBlock = new ByteBlock(1024 * 10, true))
                {
                    //最重要：千万不要引用byteBlock.Buffer
                    byteBlock.Write(10);
                    byteBlock.Write('A');
                    byteBlock.Write(100L);
                    byteBlock.Write(3.1415926);
                    byteBlock.Write("Hello TouchSocket");

                    var buffer = byteBlock.ToArray();

                    byteBlock.Position = 0;

                    var p1 = byteBlock.ReadInt32();
                    var p2 = byteBlock.ReadChar();
                    var p3 = byteBlock.ReadInt64();
                    var p4 = byteBlock.ReadDouble();
                    var p5 = byteBlock.ReadString();
                }
            }

            Console.ReadKey();
        }

        private static void Performance()
        {
            int count = 1000000;
            TimeSpan timeSpan1 = TimeMeasurer.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    byte[] buffer = new byte[1024];
                }
            });

            TimeSpan timeSpan2 = TimeMeasurer.Run(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    ByteBlock byteBlock = new ByteBlock(1024, true);
                    byteBlock.Dispose();
                }
            });

            Console.WriteLine($"直接实例化：{timeSpan1}");
            Console.WriteLine($"内存池实例化：{timeSpan2}");
        }
    }
}