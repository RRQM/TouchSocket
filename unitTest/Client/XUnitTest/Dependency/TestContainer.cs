//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using Xunit;

namespace XUnitTest.Dependency
{
    public class TestContainer
    {
        [Fact]
        public void SingletonShouldBeOk()
        {
            Container container = new Container();
            container.RegisterSingleton<ILog, MyLog1>();

            var log1 = container.Resolve<ILog>();
            var log2 = container.Resolve<ILog>();
            Assert.NotNull(log1);
            Assert.True(log1 == log2);
        }

        [Fact]
        public void ObjectSingletonShouldBeOk()
        {
            Container container = new Container();
            container.RegisterSingleton<ILog, MyLog1>();
            container.RegisterSingleton<ILog, MyLog10>("10");

            var log10 = container.Resolve<ILog>("10") as MyLog10;
            Assert.NotNull(log10);
            Assert.NotNull(log10.MyLog1);
            Assert.True(log10.MyLog1.GetType() == typeof(MyLog1));
        }

        [Fact]
        public void InstanceSingletonShouldBeOk()
        {
            Container container = new Container();
            container.RegisterSingleton<ILog, MyLog1>();

            var log3 = new MyLog1();
            container.RegisterSingleton<ILog, MyLog1>(log3);
            var log1 = container.Resolve<ILog>();
            var log2 = container.Resolve<ILog>();
            Assert.NotNull(log1);
            Assert.NotNull(log3);
            Assert.True(log1 == log2 && log1 == log3);
        }

        [Fact]
        public void KeySingletonShouldBeOk()
        {
            Container container = new Container();
            container.RegisterSingleton<ILog, MyLog1>("1");
            container.RegisterSingleton<ILog, MyLog1>("2");

            var log1 = container.Resolve<ILog>("1");
            var log2 = container.Resolve<ILog>("1");
            Assert.NotNull(log1);
            Assert.True(log1 == log2);

            var log3 = container.Resolve<ILog>("2");
            var log4 = container.Resolve<ILog>("2");
            Assert.NotNull(log3);
            Assert.True(log3 == log4);

            Assert.False(log3 == log1);
            Assert.False(log4 == log2);
        }

        [Fact]
        public void TransientShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog1>();

            var log1 = container.Resolve<ILog>();
            var log2 = container.Resolve<ILog>();
            Assert.NotNull(log1);
            Assert.False(log1 == log2);
        }

        [Fact]
        public void KeyTransientShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog1>("1");
            container.RegisterTransient<ILog, MyLog2>("2");

            var log1 = container.Resolve<ILog>("1");
            var log2 = container.Resolve<ILog>("1");
            Assert.NotNull(log1);
            Assert.False(log1 == log2);
            Assert.True(log1.GetType() == typeof(MyLog1));

            var log3 = container.Resolve<ILog>("2");
            var log4 = container.Resolve<ILog>("2");
            Assert.NotNull(log3);
            Assert.False(log3 == log4);
            Assert.True(log3.GetType() == typeof(MyLog2));
        }

        [Fact]
        public void ScopedShouldBeOk()
        {
            Container container = new Container();
            container.RegisterScoped<ILog, MyLog1>();

            var log1 = container.Resolve<ILog>();
            var log2 = container.Resolve<ILog>();
            Assert.NotNull(log1);
            Assert.False(log1 == log2);

            IContainerProvider scopedContainer = container.Resolve<IContainerProvider>();
            log1 = scopedContainer.Resolve<ILog>();
            log2 = scopedContainer.Resolve<ILog>();
            Assert.NotNull(log1);
            Assert.True(log1 == log2);
        }

        [Fact]
        public void KeyScopedShouldBeOk()
        {
            Container container = new Container();
            container.RegisterScoped<ILog, MyLog1>("1");
            container.RegisterScoped<ILog, MyLog2>("2");

            var log1 = container.Resolve<ILog>("1");
            var log2 = container.Resolve<ILog>("1");
            Assert.NotNull(log1);
            Assert.False(log1 == log2);
            Assert.True(log1.GetType() == typeof(MyLog1));

            var log3 = container.Resolve<ILog>("2");
            var log4 = container.Resolve<ILog>("2");
            Assert.NotNull(log3);
            Assert.False(log3 == log4);
            Assert.True(log3.GetType() == typeof(MyLog2));

            IContainerProvider scopedContainer = container.Resolve<IContainerProvider>();
            log1 = scopedContainer.Resolve<ILog>("1");
            log2 = scopedContainer.Resolve<ILog>("1");
            Assert.NotNull(log1);
            Assert.True(log1 == log2);

            log3 = scopedContainer.Resolve<ILog>("2");
            log4 = scopedContainer.Resolve<ILog>("2");
            Assert.NotNull(log1);
            Assert.True(log3 == log4);
        }

        [Fact]
        public void CtorShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog3>();

            var log3 = container.Resolve<ILog>() as MyLog3;

            Assert.NotNull(log3.MyLog1);
            Assert.NotNull(log3.MyLog2);
        }

        [Fact]
        public void CtorParametersShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog4>();

            var log4 = container.Resolve<ILog>() as MyLog4;

            Assert.NotNull(log4.MyLog1);
            Assert.NotNull(log4.MyLog2);
            Assert.Equal(10, log4.A);
            Assert.Equal("RRQM", log4.B);

            log4 = container.Resolve<ILog>(new object[] { 20, "IOU" }) as MyLog4;

            Assert.NotNull(log4.MyLog1);
            Assert.NotNull(log4.MyLog2);
            Assert.Equal(20, log4.A);
            Assert.Equal("IOU", log4.B);
        }

        [Fact]
        public void PropertyShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog1>("MyLog1");
            container.RegisterTransient<ILog, MyLog2>("MyLog2");
            container.RegisterTransient<ILog, MyLog3>("MyLog3");
            container.RegisterTransient<ILog, MyLog5>();

            var log5 = container.Resolve<ILog>() as MyLog5;

            Assert.NotNull(log5.MyLog1);
            Assert.NotNull(log5.MyLog2);
            Assert.True(log5.MyLog1.GetType() == typeof(MyLog1));
            Assert.True(log5.MyLog2.GetType() == typeof(MyLog2));
        }

        [Fact]
        public void MethodShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog1>("MyLog1");
            container.RegisterTransient<ILog, MyLog2>("MyLog2");
            container.RegisterTransient<ILog, MyLog3>("MyLog3");
            container.RegisterTransient<ILog, MyLog4>("MyLog4");
            container.RegisterTransient<ILog, MyLog6>("MyLog5");
            container.RegisterTransient<ILog, MyLog6>();

            var log6 = container.Resolve<ILog>() as MyLog6;

            Assert.NotNull(log6.MyLog1);
            Assert.NotNull(log6.MyLog4);
            Assert.True(log6.MyLog1.GetType() == typeof(MyLog1));
            Assert.True(log6.MyLog4.GetType() == typeof(MyLog4));
            Assert.True(((MyLog4)log6.MyLog4).A == 20);
            Assert.True(((MyLog4)log6.MyLog4).B == "IOU");
        }

        [Fact]
        public void GenericSingletonShouldBeOk()
        {
            Container container = new Container();
            container.RegisterSingleton<ILog, MyLog7>();
            container.RegisterSingleton(typeof(IGeneric<,>), typeof(Generic<,>));

            var log71 = container.Resolve<ILog>();
            var log72 = container.Resolve<ILog>();

            Assert.NotNull(log71);
            Assert.NotNull(((MyLog7)log71).Generic);

            Assert.NotNull(log72);
            Assert.NotNull(((MyLog7)log72).Generic);

            Assert.True(((MyLog7)log71).Generic == ((MyLog7)log72).Generic);
        }

        [Fact]
        public void DependencyType1ShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog1>("MyLog1");
            container.RegisterTransient<ILog, MyLog2>("MyLog2");
            container.RegisterTransient<ILog, MyLog3>("MyLog3");
            container.RegisterTransient<ILog, MyLog4>("MyLog4");
            container.RegisterTransient<ILog, MyLog6>("MyLog5");
            container.RegisterTransient<ILog, MyLog8>();

            var log8 = container.Resolve<ILog>() as MyLog8;

            Assert.NotNull(log8);
            Assert.True(log8.A == 0);
            Assert.True(log8.B == null);
        }

        [Fact]
        public void DependencyType2ShouldBeOk()
        {
            Container container = new Container();
            container.RegisterTransient<ILog, MyLog1>("MyLog1");
            container.RegisterTransient<ILog, MyLog2>("MyLog2");
            container.RegisterTransient<ILog, MyLog3>("MyLog3");
            container.RegisterTransient<ILog, MyLog4>("MyLog4");
            container.RegisterTransient<ILog, MyLog6>("MyLog5");
            container.RegisterTransient<ILog, MyLog9>();

            var log9 = container.Resolve<ILog>() as MyLog9;

            Assert.NotNull(log9);
            Assert.True(log9.A == 10);
            Assert.True(log9.B == "RRQM");
        }
    }

    public interface IGeneric<T1, T2>
    {
    }

    public class Generic<T1, T2> : IGeneric<T1, T2>
    {
    }

    public class MyLog1 : ILog
    {
        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    public class MyLog2 : ILog
    {
        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    public class MyLog3 : ILog
    {
        public MyLog3(MyLog1 myLog1, MyLog2 myLog2)
        {
            this.MyLog1 = myLog1;
            this.MyLog2 = myLog2;
        }

        public MyLog1 MyLog1 { get; }
        public MyLog2 MyLog2 { get; }

        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    public class MyLog4 : ILog
    {
        [DependencyInject(10, "RRQM")]
        public MyLog4(int a, string b, MyLog1 myLog1, MyLog2 myLog2)
        {
            this.A = a;
            this.B = b;
            this.MyLog1 = myLog1;
            this.MyLog2 = myLog2;
        }

        public int A { get; }
        public string B { get; }
        public MyLog1 MyLog1 { get; }
        public MyLog2 MyLog2 { get; }
        public LogType LogType { get ; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    public class MyLog5 : ILog
    {
        [DependencyParamterInject("MyLog1")]
        public ILog MyLog1 { get; set; }

        [DependencyParamterInject("MyLog2")]
        public ILog MyLog2 { get; set; }

        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    public class MyLog6 : ILog
    {
        [DependencyInject(10, "RRQM")]
        public void DependencyMethod(int a, string b, [DependencyParamterInject("MyLog1")] ILog myLog1, [DependencyParamterInject("MyLog4", 20, "IOU")] ILog myLog4)
        {
            this.A = a;
            this.B = b;
            this.MyLog1 = myLog1;
            this.MyLog4 = myLog4;
        }

        public int A { get; set; }
        public string B { get; set; }
        public ILog MyLog1 { get; set; }
        public ILog MyLog4 { get; set; }

        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    public class MyLog7 : ILog
    {
        public MyLog7(IGeneric<ILog, MyLog2> generic)
        {
            this.Generic = generic;
        }

        public IGeneric<ILog, MyLog2> Generic { get; }
        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    [DependencyType(DependencyType.Constructor)]
    public class MyLog8 : ILog
    {
        [DependencyInject(10, "RRQM")]
        public void DependencyMethod(int a, string b, [DependencyParamterInject("MyLog1")] ILog myLog1, [DependencyParamterInject("MyLog4", 20, "IOU")] ILog myLog4)
        {
            this.A = a;
            this.B = b;
            this.MyLog1 = myLog1;
            this.MyLog4 = myLog4;
        }

        public int A { get; set; }
        public string B { get; set; }
        public ILog MyLog1 { get; set; }
        public ILog MyLog4 { get; set; }
        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    [DependencyType(DependencyType.Constructor | DependencyType.Method)]
    public class MyLog9 : ILog
    {
        [DependencyInject(10, "RRQM")]
        public void DependencyMethod(int a, string b, [DependencyParamterInject("MyLog1")] ILog myLog1, [DependencyParamterInject("MyLog4", 20, "IOU")] ILog myLog4)
        {
            this.A = a;
            this.B = b;
            this.MyLog1 = myLog1;
            this.MyLog4 = myLog4;
        }

        public int A { get; set; }
        public string B { get; set; }
        public ILog MyLog1 { get; set; }
        public ILog MyLog4 { get; set; }
        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }

    public class MyLog10 : ILog
    {
        [DependencyParamterInject(typeof(ILog))]
        public object MyLog1 { get; set; }
        public LogType LogType { get; set; }

        public void Log(LogType logType, object source, string message, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}