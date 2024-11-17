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
using System.Collections.Generic;
using System.Dynamic;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkDependencyObject : BenchmarkBase
    {
        [Benchmark]
        public void RunDependencyIntTryGetValue()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                var a = myClassObject.TryGetValue(MyClassObject.Obj2PropertyProperty, out var ob);
            }
        }

        [Benchmark]
        public void RunDependencyIntGet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                var a = myClassObject.Int2Property;
            }
        }

        [Benchmark]
        public void RunDependencyIntSet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                myClassObject.Int2Property = i;
            }
        }

        [Benchmark]
        public void RunDependencyObjGet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                var a = myClassObject.Obj2Property;
            }
        }

        [Benchmark]
        public void RunDependencyObjSet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                myClassObject.Obj2Property = new object();
            }
        }

        [Benchmark]
        public void RunIntGet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                var a = myClassObject.IntProperty;
            }
        }

        [Benchmark]
        public void RunIntSet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                myClassObject.IntProperty = i;
            }
        }

        [Benchmark]
        public void RunObjGet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                var a = myClassObject.ObjProperty;
            }
        }

        [Benchmark]
        public void RunObjSet()
        {
            var myClassObject = new MyClassObject();
            for (var i = 0; i < this.Count; i++)
            {
                myClassObject.ObjProperty = new object();
            }
        }

        [Benchmark]
        public void RunCreateDependencyObject()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var myClassObject = new MyClassObject();
            }
        }

        [Benchmark]
        public void RunNormalObj()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var disposableObject = new DisposableObject();
            }
        }

        [Benchmark]
        public void RunObj()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var disposableObject = new Obj();
            }
        }

        [Benchmark]
        public void RunExpandoObject()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var disposableObject = new ExpandoObject();
            }
        }
    }

    internal class Obj : DisposableObject
    {
        private Dictionary<string, string> m_keyValuePairs = new Dictionary<string, string>();
        public int Int2Property { get; set; }

        public int IntProperty { get; set; }

        public object Obj2Property { get; set; }

        public object ObjProperty { get; set; }
    }

    internal class MyClassObject : DependencyObject
    {
        public static readonly DependencyProperty<int> Int2PropertyProperty =
            DependencyProperty<int>.Register("Int2Property", 0);

        public static readonly DependencyProperty<object> Obj2PropertyProperty =
            DependencyProperty<object>.Register("ObjProperty", null);

        public int Int2Property
        {
            get { return (int)this.GetValue(Int2PropertyProperty); }
            set { this.SetValue(Int2PropertyProperty, value); }
        }

        public int IntProperty { get; set; }

        public object Obj2Property
        {
            get { return (object)this.GetValue(Obj2PropertyProperty); }
            set { this.SetValue(Obj2PropertyProperty, value); }
        }

        public object ObjProperty { get; set; }
    }
}