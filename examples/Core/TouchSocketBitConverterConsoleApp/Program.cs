// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Runtime.InteropServices;
using TouchSocket.Core;

namespace TouchSocketBitConverterConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        TouchSocketBitConverterDemo.RunAll();
    }
}
/// <summary>
/// TouchSocketBitConverter 使用示例
/// </summary>
public static class TouchSocketBitConverterDemo
{
    public static void RunAll()
    {
        BasicGetBytesAndTo();
        ChangeDefaultEndian();
        WriteBytesIntoExistingBuffer();
        ToValuesFromByteSequence();
        ConvertValuesBetweenTypes();
        ConvertBoolBitPacking();
        CrossEndianBulkConversion();
        StructReadWrite();
    }

    #region 基本GetBytes与To示例
    private static void BasicGetBytesAndTo()
    {
        var value = 0x12345678;

        // 按小端获取字节
        var littleBytes = TouchSocketBitConverter.LittleEndian.GetBytes(value);
        // 按大端获取字节
        var bigBytes = TouchSocketBitConverter.BigEndian.GetBytes(value);

        Console.WriteLine($"小端字节: {ToHex(littleBytes.Span)}");
        Console.WriteLine($"大端字节: {ToHex(bigBytes.Span)}");

        // 使用与写入相同的端解析
        var v1 = TouchSocketBitConverter.LittleEndian.To<int>(littleBytes.Span);
        var v2 = TouchSocketBitConverter.BigEndian.To<int>(bigBytes.Span);

        // 交叉解析（故意错误端），会得到不同数值
        var wrong = TouchSocketBitConverter.BigEndian.To<int>(littleBytes.Span);

        Console.WriteLine($"正确解析小端 => {v1:X8}");
        Console.WriteLine($"正确解析大端 => {v2:X8}");
        Console.WriteLine($"以大端错误解析小端数据 => {wrong:X8}");
        Console.WriteLine();
    }
    #endregion

    #region 大小端转换器转换整型
    private static void ConvertInt32Example()
    {
        // int -> byte[]
        var data = TouchSocketBitConverter.BigEndian.GetBytes(0x12345678);

        // byte[] -> int
        int value = TouchSocketBitConverter.BigEndian.To<int>(data.Span);
    }
    #endregion

    #region 大小端转换器转换浮点型
    private static void ConvertFloatExample()
    {
        // float -> byte[]
        float num = 3.14f;
        var bytes = TouchSocketBitConverter.LittleEndian.GetBytes(num);

        // byte[] -> float
        float result = TouchSocketBitConverter.LittleEndian.To<float>(bytes.Span);
    }
    #endregion

    #region 大小端转换器转换长整型
    private static void ConvertInt64Example()
    {
        // long -> byte[]
        long bigValue = 0x123456789ABCDEF0;
        var data = TouchSocketBitConverter.Default.GetBytes(bigValue);

        // byte[] -> long
        long restored = TouchSocketBitConverter.Default.To<long>(data.Span);
    }
    #endregion

    #region 大小端转换器转换高精度小数
    private static void ConvertDecimalExample()
    {
        // decimal -> byte[]
        decimal money = 123456.78m;
        var decimalBytes = TouchSocketBitConverter.BigEndian.GetBytes(money);

        // byte[] -> decimal
        decimal result = TouchSocketBitConverter.BigEndian.To<decimal>(decimalBytes.Span);
    }
    #endregion

    #region 大小端转换器不安全内存操作
    private static void UnsafeMemoryExample()
    {
        byte[] buffer = new byte[8];
        long value = 0x1122334455667788;

        // 直接内存写入
        TouchSocketBitConverter.LittleEndian.WriteBytes(buffer, value);

        // 直接内存读取
        long readValue = TouchSocketBitConverter.LittleEndian.To<long>(buffer);
    }
    #endregion

    #region 大小端转换器布尔数组转换
    private static void ConvertBoolArrayExample()
    {
        // bool[] -> byte[]
        bool[] flags = { true, false, true, true, false, true, false, true };
        var byteCount = TouchSocketBitConverter.GetConvertedLength<bool, byte>(flags.Length);
        byte[] boolBytes = new byte[byteCount];
        TouchSocketBitConverter.ConvertValues<bool, byte>(flags, boolBytes);

        // byte[] -> bool[]
        bool[] decodedFlags = new bool[flags.Length];
        TouchSocketBitConverter.ConvertValues<byte, bool>(boolBytes, decodedFlags);
    }
    #endregion

    #region 大小端转换器字节序验证
    private static void CheckEndianCompatibility()
    {
        // 检查当前配置是否与系统字节序一致
        bool isCompatible = TouchSocketBitConverter.Default.IsSameOfSet();
    }
    #endregion

    #region 大小端转换器交换端序模式
    private static void SwapEndianExample()
    {
        // 使用 BigSwap 模式处理网络数据包
        var swappedData = TouchSocketBitConverter.BigSwapEndian.GetBytes(0xAABBCCDD);
    }
    #endregion

    #region 大小端转换器网络数据包处理
    private static void NetworkPacketExample()
    {
        // 配置全局大端模式
        TouchSocketBitConverter.DefaultEndianType = EndianType.Big;

        // 构造数据包
        var packet = new byte[16];
        int header = 0x4D534754;  // "MSGT"
        float payload = 1024.5f;

        // 写入数据
        TouchSocketBitConverter.Default.WriteBytes(packet.AsSpan(0, 4), header);
        TouchSocketBitConverter.Default.WriteBytes(packet.AsSpan(4, 4), payload);

        // 解析数据
        int parsedHeader = TouchSocketBitConverter.Default.To<int>(packet.AsSpan(0, 4));
        float parsedPayload = TouchSocketBitConverter.Default.To<float>(packet.AsSpan(4, 4));
    }
    #endregion

    #region 修改默认字节序示例
    private static void ChangeDefaultEndian()
    {
        Console.WriteLine($"当前默认端: {TouchSocketBitConverter.DefaultEndianType}");
        TouchSocketBitConverter.DefaultEndianType = EndianType.Big;
        Console.WriteLine($"修改后默认端: {TouchSocketBitConverter.DefaultEndianType}");

        // Default引用自动指向新的实例
        var bytes = TouchSocketBitConverter.Default.GetBytes((short)0x1234);
        Console.WriteLine($"默认端(大端)写出: {ToHex(bytes.Span)}");

        // 还原
        TouchSocketBitConverter.DefaultEndianType = EndianType.Little;
        Console.WriteLine($"还原默认端: {TouchSocketBitConverter.DefaultEndianType}");
        Console.WriteLine();
    }
    #endregion

    #region WriteBytes写入既有缓冲区示例
    private static void WriteBytesIntoExistingBuffer()
    {
        var value = 0x0A0B0C0D;
        Span<byte> buffer = stackalloc byte[4];

        TouchSocketBitConverter.BigEndian.WriteBytes(buffer, value);
        Console.WriteLine($"大端写入: {ToHex(buffer)}");

        TouchSocketBitConverter.LittleEndian.WriteBytes(buffer, value);
        Console.WriteLine($"小端覆盖: {ToHex(buffer)}");
        Console.WriteLine();
    }
    #endregion

    #region ToValues将字节批量转为数组示例
    private static void ToValuesFromByteSequence()
    {
        // 准备 3 个UInt16（小端）
        ushort[] data = { 0x1122, 0x3344, 0x5566 };
        var bytes = new byte[data.Length * 2];
        var conv = TouchSocketBitConverter.LittleEndian;
        var span = bytes.AsSpan();
        for (var i = 0; i < data.Length; i++)
        {
            conv.WriteBytes(span.Slice(i * 2, 2), data[i]);
        }

        Console.WriteLine($"原始字节(小端): {ToHex(bytes)}");

        // 按小端解析
        var u16 = TouchSocketBitConverter.ToValues<ushort>(bytes, EndianType.Little);
        Console.WriteLine("解析结果: " + string.Join(", ", u16.Span.ToArray().Select(v => $"0x{v:X4}")));
        Console.WriteLine();
    }
    #endregion

    #region ConvertValues类型批量转换示例
    private static void ConvertValuesBetweenTypes()
    {
        int[] ints = { 1, 2, 3, 0x10203040 };
        // int[] -> byte[]
        var byteCount = TouchSocketBitConverter.GetConvertedLength<int, byte>(ints.Length);
        var buffer = new byte[byteCount];
        TouchSocketBitConverter.ConvertValues<int, byte>(ints, buffer);

        Console.WriteLine($"int数组 => byte: {ToHex(buffer)}");

        // 反向 byte -> int
        var back = new int[ints.Length];
        TouchSocketBitConverter.ConvertValues<byte, int>(buffer, back);
        Console.WriteLine("还原int: " + string.Join(", ", back.Select(v => $"0x{v:X8}")));
        Console.WriteLine();
    }
    #endregion

    #region ConvertValues布尔位打包示例
    private static void ConvertBoolBitPacking()
    {
        bool[] bools =
        {
                true,false,true,true, false,false,false,true,
                true,true,false,false,false,false,false,false
            };

        // bool -> byte（按位打包）
        var byteLength = TouchSocketBitConverter.GetConvertedLength<bool, byte>(bools.Length);
        var packed = new byte[byteLength];
        TouchSocketBitConverter.ConvertValues<bool, byte>(bools, packed);
        Console.WriteLine($"bool打包 => {ToHex(packed)} (总位:{bools.Length})");

        // byte -> bool
        var unpacked = new bool[bools.Length];
        TouchSocketBitConverter.ConvertValues<byte, bool>(packed, unpacked);
        Console.WriteLine("解包结果: " + string.Join("", unpacked.Select(b => b ? '1' : '0')));
        Console.WriteLine();
    }
    #endregion

    #region ConvertValues跨端批量转换示例
    private static void CrossEndianBulkConversion()
    {
        uint[] values = { 0x11223344, 0xAABBCCDD };
        // 源认为大端存储 -> 转成小端字节序的byte[]
        var byteLength = TouchSocketBitConverter.GetConvertedLength<uint, byte>(values.Length);
        var littleBytes = new byte[byteLength];
        TouchSocketBitConverter.ConvertValues<uint, byte>(values, littleBytes, EndianType.Big, EndianType.Little);
        Console.WriteLine($"大端uint -> 小端byte: {ToHex(littleBytes)}");

        // 再按小端解释为uint
        var round = new uint[values.Length];
        TouchSocketBitConverter.ConvertValues<byte, uint>(littleBytes, round, EndianType.Little, EndianType.Little);
        Console.WriteLine("还原uint: " + string.Join(", ", round.Select(v => $"0x{v:X8}")));
        Console.WriteLine();
    }
    #endregion

    #region 自定义结构读写示例
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MyPacket
    {
        public ushort Id;
        public int Value;
        public byte Flag;
    }

    private static void StructReadWrite()
    {
        var pkt = new MyPacket
        {
            Id = 0x1234,
            Value = 0x11223344,
            Flag = 0x5A
        };

        // 逐字段写入（控制端）
        Span<byte> buffer = stackalloc byte[2 + 4 + 1];
        var big = TouchSocketBitConverter.BigEndian;
        big.WriteBytes(buffer.Slice(0, 2), pkt.Id);
        big.WriteBytes(buffer.Slice(2, 4), pkt.Value);
        buffer[6] = pkt.Flag;

        Console.WriteLine($"MyPacket序列化(大端): {ToHex(buffer)}");

        // 读取
        var id = big.To<ushort>(buffer.Slice(0, 2));
        var val = big.To<int>(buffer.Slice(2, 4));
        var flag = buffer[6];

        Console.WriteLine($"解析 => Id=0x{id:X4}, Value=0x{val:X8}, Flag=0x{flag:X2}");
        Console.WriteLine();
    }
    #endregion

    #region 工具方法
    private static string ToHex(ReadOnlySpan<byte> span)
    {
        if (span.Length == 0)
        {
            return string.Empty;
        }

        // 每个字节需要2个十六进制字符，字节间需要1个空格（除了最后一个）
        var resultLength = span.Length * 3 - 1;
        Span<char> chars = stackalloc char[resultLength];

        const string hexChars = "0123456789ABCDEF";
        var pos = 0;

        for (var i = 0; i < span.Length; i++)
        {
            var b = span[i];
            chars[pos++] = hexChars[b >> 4];      // 高4位
            chars[pos++] = hexChars[b & 0x0F];    // 低4位

            if (i < span.Length - 1)  // 不是最后一个字节时添加空格
            {
                chars[pos++] = ' ';
            }
        }

        return new string(chars);
    }
    #endregion
}