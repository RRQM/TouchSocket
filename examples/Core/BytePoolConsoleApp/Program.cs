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
using TouchSocket.Sockets;

namespace BytePoolConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        CreateArrayPool();
        CreateByteBlock();
        CreateValueByteBlock();
        CreateByteBlockWithCustomPool();
        CreateByteBlockWithUsing();
        BaseWriteRead();
        PrimitiveWriteRead();
        BufferWriterWriteRead();
        HoldingExample();

        Console.ReadKey();
    }

    #region 内存池创建自定义内存池

    private static void CreateArrayPool()
    {
        System.Buffers.ArrayPool<byte> bytePool = System.Buffers.ArrayPool<byte>.Create(maxArrayLength: 1024 * 1024, maxArraysPerBucket: 50);
    }

    #endregion 内存池创建自定义内存池

    #region 内存池创建ByteBlock

    private static void CreateByteBlock()
    {
        var byteBlock = new ByteBlock(1024 * 64);
        byteBlock.Dispose();
    }

    #endregion 内存池创建ByteBlock

    #region 内存池创建ValueByteBlock

    private static void CreateValueByteBlock()
    {
        var byteBlock = new ValueByteBlock(1024 * 64);
        byteBlock.Dispose();
    }

    #endregion 内存池创建ValueByteBlock

    #region 内存池使用自定义内存池创建

    private static void CreateByteBlockWithCustomPool()
    {
        var customPool = System.Buffers.ArrayPool<byte>.Create();
        var byteBlock = new ByteBlock(1024 * 64, (c) => customPool.Rent(c), (m) =>
        {
            if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)m, out var result))
            {
                customPool.Return(result.Array);
            }
        });
        byteBlock.Dispose();
    }

    #endregion 内存池使用自定义内存池创建

    #region 内存池使用using释放

    private static void CreateByteBlockWithUsing()
    {
        using (var byteBlock = new ByteBlock(1024 * 64))
        {
            //使用ByteBlock
        }
    }

    #endregion 内存池使用using释放

    #region 内存池读写数组

    private static void BaseWriteRead()
    {
        using (var byteBlock = new ByteBlock(1024 * 64))
        {
            byteBlock.Write(new byte[] { 0, 1, 2, 3 });//将字节数组写入

            byteBlock.SeekToStart();//将游标重置

            var buffer = new byte[byteBlock.Length];//定义一个数组容器
            var r = byteBlock.Read(buffer);//读取数据到容器，并返回读取的长度r
        }
    }

    #endregion 内存池读写数组

    #region 内存池基础类型读写

    private static void PrimitiveWriteRead()
    {
        var byteBlock = new ByteBlock(1024 * 64);
        try
        {
            WriterExtension.WriteValue(ref byteBlock, byte.MaxValue);//写入byte类型
            WriterExtension.WriteValue(ref byteBlock, int.MaxValue);//写入int类型
            WriterExtension.WriteValue(ref byteBlock, long.MaxValue);//写入long类型
            WriterExtension.WriteString(ref byteBlock, "RRQM");//写入字符串类型

            byteBlock.SeekToStart();//读取时，先将游标移动到初始写入的位置，然后按写入顺序，依次读取

            var byteValue = ReaderExtension.ReadValue<ByteBlock, byte>(ref byteBlock);
            var intValue = ReaderExtension.ReadValue<ByteBlock, int>(ref byteBlock);
            var longValue = ReaderExtension.ReadValue<ByteBlock, long>(ref byteBlock);
            var stringValue = ReaderExtension.ReadString<ByteBlock>(ref byteBlock);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    #endregion 内存池基础类型读写

    #region 内存池BufferWriter方式写入

    private static void BufferWriterWriteRead()
    {
        using (var byteBlock = new ByteBlock(1024 * 64))
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

    #endregion 内存池BufferWriter方式写入

    #region 内存池多线程同步协作

    private static void HoldingExample()
    {
        // 此示例演示了SetHolding的用法
        // 实际使用请参考MyTClient和MyTClientError类
    }

    #endregion 内存池多线程同步协作

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
}

#region 内存池异步Hold错误示例

// 错误示例：直接在异步任务中使用ByteBlock会导致异常
// 因为byteBlock在异步任务开始前就已经被释放了
internal static class HoldErrorExample
{
    public static void HandleData(ByteBlock byteBlock)
    {
        System.Threading.Tasks.Task.Run(() =>
        {
            // 错误：此时byteBlock可能已被释放
            string mes = byteBlock.Span.ToString(Encoding.UTF8);
            Console.WriteLine($"已接收到信息：{mes}");
        });
    }
}

#endregion 内存池异步Hold错误示例

#region 内存池异步Hold正确示例

// 正确示例：使用SetHolding锁定ByteBlock，在异步任务中使用后再解锁
internal static class HoldCorrectExample
{
    public static void HandleData(ByteBlock byteBlock)
    {
        byteBlock.SetHolding(true);//异步前锁定
        System.Threading.Tasks.Task.Run(() =>
        {
            string mes = byteBlock.Span.ToString(Encoding.UTF8);
            byteBlock.SetHolding(false);//使用完成后取消锁定，且不用再调用Dispose
            Console.WriteLine($"已接收到信息：{mes}");
        });
    }
}

#endregion 内存池异步Hold正确示例

internal class MyPackage : PackageBase
{
    public int Property { get; set; }


    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        WriterExtension.WriteValue<TByteBlock, int>(ref byteBlock, this.Property);
    }

    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.Property = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
    }
}

internal static class MyByteBlockExtension
{
    public static void ExtensionWrite<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IBytesWriter
    {
        WriterExtension.WriteValue(ref byteBlock, (short)10);
        WriterExtension.WriteValue(ref byteBlock, 10);
    }
}