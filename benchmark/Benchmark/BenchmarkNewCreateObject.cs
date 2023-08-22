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
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class BenchmarkNewCreateObject : BenchmarkBase
    {
        [Benchmark]
        public void NewCreate()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var myClass = new MyClass();
            }
        }

        [Benchmark]
        public void ExpressionsCreate()
        {
            var type = typeof(MyClass);
            for (var i = 0; i < this.Count; i++)
            {
                var myClass = (MyClass)CreateInstance.CreateInstanceByExpression(type);
            }
        }

        [Benchmark]
        public void TCreate()
        {
            for (var i = 0; i < this.Count; i++)
            {
                var myClass = CreateInstance.Create<MyClass>();
            }
        }

        [Benchmark]
        public void ActivatorCreate()
        {
            var type = typeof(MyClass);
            for (var i = 0; i < this.Count; i++)
            {
                var myClass = (MyClass)Activator.CreateInstance(type);
            }
        }

        [Benchmark]
        public void ActivatorCreateStringList()
        {
            var type = typeof(string);
            for (var i = 0; i < this.Count; i++)
            {
                this.GoType(type);
            }
        }



        [Benchmark]
        public void NewCreateStringList()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.GoT<string>();
            }
        }

        private object GoType(Type type)
        {
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList;
            return list;
        }

        private List<T> GoT<T>()
        {
            var t = new List<T>();
            return t;
        }

        private class MyClass
        {
        }
    }

    public static class CreateInstance
    {
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());//缓存

        /// <summary>
        /// 根据对象类型创建对象实例
        /// </summary>
        /// <param name="key">对象类型</param>
        /// <returns></returns>
        public static object CreateInstanceByExpression(Type key)
        {
            var value = (Func<object>)paramCache[key];
            if (value == null)
            {
                value = CreateInstanceByType(key);
                paramCache[key] = value;
            }
            return value();
        }

        private static Func<object> CreateInstanceByType(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type), null).Compile();
        }

        public static T Create<T>() where T : new()
        {
            return new T();
        }
    }
}