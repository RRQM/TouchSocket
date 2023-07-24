using System;
using TouchSocket.Core;

namespace BytePoolConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ReadKey();

            var byteBlock1 = new ByteBlock(byteSize: 1024 * 1024);
            byteBlock1.Dispose();

            var byteBlock2 = BytePool.Default.GetByteBlock(byteSize: 1024 * 1024);
            byteBlock2.Dispose();

            using (var byteBlock3 = new ByteBlock())
            {
            }

            //BytePool.AutoZero = true;
            for (var i = 0; i < 5; i++)
            {
                var data = BytePool.Default.Rent(1024 * 10);
                BytePool.Default.Return(data);
                using (var byteBlock = new ByteBlock(1024 * 10))
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
}