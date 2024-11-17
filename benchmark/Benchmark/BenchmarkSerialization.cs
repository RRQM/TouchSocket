//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

#if NET6_0_OR_GREATER
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkSerialization : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public void DirectNew()
        {
            var v = new MyPackPerson { Age = 40, Name = "John" };
            for (var i = 0; i < this.Count; i++)
            {
                var bytes = new byte[8];//开辟小内存模拟序列化
                var val = new MyPackPerson { Age = 40, Name = "John" };//直接新建，模拟反序列化
            }
        }

        [Benchmark]
        public void MemoryPack()
        {
            var v = new MemoryPackPerson { Age = 40, Name = "John" };
            for (var i = 0; i < this.Count; i++)
            {
                var bin = MemoryPackSerializer.Serialize(v);
                var val = MemoryPackSerializer.Deserialize<MemoryPackPerson>(bin);
            }
        }

        [Benchmark]
        public void TouchPack_1()
        {
            var v = new MyPackPerson { Age = 40, Name = "John" };

            using (var byteBlock = new ByteBlock())
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();//避免ByteBlock的创建
                    byteBlock.WritePackage(v);//序列化
                    byteBlock.Seek(0);
                    var val = byteBlock.ReadPackage<MyPackPerson>();//反序列化
                }
            }
        }
        [Benchmark]
        public void TouchPack_2()
        {
            var v = new MyPackPerson { Age = 40, Name = "John" };
            using (var byteBlock = new ByteBlock())
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();//避免ByteBlock的创建
                    byteBlock.WritePackage(v);//序列化
                    byteBlock.Seek(0);
                    var myPackPerson = new MyPackPerson();
                    myPackPerson.Unpackage(byteBlock);//反序列化
                }
            }
        }

        [Benchmark]
        public void NewtonsoftJson()
        {
            var v = new MyPackPerson { Age = 40, Name = "John" };
            for (var i = 0; i < this.Count; i++)
            {
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(v);
                var val = Newtonsoft.Json.JsonConvert.DeserializeObject<MyPackPerson>(str);
            }
        }

        [Benchmark]
        public void SystemTextJson()
        {
            var v = new MyPackPerson { Age = 40, Name = "John" };
            for (var i = 0; i < this.Count; i++)
            {
                var str = System.Text.Json.JsonSerializer.Serialize(v);
                var val = System.Text.Json.JsonSerializer.Deserialize<MyPackPerson>(str);
            }
        }

        [Benchmark]
        public void FastBinarySerialize()
        {
            var v = new MyPackPerson { Age = 40, Name = "John" };
            using (var byteBlock = new ByteBlock())
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();
                    SerializeConvert.FastBinarySerialize(byteBlock, v);
                    byteBlock.Seek(0);
                    var val = SerializeConvert.FastBinaryDeserialize<MyPackPerson>(byteBlock.Buffer);
                }
            }
        }

        [Benchmark]
        public void SystemBinarySerialize()
        {
            var v = new MyPackPerson { Age = 40, Name = "John" };
            using (var byteBlock = new ByteBlock())
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();
                    SerializeConvert.BinarySerialize(byteBlock, v);
                    byteBlock.Seek(0);
                    var val = SerializeConvert.BinaryDeserialize<MyPackPerson>(byteBlock.Buffer);
                }
            }
        }
    }

    [MemoryPackable]
    public partial class MemoryPackPerson
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }


    [Serializable]
    public class MyPackPerson : PackageBase
    {
        public int Age { get; set; }
        public string Name { get; set; }

        public override void Package(in ByteBlock byteBlock)
        {
            byteBlock.Write(this.Age);
            byteBlock.Write(this.Name);
        }

        public override void Unpackage(in ByteBlock byteBlock)
        {
            this.Age = byteBlock.ReadInt32();
            this.Name = byteBlock.ReadString();
        }
    }
}
#endif