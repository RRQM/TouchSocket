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

using System.Drawing;
using TouchSocket.Core;

namespace PackageConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        TestMyClassFromByteBlock();

        {
            //测试手动打包和解包
            var myClass = new MyPackage();
            myClass.P1 = 1;
            myClass.P2 = "若汝棋茗";
            myClass.P3 = 'a';
            myClass.P4 = 3;

            myClass.P5 = new List<int> { 1, 2, 3 };

            myClass.P6 = new Dictionary<int, MyClassModel>()
        {
            { 1,new MyClassModel(){ P1=DateTime.Now} },
            { 2,new MyClassModel(){ P1=DateTime.Now} }
        };

            var byteBlock = new ByteBlock(1024*64);
            try
            {
                myClass.Package(ref byteBlock);//打包，相当于序列化

                byteBlock.SeekToStart();//将流位置重置为0

                var myNewClass = new MyPackage();
                myNewClass.Unpackage(ref byteBlock);//解包，相当于反序列化
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        {
            //测试源生成打包和解包
            var myClass = new MyGeneratorPackage();
            myClass.P1 = 1;
            myClass.P2 = "若汝棋茗";
            myClass.P3 = 'a';
            myClass.P4 = 3;

            myClass.P5 = new List<int> { 1, 2, 3 };

            myClass.P6 = new Dictionary<int, MyClassModel>()
        {
            { 1,new MyClassModel(){ P1=DateTime.Now} },
            { 2,new MyClassModel(){ P1=DateTime.Now} }
        };

            var byteBlock = new ByteBlock(1024*64);

            try
            {
                myClass.Package(ref byteBlock);//打包，相当于序列化

                byteBlock.SeekToStart();//将流位置重置为0

                var myNewClass = new MyGeneratorPackage();
                myNewClass.Unpackage(ref byteBlock);//解包，相当于反序列化
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }


    public static void TestMyClassFromByteBlock()
    {
        //声明内存大小。
        //在打包时，一般会先估算一下包的最大尺寸，避免内存块扩张带来的性能损失。
        using (var byteBlock = new ByteBlock(1024 * 64))
        {
            //初始化对象
            var myClass = new MyClass()
            {
                P1 = 10,
                P2 = "RRQM"
            };

            myClass.Package(byteBlock);
            Console.WriteLine($"打包完成，长度={byteBlock.Length}");

            //在解包时，需要把游标移动至正确位置，此处为0.
            byteBlock.SeekToStart();

            //先新建对象
            var newMyClass = new MyClass();
            newMyClass.Unpackage(byteBlock);
            Console.WriteLine($"解包完成，{newMyClass.ToJsonString()}");
        }
    }

    public static void TestMyClassFromValueByteBlock()
    {
        //声明内存大小。
        //在打包时，一般会先估算一下包的最大尺寸，避免内存块扩张带来的性能损失。

        var byteBlock = new ValueByteBlock(1024 * 64);

        try
        {
            //初始化对象
            var myClass = new MyClass()
            {
                P1 = 10,
                P2 = "RRQM"
            };

            myClass.Package(ref byteBlock);
            Console.WriteLine($"打包完成，长度={byteBlock.Length}");

            //在解包时，需要把游标移动至正确位置，此处为0.
            byteBlock.SeekToStart();

            //先新建对象
            var newMyClass = new MyClass();
            newMyClass.Unpackage(ref byteBlock);
            Console.WriteLine($"解包完成，{newMyClass.ToJsonString()}");
        }
        finally
        {
            byteBlock.Dispose();
        }

    }
}

internal class MyPackage : PackageBase
{
    public int P1 { get; set; }
    public string P2 { get; set; }
    public char P3 { get; set; }
    public double P4 { get; set; }
    public List<int> P5 { get; set; }
    public Dictionary<int, MyClassModel> P6 { get; set; }

    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        //基础类型直接写入。
        WriterExtension.WriteValue(ref byteBlock,(int)this.P1);
        WriterExtension.WriteString(ref byteBlock,(string)this.P2);
        WriterExtension.WriteValue(ref byteBlock, (char)this.P3);
        WriterExtension.WriteValue(ref byteBlock,(double)this.P4);

        //集合类型，可以先判断是否为null
        WriterExtension.WriteIsNull(ref byteBlock,this.P5);
        if (this.P5 != null)
        {
            //如果不为null
            //就先写入集合长度
            //然后遍历将每个项写入
            WriterExtension.WriteValue(ref byteBlock,(int)this.P5.Count);
            foreach (var item in this.P5)
            {
                WriterExtension.WriteValue(ref byteBlock,(int)item);
            }
        }

        //字典类型，可以先判断是否为null
        WriterExtension.WriteIsNull(ref byteBlock,this.P6);
        if (this.P6 != null)
        {
            //如果不为null
            //就先写入字典长度
            //然后遍历将每个项，按键、值写入
            WriterExtension.WriteValue(ref byteBlock,(int)this.P6.Count);
            foreach (var item in this.P6)
            {
                WriterExtension.WriteValue(ref byteBlock,(int)item.Key);
                WriterExtension.WritePackage(ref byteBlock,item.Value);//因为值MyClassModel实现了IPackage，所以可以直接写入
            }
        }
    }

    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        //基础类型按序读取。
        this.P1 = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
        this.P2 = ReaderExtension.ReadString<TByteBlock>(ref byteBlock);
        this.P3 = ReaderExtension.ReadValue<TByteBlock, char>(ref byteBlock);
        this.P4 = ReaderExtension.ReadValue<TByteBlock, double>(ref byteBlock);

        var isNull = ReaderExtension.ReadIsNull<TByteBlock>(ref byteBlock);
        if (!isNull)
        {
            var count = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
            var list = new List<int>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock));
            }
            this.P5 = list;
        }

        isNull = ReaderExtension.ReadIsNull<TByteBlock>(ref byteBlock);//复用前面的变量，省的重新声明
        if (!isNull)
        {
            var count = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
            var dic = new Dictionary<int, MyClassModel>(count);
            for (var i = 0; i < count; i++)
            {
                var key = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
                //因为值MyClassModel实现了IPackage，所以可以直接读取
                var value = ReaderExtension.ReadPackage<TByteBlock, MyClassModel>(ref byteBlock);
                dic.Add(key, value);
            }
            this.P6 = dic;
        }
    }
}

/// <summary>
/// 使用源生成包序列化。
/// 也就是不需要手动Package和Unpackage
/// </summary>
[GeneratorPackage]
internal partial class MyGeneratorPackage : PackageBase
{
    public int P1 { get; set; }
    public string P2 { get; set; }
    public char P3 { get; set; }
    public double P4 { get; set; }
    public List<int> P5 { get; set; }
    public Dictionary<int, MyClassModel> P6 { get; set; }

    [PackageMember(Behavior = PackageBehavior.Ignore)]
    public string P7 { get; set; }

    [PackageMember(Behavior = PackageBehavior.Include)]
    private int P8;

    [PackageMember(Index = -1)]
    public string P9 { get; set; }
}


[GeneratorPackage]
internal partial class MyGeneratorIndexPackage : PackageBase
{
    [PackageMember(Index = 2)]
    public int P1 { get; private set; }

    [PackageMember(Index = 0)]
    public string P2 { get; set; }

    [PackageMember(Index = 1)]
    public char P3 { get; set; }
}

[GeneratorPackage]
internal partial class MyGeneratorConvertPackage : PackageBase
{
    [PackageMember(Converter = typeof(RectangleConverter))]
    public Rectangle P1 { get; set; }
}

internal class RectangleConverter : FastBinaryConverter<Rectangle>
{
    protected override Rectangle Read<TByteBlock>(ref TByteBlock byteBlock, Type type)
    {
        var rectangle = new Rectangle(ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock), ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock), ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock), ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock));
        return rectangle;
    }

    protected override void Write<TByteBlock>(ref TByteBlock byteBlock, in Rectangle obj)
    {
        WriterExtension.WriteValue(ref byteBlock,(int)obj.X);
        WriterExtension.WriteValue(ref byteBlock,(int)obj.Y);
        WriterExtension.WriteValue(ref byteBlock,(int)obj.Width);
        WriterExtension.WriteValue(ref byteBlock,(int)obj.Height);
    }
}

public class MyClassModel : PackageBase
{
    public DateTime P1 { get; set; }

    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        WriterExtension.WriteValue(ref byteBlock,(DateTime)this.P1);
    }

    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        this.P1 = ReaderExtension.ReadValue<TByteBlock, DateTime>(ref byteBlock);
    }
}

public class MyClass : PackageBase
{
    public int P1 { get; set; }
    public string P2 { get; set; }

    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        //将P1与P2属性按类型依次写入
        WriterExtension.WriteValue(ref byteBlock,(int)this.P1);
        WriterExtension.WriteString(ref byteBlock,(string)this.P2);
    }

    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        //将P1与P2属性按类型依次读取
        this.P1 = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
        this.P2 = ReaderExtension.ReadString<TByteBlock>(ref byteBlock);
    }
}

public class MyArrayClass : PackageBase
{
    public int[] P5 { get; set; }

    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        //集合类型，可以先判断集合是否为null
        WriterExtension.WriteIsNull(ref byteBlock,this.P5);
        if (this.P5 != null)
        {
            //如果不为null
            //就先写入集合长度
            //然后遍历将每个项写入
            WriterExtension.WriteValue(ref byteBlock,(int)this.P5.Length);
            foreach (var item in this.P5)
            {
                WriterExtension.WriteValue(ref byteBlock,(int)item);
            }
        }
    }

    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        var isNull_P5 = ReaderExtension.ReadIsNull<TByteBlock>(ref byteBlock);
        if (!isNull_P5)
        {
            //有值
            var count = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
            var array = new int[count];
            for (var i = 0; i < count; i++)
            {
                array[i] = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
            }

            //赋值
            this.P5 = array;
        }
    }
}

public class MyDictionaryClass : PackageBase
{
    public Dictionary<int, MyClassModel> P6 { get; set; }

    public override void Package<TByteBlock>(ref TByteBlock byteBlock)
    {
        //字典类型，可以先判断是否为null
        WriterExtension.WriteIsNull(ref byteBlock,this.P6);
        if (this.P6 != null)
        {
            //如果不为null
            //就先写入字典长度
            //然后遍历将每个项，按键、值写入
            WriterExtension.WriteValue(ref byteBlock,(int)this.P6.Count);
            foreach (var item in this.P6)
            {
                WriterExtension.WriteValue(ref byteBlock,(int)item.Key);
                //因为值MyClassModel实现了IPackage，所以可以直接写入
                WriterExtension.WritePackage(ref byteBlock,item.Value);
            }
        }
    }

    public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)
    {
        var isNull_6 = ReaderExtension.ReadIsNull<TByteBlock>(ref byteBlock);
        if (!isNull_6)
        {
            var count = ReaderExtension.ReadValue<TByteBlock,int>(ref byteBlock);
            var dic = new Dictionary<int, MyClassModel>(count);
            for (var i = 0; i < count; i++)
            {
                dic.Add(ReaderExtension.ReadValue<TByteBlock,int>(ref byteBlock), ReaderExtension.ReadPackage<TByteBlock, MyClassModel>(ref byteBlock));
            }
            this.P6 = dic;
        }
    }
}