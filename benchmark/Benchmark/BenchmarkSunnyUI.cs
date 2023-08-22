using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SunnyUI.FrameDecoder;
using System;
using System.Collections.Generic;
using TouchSocket.Core;
using StreamReader = SunnyUI.FrameDecoder.StreamReader;
using StreamWriter = SunnyUI.FrameDecoder.StreamWriter;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class BenchmarkSunnyUI
    {
        private const int Count = 1000;

        [Benchmark]
        public void Bytes()
        {
            for (var i = 0; i < Count; i++)
            {
                var bytes1 = new byte[4];
                var bytes2 = new byte[4];
                var bytes3 = new byte[1];
                var bytes4 = new byte[8];
                var bytes5 = new byte[1];
                var bytes6 = new byte[4];
                var bytes7 = new byte[4];
                var bytes8 = new byte[4];
            }
        }


        [Benchmark]
        public void TouchPack()
        {
            var myClass = MyClass.Instance();
            var byteBlock = new ByteBlock();
            for (var i = 0; i < Count; i++)
            {
                byteBlock.Reset();//避免ByteBlock的创建
                myClass.Package(byteBlock);//序列化
                //byteBlock.WritePackage(myClass);
                byteBlock.Seek(0);
                var myClass1 = new MyClass();
                myClass1.Unpackage(byteBlock);//反序列化   
                //var val = byteBlock.ReadPackage<MyClass>();                 
            }

            byteBlock.Dispose();
        }

        [Benchmark]
        public void SunnyUIByteWriter()
        {
            var myClass = SunnyByteClass.Instance();

            for (var i = 0; i < Count; i++)
            {
                using (var writer = new ByteWriter())
                {
                    writer.Serialize(myClass);//序列化
                    var reader = new ByteReader(writer.AsSpan(), writer.EncodingType);
                    var myPackPerson = reader.Deserialize<SunnyByteClass>();//反序列化
                }
            }
        }

        [Benchmark]
        public void SunnyUIStreamWriter()
        {
            var myClass = SunnyByteClass.Instance();

            for (var i = 0; i < Count; i++)
            {
                using (var writer = new StreamWriter())
                {
                    writer.Serialize(myClass);//序列化
                    var reader = new StreamReader(writer);
                    var myPackPerson = reader.Deserialize<SunnyByteClass>();//反序列化
                }
            }
        }

        [Benchmark(Baseline = true)]
        public void SunnyUIMemoryBufferWriter()
        {
            var myClass = SunnyMemoryClass.Instance();
            var bufferWriter = new MemoryBufferWriter<byte>(16);

            for (var i = 0; i < Count; i++)
            {
                bufferWriter.Clear();
                var writer = new MemoryWriter<MemoryBufferWriter<byte>>(ref bufferWriter, EncodingType.Utf8);
                myClass.Serialize(ref writer);
                writer.Flush();

                var reader = new MemoryReader(bufferWriter.WrittenSpan, EncodingType.Utf8);
                var myPackPerson = new SunnyMemoryClass();
                myPackPerson.Deserialize(ref reader);
            }
        }

    }

    public class MyClass : IPackage
    {
        public static MyClass Instance()
        {
            var myClass = new MyClass();
            myClass.P1 = 1;
            myClass.P2 = "SunnyUI阳光彩虹小白马";
            myClass.P3 = '阳';
            myClass.P4 = 3;

            myClass.P5 = new List<int> { 1, 2, 3 };

            myClass.P6 = new Dictionary<int, MyClassModel>()
            {
                { 1,new MyClassModel(){ P1=DateTime.Now} },
                { 2,new MyClassModel(){ P1=DateTime.Now.AddMinutes(1)} }
            };

            return myClass;
        }

        public uint P1 { get; set; }
        public string P2 { get; set; }
        public char P3 { get; set; }
        public double P4 { get; set; }
        public List<int> P5 { get; set; }
        public Dictionary<int, MyClassModel> P6 { get; set; }
        public void Package(in ByteBlock byteBlock)
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

        public void Unpackage(in ByteBlock byteBlock)
        {
            //基础类型按序读取。
            this.P1 = byteBlock.ReadUInt32();
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

    public class MyClassModel : PackageBase
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

    public class SunnyByteClass : ISerializeObject, IStreamSerializeObject
    {
        public uint P1 { get; set; }
        public string P2 { get; set; }
        public char P3 { get; set; }
        public double P4 { get; set; }
        public List<int> P5 { get; set; }

        public Dictionary<int, SunnyByteModel> P6 { get; set; }

        public static SunnyByteClass Instance()
        {
            var myClass = new SunnyByteClass();
            myClass.P1 = 1;
            myClass.P2 = "SunnyUI阳光彩虹小白马";
            myClass.P3 = '阳';
            myClass.P4 = 3;

            myClass.P5 = new List<int> { 1, 2, 3 };

            myClass.P6 = new Dictionary<int, SunnyByteModel>()
            {
                { 1,new SunnyByteModel(){ P1=DateTime.Now} },
                { 2,new SunnyByteModel(){ P1=DateTime.Now.AddMinutes(1)} }
            };

            return myClass;
        }

        public void Deserialize(ByteReader reader)
        {
            //基础类型按序读取。
            this.P1 = reader.ReadUInt();
            this.P2 = reader.ReadString();
            this.P3 = reader.ReadChar();
            this.P4 = reader.ReadDouble();

            if (reader.TryReadCollectionHeader(out var listCount))
            {
                var list = new List<int>(listCount);
                for (var i = 0; i < listCount; i++)
                {
                    list.Add(reader.ReadInt());
                }

                this.P5 = list;
            }

            if (reader.TryReadCollectionHeader(out var dicCount))
            {
                var dic = new Dictionary<int, SunnyByteModel>(dicCount);
                for (var i = 0; i < dicCount; i++)
                {
                    dic.Add(reader.ReadInt(), reader.Deserialize<SunnyByteModel>());
                }

                this.P6 = dic;
            }
        }

        public void Deserialize(SunnyUI.FrameDecoder.StreamReader reader)
        {
            //基础类型按序读取。
            this.P1 = reader.ReadUInt();
            this.P2 = reader.ReadString();
            this.P3 = reader.ReadChar();
            this.P4 = reader.ReadDouble();

            if (reader.TryReadCollectionHeader(out var listCount))
            {
                var list = new List<int>(listCount);
                for (var i = 0; i < listCount; i++)
                {
                    list.Add(reader.ReadInt());
                }

                this.P5 = list;
            }

            if (reader.TryReadCollectionHeader(out var dicCount))
            {
                var dic = new Dictionary<int, SunnyByteModel>(dicCount);
                for (var i = 0; i < dicCount; i++)
                {
                    dic.Add(reader.ReadInt(), reader.Deserialize<SunnyByteModel>());
                }

                this.P6 = dic;
            }
        }

        public void Serialize(IByteWriter writer)
        {
            //基础类型直接写入。
            writer.WriteUInt(this.P1);
            writer.WriteString(this.P2);
            writer.WriteChar(this.P3);
            writer.WriteDouble(this.P4);

            //集合类型，可以先判断是否为null
            if (this.P5 == null)
            {
                writer.WriteNullCollectionHeader();
            }
            else
            {
                //如果不为null
                //就先写入集合长度
                //然后遍历将每个项写入
                writer.WriteCollectionHeader(this.P5.Count);
                foreach (var item in this.P5)
                {
                    writer.WriteInt(item);
                }
            }

            if (this.P6 == null)
            {
                writer.WriteNullCollectionHeader();
            }
            else
            {
                //如果不为null
                //就先写入字典长度
                //然后遍历将每个项，按键、值写入
                writer.WriteCollectionHeader(this.P6.Count);
                foreach (var item in this.P6)
                {
                    writer.WriteInt(item.Key);
                    writer.Serialize(item.Value);
                }
            }
        }
    }

    public class SunnyByteModel : ISerializeObject, IStreamSerializeObject
    {
        public DateTime P1 { get; set; }

        public void Deserialize(ByteReader reader)
        {
            this.P1 = reader.ReadDateTime();
        }

        public void Deserialize(SunnyUI.FrameDecoder.StreamReader reader)
        {
            this.P1 = reader.ReadDateTime();
        }

        public void Serialize(IByteWriter writer)
        {
            writer.WriteDateTime(this.P1);
        }
    }

    public partial class SunnyMemoryClass : MemoryObject
    {
        public uint P1 { get; set; }
        public string P2 { get; set; }
        public char P3 { get; set; }
        public double P4 { get; set; }
        public List<int> P5 { get; set; }

        public Dictionary<int, SunnyMemoryModel> P6 { get; set; }

        public static SunnyMemoryClass Instance()
        {
            var myClass = new SunnyMemoryClass();
            myClass.P1 = 1;
            myClass.P2 = "SunnyUI阳光彩虹小白马";
            myClass.P3 = '阳';
            myClass.P4 = 3;

            myClass.P5 = new List<int> { 1, 2, 3 };

            myClass.P6 = new Dictionary<int, SunnyMemoryModel>()
            {
                { 1, new SunnyMemoryModel(){ P1=DateTime.Now} },
                { 2, new SunnyMemoryModel(){ P1=DateTime.Now.AddMinutes(1)} }
            };

            return myClass;
        }

        public override void Deserialize(ref MemoryReader reader)
        {
            reader.TryReadObjectHeader(out _);

            //基础类型按序读取。
            this.P1 = reader.ReadUInt();
            this.P2 = reader.ReadString();
            this.P3 = reader.ReadChar();
            this.P4 = reader.ReadDouble();

            if (reader.TryReadCollectionHeader(out var length))
            {
                var list = new List<int>(length);
                for (var i = 0; i < length; i++)
                {
                    list.Add(reader.ReadInt());
                }

                this.P5 = list;
            }

            if (reader.TryReadCollectionHeader(out var dicCount))
            {
                var dic = new Dictionary<int, SunnyMemoryModel>(dicCount);
                for (var i = 0; i < dicCount; i++)
                {
                    var key = reader.ReadInt();
                    var value = new SunnyMemoryModel();
                    value.Deserialize(ref reader);
                    dic.Add(key, value);
                }

                this.P6 = dic;
            }
        }

        public override void Serialize<TBufferWriter>(ref MemoryWriter<TBufferWriter> writer)
        {
            writer.WriteObjectHeader(0);

            //基础类型直接写入。
            writer.WriteUInt(this.P1);
            writer.WriteString(this.P2);
            writer.WriteChar(this.P3);
            writer.WriteDouble(this.P4);

            //集合类型，可以先判断是否为null
            if (this.P5 == null)
            {
                writer.WriteNullCollectionHeader();
            }
            else
            {
                //如果不为null
                //就先写入集合长度
                //然后遍历将每个项写入
                writer.WriteCollectionHeader(this.P5.Count);
                foreach (var item in this.P5)
                {
                    writer.WriteInt(item);
                }
            }

            //字典类型，可以先判断是否为null
            if (this.P5 == null)
            {
                writer.WriteNullCollectionHeader();
            }
            else
            {
                //如果不为null
                //就先写入字典长度
                //然后遍历将每个项，按键、值写入
                writer.WriteCollectionHeader(this.P6.Count);
                foreach (var item in this.P6)
                {
                    writer.WriteInt(item.Key);
                    item.Value.Serialize(ref writer);
                }
            }
        }
    }

    public class SunnyMemoryModel : MemoryObject
    {
        public DateTime P1 { get; set; }

        public override void Deserialize(ref MemoryReader reader)
        {
            this.P1 = reader.ReadDateTime();
        }

        public override void Serialize<TBufferWriter>(ref MemoryWriter<TBufferWriter> writer)
        {
            writer.WriteDateTime(this.P1);
        }
    }

}


