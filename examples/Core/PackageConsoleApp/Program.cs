using TouchSocket.Core;

namespace PackageConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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

                using (var byteBlock = new ByteBlock())
                {
                    myClass.Package(byteBlock);//打包，相当于序列化

                    byteBlock.Seek(0);//将流位置重置为0

                    var myNewClass = new MyPackage();
                    myNewClass.Unpackage(byteBlock);//解包，相当于反序列化
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

                using (var byteBlock = new ByteBlock())
                {
                    myClass.Package(byteBlock);//打包，相当于序列化

                    byteBlock.Seek(0);//将流位置重置为0

                    var myNewClass = new MyGeneratorPackage();
                    myNewClass.Unpackage(byteBlock);//解包，相当于反序列化
                }
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

        public override void Package(in ByteBlock byteBlock)
        {
            //基础类型直接写入。
            byteBlock.Write(this.P1);
            byteBlock.Write(this.P2);
            byteBlock.Write(this.P3);
            byteBlock.Write(this.P4);

            //集合类型，可以先判断是否为null
            byteBlock.WriteIsNull(this.P5);
            if (this.P5 != null)
            {
                //如果不为null
                //就先写入集合长度
                //然后遍历将每个项写入
                byteBlock.Write(this.P5.Count);
                foreach (var item in this.P5)
                {
                    byteBlock.Write(item);
                }
            }

            //字典类型，可以先判断是否为null
            byteBlock.WriteIsNull(this.P6);
            if (this.P6 != null)
            {
                //如果不为null
                //就先写入字典长度
                //然后遍历将每个项，按键、值写入
                byteBlock.Write(this.P6.Count);
                foreach (var item in this.P6)
                {
                    byteBlock.Write(item.Key);
                    byteBlock.WritePackage(item.Value);//因为值MyClassModel实现了IPackage，所以可以直接写入
                }
            }
        }

        public override void Unpackage(in ByteBlock byteBlock)
        {
            //基础类型按序读取。
            this.P1 = byteBlock.ReadInt32();
            this.P2 = byteBlock.ReadString();
            this.P3 = byteBlock.ReadChar();
            this.P4 = byteBlock.ReadDouble();

            var isNull = byteBlock.ReadIsNull();
            if (!isNull)
            {
                var count = byteBlock.ReadInt32();
                var list = new List<int>(count);
                for (var i = 0; i < count; i++)
                {
                    list.Add(byteBlock.ReadInt32());
                }
                this.P5 = list;
            }

            isNull = byteBlock.ReadIsNull();//复用前面的变量，省的重新声明
            if (!isNull)
            {
                var count = byteBlock.ReadInt32();
                var dic = new Dictionary<int, MyClassModel>(count);
                for (var i = 0; i < count; i++)
                {
                    dic.Add(byteBlock.ReadInt32(), byteBlock.ReadPackage<MyClassModel>());
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
    }

    internal class MyClassModel : PackageBase
    {
        public DateTime P1 { get; set; }

        public override void Package(in ByteBlock byteBlock)
        {
            byteBlock.Write(this.P1);
        }

        public override void Unpackage(in ByteBlock byteBlock)
        {
            this.P1 = byteBlock.ReadDateTime();
        }
    }
}