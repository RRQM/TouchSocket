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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkInvokeMethod : BenchmarkBase
    {
        public BenchmarkInvokeMethod()
        {
            this.m_method = new Method(typeof(MyClass2).GetMethod("Add"));

            var AddStatic = typeof(MyClass2).GetProperty("AddStaticAction");

            this.m_myClass3 = new MyClass3();

            this.m_myClass3.Func = (Func<object, object[], object>)AddStatic.GetValue(null);
        }

        private MyClass3 m_myClass3;
        private Method m_method;

        [Benchmark]
        public void DirectRun()
        {
            var myClass1 = new MyClass1();

            var a = 10;
            var b = 20;
            var c = 0;

            var objects = new object[] { a, b, c };

            for (var i = 0; i < this.Count; i++)
            {
                myClass1.Add(a, b, out c);
            }
        }

        [Benchmark]
        public void MethodILRun()
        {
            var myClass2 = new MyClass2();

            var a = 10;
            var b = 20;
            var c = 0;

            var objects = new object[] { a, b, c };
            for (var i = 0; i < this.Count; i++)
            {
                this.m_method.Invoke(myClass2, objects);
            }
        }

        [Benchmark]
        public void MethodInfoRun()
        {
            var myClass2 = new MyClass2();

            var a = 10;
            var b = 20;
            var c = 0;

            var objects = new object[] { a, b, c };
            for (var i = 0; i < this.Count; i++)
            {
                this.m_method.Info.Invoke(myClass2, objects);
            }
        }

        [Benchmark]
        public void StaticMethodRun1()
        {
            var myClass1 = new MyClass1();

            var a = 10;
            var b = 20;
            var c = 0;

            var objects = new object[] { a, b, c };

            for (var i = 0; i < this.Count; i++)
            {
                MyClass1.AddStatic(myClass1, a, b, out c);
            }
        }

        [Benchmark]
        public void StaticMethodRun2()
        {
            var myClass2 = new MyClass2();

            var a = 10;
            var b = 20;
            var c = 0;

            var objects = new object[] { a, b, c };

            Func<object, object[], object> action = MyClass2.AddStatic;

            for (var i = 0; i < this.Count; i++)
            {
                action.Invoke(myClass2, objects);
            }
        }

        [Benchmark]
        public void StaticMethodRun3()
        {
            var myClass2 = new MyClass2();

            var a = 10;
            var b = 20;
            var c = 0;

            var objects = new object[] { a, b, c };

            for (var i = 0; i < this.Count; i++)
            {
                this.m_myClass3.Func.Invoke(myClass2, objects);
            }
        }

        public partial class MyClass1
        {
            //这个是被调用函数。
            public int Add(int a, int b, out int sum)
            {
                sum = a + b;
                return a + b;
            }

            public static int AddStatic(MyClass1 myClass, int a, int b, out int sum)
            {
                var result = myClass.Add(a, b, out sum);
                return result;
            }
        }

        public partial class MyClass2
        {
            public static Func<object, object[], object> AddStaticAction => AddStatic;

            public static object AddStatic(object myClass, object[] ps)
            {
                var a = (int)ps[0];
                var b = (int)ps[1];
                var result = ((MyClass2)myClass).Add(a, b, out var c);
                ps[2] = c;
                return result;
            }

            //这个是被调用函数。
            public int Add(int a, int b, out int sum)
            {
                sum = a + b;
                return a + b;
            }
        }

        private class MyClass3
        {
            public Func<object, object[], object> Func;
        }
    }
}