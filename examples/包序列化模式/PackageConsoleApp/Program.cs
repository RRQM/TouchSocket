using TouchSocket.Core;

namespace PackageConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var myClass = new MyClass();
            myClass.P1 = 1;
            myClass.P2 = "若汝棋茗";
            myClass.P3 = 'a';
            myClass.P4= 3;

            myClass.P5=new List<int> { 1, 2, 3 };

            myClass.P6= new Dictionary<int, MyClassModel>() 
            {
                { 1,new MyClassModel(){ P1=DateTime.Now} },
                { 2,new MyClassModel(){ P1=DateTime.Now} }
            };

            using (ByteBlock byteBlock=new ByteBlock())
            {
                myClass.Package(byteBlock);//打包，相当于序列化

                byteBlock.Seek(0);//将流位置重置为0

                var myNewClass = new MyClass();
                myNewClass.Unpackage(byteBlock);//解包，相当于反序列化
            }
        }
    }

    class MyClass : IPackage
    {
        public int P1 { get; set; }
        public string P2 { get; set; }
        public char P3 { get; set; }
        public double P4 { get; set; }
        public List<int> P5 { get; set; }
        public Dictionary<int, MyClassModel> P6 { get; set; }
        public void Package(ByteBlock byteBlock)
        {
            //基础类型直接写入。
            byteBlock.Write(P1);
            byteBlock.Write(P2);
            byteBlock.Write(P3);
            byteBlock.Write(P4);

            //集合类型，可以先判断是否为null
            byteBlock.WriteIsNull(P5);
            if (P5 != null)
            {
                //如果不为null
                //就先写入集合长度
                //然后遍历将每个项写入
                byteBlock.Write(P5.Count);
                foreach (var item in P5)
                {
                    byteBlock.Write(item);
                }
            }

            //字典类型，可以先判断是否为null
            byteBlock.WriteIsNull(P6);
            if (P5 != null)
            {
                //如果不为null
                //就先写入字典长度
                //然后遍历将每个项，按键、值写入
                byteBlock.Write(P6.Count);
                foreach (var item in P6)
                {
                    byteBlock.Write(item.Key);
                    byteBlock.WritePackage(item.Value);//因为值MyClassModel实现了IPackage，所以可以直接写入
                }
            }
        }

        public void Unpackage(ByteBlock byteBlock)
        {
            //基础类型按序读取。
            this.P1 = byteBlock.ReadInt32();
            this.P2 = byteBlock.ReadString();
            this.P3 = byteBlock.ReadChar();
            this.P4 = byteBlock.ReadDouble();

            var isNull = byteBlock.ReadIsNull();
            if (!isNull)
            {
                int count = byteBlock.ReadInt32();
                var list = new List<int>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add(byteBlock.ReadInt32());
                }
                this.P5 = list;
            }


            isNull = byteBlock.ReadIsNull();//复用前面的变量，省的重新声明
            if (!isNull)
            {
                int count = byteBlock.ReadInt32();
                var dic = new Dictionary<int, MyClassModel>(count);
                for (int i = 0; i < count; i++)
                {
                    dic.Add(byteBlock.ReadInt32(), byteBlock.ReadPackage<MyClassModel>());
                }
                this.P6 = dic;
            }
        }
    }

    class MyClassModel : PackageBase
    {
        public DateTime P1 { get; set; }
        public override void Package(ByteBlock byteBlock)
        {
            byteBlock.Write(P1);
        }

        public override void Unpackage(ByteBlock byteBlock)
        {
            this.P1 = byteBlock.ReadDateTime();
        }
    }
}