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

using System;
using System.Text;
using TouchSocket.Core;

namespace BytePoolConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        BaseWriteRead();
        BufferWriterWriteRead();
        PrimitiveWriteRead();
        BytesPackageWriteRead();
        IPackageWriteRead();
        IPackageWriteRead();
        Console.ReadKey();
    }

    private static void ExtensionWrite()
    {
        var byteBlock = new ValueByteBlock(1024);
        try
        {
            MyByteBlockExtension.ExtensionWrite(ref byteBlock);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    private static void IPackageWriteRead()
    {
        using (var byteBlock = new ByteBlock(1024*64))
        {
            byteBlock.WritePackage(new MyPackage()
            {
                Property = 10
            });
            byteBlock.SeekToStart();

            var myPackage = byteBlock.ReadPackage<MyPackage>();
        }
    }

    private static void BytesPackageWriteRead()
    {
        using (var byteBlock = new ByteBlock(1024*64))
        {
            byteBlock.WriteBytesPackage(Encoding.UTF8.GetBytes("TouchSocket"));

            byteBlock.SeekToStart();

            var bytes = byteBlock.ReadBytesPackage();

            byteBlock.SeekToStart();

            //使用下列方式即可高效完成读取
            var memory = byteBlock.ReadBytesPackageMemory();

        }
    }

    private static void PrimitiveWriteRead()
    {
        using (var byteBlock = new ByteBlock(1024*64))
        {
            byteBlock.WriteByte(byte.MaxValue);//写入byte类型
            byteBlock.WriteInt32(int.MaxValue);//写入int类型
            byteBlock.WriteInt64(long.MaxValue);//写入long类型
            byteBlock.WriteString("RRQM");//写入字符串类型

            byteBlock.SeekToStart();//读取时，先将游标移动到初始写入的位置，然后按写入顺序，依次读取

            var byteValue = byteBlock.ReadByte();
            var intValue = byteBlock.ReadInt32();
            var longValue = byteBlock.ReadInt64();
            var stringValue = byteBlock.ReadString();
        }
    }

    private static void BufferWriterWriteRead()
    {
        using (var byteBlock = new ByteBlock(1024*64))
        {
            var span = byteBlock.GetSpan(4);
            span[0] = 0;
            span[1] = 1;
            span[2] = 2;
            span[3] = 3;
            byteBlock.Advance(4);

            var memory = byteBlock.GetMemory(4);
            memory.Span[0] = 4;
            memory.Span[1] = 5;
            memory.Span[2] = 6;
            memory.Span[3] = 7;
            byteBlock.Advance(4);

            //byteBlock.Length 应该是8
        }
    }

    private static void BaseWriteRead()
    {
        using (var byteBlock = new ByteBlock(1024*64))
        {
            byteBlock.Write(new byte[] { 0, 1, 2, 3 });//将字节数组写入

            byteBlock.SeekToStart();//将游标重置

            var buffer = new byte[byteBlock.Length];//定义一个数组容器
            var r = byteBlock.Read(buffer);//读取数据到容器，并返回读取的长度r
        }
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

internal class MyPackage : PackageBase
{
    public int Property { get; set; }

    /*新写法*/
    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.WriteInt32(this.Property);
    }

    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.Property = byteBlock.ReadInt32();
    }

    /*旧写法*/
    //public override void Package(in ByteBlock byteBlock)
    //{
    //    byteBlock.Write(this.Property);
    //}
    //public override void Unpackage(in ByteBlock byteBlock)
    //{
    //    this.Property = byteBlock.ReadInt32();
    //}

}

internal class MyClass
{
    public int Property { get; set; }
}

internal static class MyByteBlockExtension
{
    public static void ExtensionWrite<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.WriteInt16(10);
        byteBlock.WriteInt32(10);
    }
}