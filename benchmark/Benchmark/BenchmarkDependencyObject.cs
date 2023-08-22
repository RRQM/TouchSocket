using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Collections.Generic;
using System.Dynamic;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [MemoryDiagnoser]
    public class BenchmarkDependencyObject : BenchmarkBase
    {
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

    class Obj : DisposableObject
    {
        Dictionary<string, string> m_keyValuePairs = new Dictionary<string, string>();
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