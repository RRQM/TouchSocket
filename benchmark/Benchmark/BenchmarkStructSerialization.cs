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
//------------------------------------------------------------------------------
#if NETCOREAPP3_1_OR_GREATER
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MemoryPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class BenchmarkStructSerialization : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public void DirectNew()
        {
            var v = new MemoryPackTagAccountInfo
            {
                Account = "123",
                Email = "505554090@qq.com",
                Header = 10,
                Password = "abc",
                Phone = "123456789",
                ReadId = "id"
            };
            for (var i = 0; i < this.Count; i++)
            {
                var bytes = new byte[50];//开辟小内存模拟序列化
                var val = new MemoryPackTagAccountInfo
                {
                    Account = "123",
                    Email = "505554090@qq.com",
                    Header = 10,
                    Password = "abc",
                    Phone = "123456789",
                    ReadId = "id"
                };//直接新建，模拟反序列化
            }
        }

        [Benchmark]
        public void MemoryPack()
        {
            var v = new MemoryPackTagAccountInfo
            {
                Account = "123",
                Email = "505554090@qq.com",
                Header = 10,
                Password = "abc",
                Phone = "123456789",
                ReadId = "id"
            };
            for (var i = 0; i < this.Count; i++)
            {
                var bin = MemoryPackSerializer.Serialize(v);
                var val = MemoryPackSerializer.Deserialize<MemoryPackTagAccountInfo>(bin);
            }
        }

        [Benchmark]
        public void TouchPack()
        {
            var v = new TagAccountInfo
            {
                Account = "123",
                Email = "505554090@qq.com",
                Header = 10,
                Password = "abc",
                Phone = "123456789",
                ReadId = "id"
            };
            using (var byteBlock = new ByteBlock())
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();//避免ByteBlock的创建
                    v.Package(byteBlock);//序列化
                    byteBlock.Seek(0);
                    var val = new TagAccountInfo();
                    val.Unpackage(byteBlock);//反序列化
                }
            }
        }

        [Benchmark]
        public void FastBinary()
        {
            var v = new TagAccountInfo
            {
                Account = "123",
                Email = "505554090@qq.com",
                Header = 10,
                Password = "abc",
                Phone = "123456789",
                ReadId = "id"
            };
            using (var byteBlock = new ByteBlock())
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();//避免ByteBlock的创建
                    SerializeConvert.FastBinarySerialize(byteBlock, v);//序列化
                    byteBlock.Seek(0);
                    var val = SerializeConvert.FastBinaryDeserialize<TagAccountInfo>(byteBlock.Buffer);//反序列化
                }
            }
        }

        [Benchmark]
        public void NewtonsoftJson()
        {
            var v = new TagAccountInfo
            {
                Account = "123",
                Email = "505554090@qq.com",
                Header = 10,
                Password = "abc",
                Phone = "123456789",
                ReadId = "id"
            };
            for (var i = 0; i < this.Count; i++)
            {
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(v);
                var val = Newtonsoft.Json.JsonConvert.DeserializeObject<TagAccountInfo>(str);
            }
        }

        [Benchmark]
        public void SafeStructSerialize()
        {
            var v = new TagAccountInfo
            {
                Account = "123",
                Email = "505554090@qq.com",
                Header = 10,
                Password = "abc",
                Phone = "123456789",
                ReadId = "id"
            };
            for (var i = 0; i < this.Count; i++)
            {
                var bytes = StuctToBytes(v);
                var val = BytesToStuct<TagAccountInfo>(bytes);
            }
        }


        [Benchmark]
        public unsafe void UnsafeStructSerialize()
        {
            var v = new TagAccountInfo
            {
                Account = "123",
                Email = "505554090@qq.com",
                Header = 10,
                Password = "abc",
                Phone = "123456789",
                ReadId = "id"
            };
            for (var i = 0; i < this.Count; i++)
            {
                var array = ClassToBytes(v);
                fixed (byte* pArray = array)
                {
                    var val = BytesToClass<TagAccountInfo>(pArray, array.Length);
                }
            }
        }

        /// <summary>
        /// 结构体转byte数组
        /// </summary>
        /// <param name="structObj">要转换的结构体</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] StuctToBytes<T>(T structObj) where T : struct
        {
            //    if ("Byte[]" == structObj.GetType().Name)
            //        return (byte[])structObj;
            //    else if ("String" == structObj.GetType().Name)
            //        return Encoding.GetEncoding("gb2312").GetBytes((string)structObj);

            //得到结构体的大小
            var size = Marshal.SizeOf(structObj);
            //创建byte数组
            var bytes = new byte[size];
            //分配结构体大小的内存空间
            var structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }

        /// <summary>
        /// byte数组转结构体
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="type">结构体类型</param>
        /// <returns>转换后的结构体</returns>
        public static T BytesToStuct<T>(byte[] bytes) where T : struct
        {
            //得到结构体的大小
            var size = Marshal.SizeOf<T>();
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                //返回空
#pragma warning disable CS8603 // 可能返回 null 引用。
                return default;
#pragma warning restore CS8603 // 可能返回 null 引用。
            }
            //分配结构体大小的内存空间
            var structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure<T>(structPtr);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构体
#pragma warning disable CS8603 // 可能返回 null 引用。
            return (T)obj;
#pragma warning restore CS8603 // 可能返回 null 引用。
        }

        public static unsafe byte[] ClassToBytes(object obj)
        {
            var typeSize = Marshal.SizeOf(obj);
            var array = new byte[typeSize];
            fixed (byte* pArray = array)
                Marshal.StructureToPtr(obj, (IntPtr)pArray, false);
            return array;
        }

        public static unsafe T BytesToClass<T>(byte* pArray, int length)
        {
            var type = typeof(T);
            var typeSize = Marshal.SizeOf(type);
            if (typeSize > length)
                return default;

            return (T)Marshal.PtrToStructure((IntPtr)pArray, type);
        }
    }

    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct MemoryPackTagAccountInfo //0x04
    {
        public byte Header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string Account;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string Password;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string ReadId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string Phone;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Email;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TagAccountInfo : IPackage //0x04
    {
        public byte Header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string Account;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string Password;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string ReadId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string Phone;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Email;

        public void Package(in ByteBlock byteBlock)
        {
            byteBlock.Write(this.Header);
            byteBlock.Write(this.Account);
            byteBlock.Write(this.Password);
            byteBlock.Write(this.ReadId);
            byteBlock.Write(this.Phone);
            byteBlock.Write(this.Email);
        }

        public void Unpackage(in ByteBlock byteBlock)
        {
            this.Header = (byte)byteBlock.ReadByte();
            this.Account = byteBlock.ReadString();
            this.Password = byteBlock.ReadString();
            this.ReadId = byteBlock.ReadString();
            this.Phone = byteBlock.ReadString();
            this.Email = byteBlock.ReadString();
        }
    }

}
#endif