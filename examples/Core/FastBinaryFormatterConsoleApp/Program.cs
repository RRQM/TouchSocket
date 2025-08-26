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

using TouchSocket.Core;

namespace FastBinaryFormatterConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        SerializeAndDeserialize(new MyClass4());

        TestMyClass2_3();
        TestMyClass1();

        SerializeAndDeserialize(10);
        SerializeAndDeserialize("RRQM");

        SerializeAndDeserializeFromValueByteBlock(10);
        SerializeAndDeserializeFromValueByteBlock("RRQM");

        SerializeAndDeserializeFromBytes(10);
        SerializeAndDeserializeFromBytes("RRQM");

        FastBinaryFormatter.AddFastBinaryConverter(typeof(MyClass5), new MyClass5FastBinaryConverter());

        Console.ReadKey();
    }

    public static void TestMyClass2_3()
    {
        var myClass2 = new MyClass2()
        {
            P1 = 10
        };
        var bytes = FastBinaryFormatter.SerializeToBytes(myClass2);

        var newObj = FastBinaryFormatter.Deserialize<MyClass3>(bytes);

        var ss = newObj.ToJsonString();

    }


    public static void TestMyClass1()
    {
        var myClass1 = new MyClass1();
        myClass1.P1 = 10;
        myClass1.SetP2(20);
        myClass1.P3 = 30;
        myClass1.SetP4(40);
        myClass1.SetP5(50);
        myClass1.P6 = 60;
        myClass1.SetP7(70);

        SerializeAndDeserialize(myClass1);
    }


    public static void SerializeAndDeserialize<TValue>(TValue value)
    {
        //申请内存块，并指定此次序列化可能使用到的最大尺寸。
        //合理的尺寸设置可以避免内存块扩张。
        using (var block = new ByteBlock(1024 * 64))
        {
            //将数据序列化到内存块
            FastBinaryFormatter.Serialize(block, value);

            Console.WriteLine($"ByteBlock序列化“{typeof(TValue)}”完成，数据长度={block.Length}");

            //在反序列化前，将内存块数据游标移动至正确位。
            block.SeekToStart();

            //反序列化
            var newObj = FastBinaryFormatter.Deserialize<TValue>(block);

            Console.WriteLine($"ByteBlock反序列化“{typeof(TValue)}”完成，Value={newObj.ToJsonString()}");
        }
    }

    public static void SerializeAndDeserializeFromValueByteBlock<TValue>(TValue value)
    {
        //申请内存块，并指定此次序列化可能使用到的最大尺寸。
        //合理的尺寸设置可以避免内存块扩张。
        var block = new ValueByteBlock(1024 * 64);

        try
        {
            //将数据序列化到内存块
            FastBinaryFormatter.Serialize(ref block, value);

            Console.WriteLine($"ValueByteBlock序列化“{typeof(TValue)}”完成，数据长度={block.Length}");

            //在反序列化前，将内存块数据游标移动至正确位。
            block.SeekToStart();

            //反序列化
            var newObj = FastBinaryFormatter.Deserialize<TValue>(ref block);

            Console.WriteLine($"ValueByteBlock反序列化“{typeof(TValue)}”完成，Value={newObj.ToJsonString()}");
        }
        finally
        {
            //因为使用了ref block，所以无法使用using，只能使用try-finally
            block.Dispose();
        }
    }

    public static void SerializeAndDeserializeFromBytes<TValue>(TValue value)
    {
        var bytes = FastBinaryFormatter.SerializeToBytes(value);

        Console.WriteLine($"Bytes序列化“{typeof(TValue)}”完成，数据长度={bytes.Length}");


        var newObj = FastBinaryFormatter.Deserialize<TValue>(bytes);

        Console.WriteLine($"Bytes反序列化“{typeof(TValue)}”完成，Value={newObj.ToJsonString()}");
    }
}

public class MyClass1
{
    private int m_p5;

    //公共属性，有效
    public int P1 { get; set; }

    //自动公共属性，即使包含set访问器，但private，无效
    public int P2 { get; private set; }

    //公共字段，有效
    public int P3;

    //私有字段，无效
    private int P4;

    //公共属性，不包含set访问器，无效
    public int P5 => this.m_p5;

    //公共属性，但忽略，无效
    [FastNonSerialized]
    public int P6 { get; set; }

    //自动公共属性，包含set访问器，即使private，但因为FastSerialized后，有效
    [FastSerialized]
    public int P7 { get; private set; }

    public void SetP4(int value)
    {
        this.P4 = value;
    }

    public void SetP2(int value)
    {
        this.P2 = value;
    }
    public void SetP5(int value)
    {
        this.m_p5 = value;
    }
    public void SetP7(int value)
    {
        this.P7 = value;
    }
}

public class MyClass2
{
    public int P1 { get; set; }
}

public class MyClass3
{
    public int P1 { get; set; }
    public string P2 { get; set; }
}

[FastSerialized(EnableIndex = true)]
public class MyClass4
{
    [FastMember(1)]
    public int MyProperty1 { get; set; }

    [FastMember(2)]
    public int MyProperty2 { get; set; }
}

[FastConverter(typeof(MyClass5FastBinaryConverter))]
public class MyClass5
{
    public int P1 { get; set; }
    public int P2 { get; set; }
}

public sealed class MyClass5FastBinaryConverter : FastBinaryConverter<MyClass5>
{
    protected override MyClass5 Read<TByteBlock>(ref TByteBlock byteBlock, Type type)
    {
        //此处不用考虑为null的情况
        //我们只需要把有效信息按写入的顺序，读取即可。

        var myClass5 = new MyClass5();
        myClass5.P1 = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
        myClass5.P2 = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);

        return myClass5;
    }

    protected override void Write<TByteBlock>(ref TByteBlock byteBlock, in MyClass5 obj)
    {
        //此处不用考虑为null的情况
        //我们只需要把有效信息写入即可。
        //对于MyClass5类，只有两个属性是有效的。

        //所以，依次写入属性值即可
        WriterExtension.WriteValue(ref byteBlock, obj.P1);
        WriterExtension.WriteValue(ref byteBlock, obj.P2);

    }
}


[GeneratorPackage]
public partial class MyClass6 : PackageBase
{
    public int P1 { get; set; }
    public int P2 { get; set; }
}
