// ------------------------------------------------------------------------------
// 此代码版权(除特别声明或在XREF结尾的命名空间的代码)归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议,若本仓库没有设置,则按MIT开源协议授权
// CSDN博客:https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频:https://space.bilibili.com/94253567
// Gitee源代码仓库:https://gitee.com/RRQM_Home
// Github源代码仓库:https://github.com/RRQM
// API首页:https://touchsocket.net/
// 交流QQ群:234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Core;

namespace OtherCoreConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("=== TouchSocket 其他核心功能示例 ===\n");

        // 一、Crc计算示例
        OtherCoreDemo.CrcExample();
        OtherCoreDemo.Crc16ValueExample();

        // 二、时间测量器示例
        OtherCoreDemo.TimeMeasurerExample();
        OtherCoreDemo.TimeMeasurerAsyncExample();

        // 三、MD5计算示例
        OtherCoreDemo.MD5Example();

        // 四、16进制相关示例
        OtherCoreDemo.HexStringExample();

        // 五、雪花Id生成示例
        OtherCoreDemo.SnowflakeIdExample();

        // 六、数据压缩示例
        OtherCoreDemo.GZipExample();
        OtherCoreDemo.CustomDataCompressorExample();

        // 七、短时间戳示例
        OtherCoreDemo.ShortTimestampExample();

        // 八、读写锁using示例
        OtherCoreDemo.ReadWriteLockExample();

        // 九、3DES示例
        OtherCoreDemo.TripleDESExample();

        Console.WriteLine("\n=== 所有示例执行完毕 ===");
        Console.ReadKey();
    }
}

/// <summary>
/// 其他核心功能示例类
/// </summary>
public static class OtherCoreDemo
{
    #region 其他核心Crc计算
    /// <summary>
    /// Crc计算示例
    /// </summary>
    public static void CrcExample()
    {
        Console.WriteLine("【一、Crc计算示例】");

        byte[] data = new byte[10];
        new Random().NextBytes(data);

        // Crc16计算
        byte[] result = Crc.Crc16(data);

        Console.WriteLine($"原始数据: {BitConverter.ToString(data)}");
        Console.WriteLine($"Crc16结果: {BitConverter.ToString(result)}");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心Crc16Value计算
    /// <summary>
    /// Crc16Value计算示例
    /// </summary>
    public static void Crc16ValueExample()
    {
        Console.WriteLine("【Crc16Value计算示例】");

        byte[] data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        // 计算Crc16Value(返回ushort)
        ushort crc16Value = Crc.Crc16Value(data);

        Console.WriteLine($"原始数据: {BitConverter.ToString(data)}");
        Console.WriteLine($"Crc16Value: 0x{crc16Value:X4}");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心时间测量器
    /// <summary>
    /// 时间测量器示例
    /// </summary>
    public static void TimeMeasurerExample()
    {
        Console.WriteLine("【二、时间测量器示例】");

        TimeSpan timeSpan = TimeMeasurer.Run(() =>
        {
            Thread.Sleep(1000);
        });

        Console.WriteLine($"同步操作耗时: {timeSpan.TotalMilliseconds} 毫秒");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心时间测量器异步
    /// <summary>
    /// 时间测量器异步示例
    /// </summary>
    public static void TimeMeasurerAsyncExample()
    {
        Console.WriteLine("【时间测量器异步示例】");

        var timeSpan = TimeMeasurer.Run(async () =>
        {
            await Task.Delay(500);
        });

        Console.WriteLine($"异步操作耗时: {timeSpan.TotalMilliseconds} 毫秒");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心MD5计算
    /// <summary>
    /// MD5计算示例
    /// </summary>
    public static void MD5Example()
    {
        Console.WriteLine("【三、MD5计算示例】");

        string str = MD5.GetMD5Hash("TouchSocket");
        bool b = MD5.VerifyMD5Hash("TouchSocket", str);

        Console.WriteLine($"MD5哈希值: {str}");
        Console.WriteLine($"验证结果: {b}");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心16进制转字节数组
    /// <summary>
    /// 16进制相关示例
    /// </summary>
    public static void HexStringExample()
    {
        Console.WriteLine("【四、16进制相关示例】");

        // 将16进制字符串转换为字节数组
        string hexString = "01-02-03-04-05";
        byte[] bytes = hexString.ByHexStringToBytes("-");

        Console.WriteLine($"16进制字符串: {hexString}");
        Console.WriteLine($"转换为字节数组: {BitConverter.ToString(bytes)}");

        // 将16进制字符串转换为int32
        string hexInt = "12345678";
        int value = hexInt.ByHexStringToInt32();

        Console.WriteLine($"16进制字符串: {hexInt}");
        Console.WriteLine($"转换为int32: 0x{value:X8}");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心雪花Id生成
    /// <summary>
    /// 雪花Id生成示例
    /// </summary>
    public static void SnowflakeIdExample()
    {
        Console.WriteLine("【五、雪花Id生成示例】");

        SnowflakeIdGenerator generator = new SnowflakeIdGenerator(4);

        // 生成多个Id
        for (int i = 0; i < 5; i++)
        {
            long id = generator.NextId();
            Console.WriteLine($"生成的雪花Id: {id}");
        }

        Console.WriteLine();
    }
    #endregion

    #region 其他核心GZip压缩
    /// <summary>
    /// GZip数据压缩示例
    /// </summary>
    public static void GZipExample()
    {
        Console.WriteLine("【六、数据压缩示例】");

        byte[] data = new byte[1024];
        new Random().NextBytes(data);

        using (ByteBlock byteBlock = new ByteBlock(1024 * 64))
        {
            // 压缩
            GZip.Compress(byteBlock, data);
            Console.WriteLine($"原始数据大小: {data.Length} 字节");
            Console.WriteLine($"压缩后大小: {byteBlock.Length} 字节");

            // 解压
            var decompressData = GZip.Decompress(byteBlock.Span).ToArray();
            Console.WriteLine($"解压后大小: {decompressData.Length} 字节");
            Console.WriteLine($"数据一致性: {data.SequenceEqual(decompressData)}");
        }

        Console.WriteLine();
    }
    #endregion

    #region 其他核心自定义压缩器
    /// <summary>
    /// 自定义数据压缩器示例
    /// </summary>
    public static void CustomDataCompressorExample()
    {
        Console.WriteLine("【自定义数据压缩器示例】");

        // 使用GZip压缩器
        IDataCompressor compressor = new GZipDataCompressor();

        byte[] data = new byte[512];
        new Random().NextBytes(data);

        // 压缩
        var compressedBlock = new ByteBlock(1024);
        compressor.Compress(ref compressedBlock, data);
        Console.WriteLine($"原始数据大小: {data.Length} 字节");
        Console.WriteLine($"压缩后大小: {compressedBlock.Length} 字节");

        // 解压
        var decompressedBlock = new ByteBlock(1024);
        compressor.Decompress(ref decompressedBlock, compressedBlock.Span);
        Console.WriteLine($"解压后大小: {decompressedBlock.Length} 字节");
        Console.WriteLine($"数据一致性: {data.SequenceEqual(decompressedBlock.Span.ToArray())}");

        // 释放资源
        compressedBlock.Dispose();
        decompressedBlock.Dispose();

        Console.WriteLine();
    }
    #endregion

    #region 其他核心短时间戳
    /// <summary>
    /// 短时间戳示例
    /// </summary>
    public static void ShortTimestampExample()
    {
        Console.WriteLine("【七、短时间戳示例】");

        // 获取当前时间的短时间戳(自1970年1月1日以来的毫秒数,uint类型)
        uint timestamp = DateTime.Now.ToUnsignedMillis();
        Console.WriteLine($"短时间戳(uint): {timestamp}");

        // 使用DateTimeOffset
        timestamp = DateTimeOffset.Now.ToUnsignedMillis();
        Console.WriteLine($"DateTimeOffset短时间戳: {timestamp}");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心读写锁Using
    /// <summary>
    /// 读写锁using示例
    /// </summary>
    public static void ReadWriteLockExample()
    {
        Console.WriteLine("【八、读写锁using示例】");

        ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
        int sharedResource = 0;

        // 读锁示例
        using (new ReadLock(lockSlim))
        {
            Console.WriteLine($"读取共享资源: {sharedResource}");
        }

        // 写锁示例
        using (new WriteLock(lockSlim))
        {
            sharedResource = 100;
            Console.WriteLine($"写入共享资源: {sharedResource}");
        }

        // 再次读取
        using (new ReadLock(lockSlim))
        {
            Console.WriteLine($"再次读取共享资源: {sharedResource}");
        }

        lockSlim.Dispose();
        Console.WriteLine();
    }
    #endregion

    #region 其他核心3DES加密
    /// <summary>
    /// 3DES加密解密示例
    /// </summary>
    public static void TripleDESExample()
    {
        Console.WriteLine("【九、3DES加密解密示例】");

        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello TouchSocket!");
        Console.WriteLine($"原始数据: {System.Text.Encoding.UTF8.GetString(data)}");

        // 加密(口令长度为8)
        var dataLocked = DataSecurity.EncryptDES(data, "12345678");
        Console.WriteLine($"加密后: {BitConverter.ToString(dataLocked)}");

        // 解密
        var newData = DataSecurity.DecryptDES(dataLocked, "12345678");
        Console.WriteLine($"解密后: {System.Text.Encoding.UTF8.GetString(newData)}");
        Console.WriteLine($"数据一致性: {data.SequenceEqual(newData)}");
        Console.WriteLine();
    }
    #endregion

    #region 其他核心自定义压缩器接口实现
    /// <summary>
    /// 自定义压缩器实现示例
    /// </summary>
    class MyDataCompressor : IDataCompressor
    {
        public void Compress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> data)
            where TWriter : IBytesWriter
        {
            //此处实现压缩
            throw new NotImplementedException();
        }

        public void Decompress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> data)
            where TWriter : IBytesWriter
        {
            //此处实现解压
            throw new NotImplementedException();
        }
    }
    #endregion

    #region 其他核心读写锁传统用法
    /// <summary>
    /// 读写锁传统用法示例
    /// </summary>
    public static void TraditionalReadWriteLockExample()
    {
        ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
        try
        {
            lockSlim.EnterReadLock();
            //do something
        }
        finally
        {
            lockSlim.ExitReadLock();
        }
    }
    #endregion
}
