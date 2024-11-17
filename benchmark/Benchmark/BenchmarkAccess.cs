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

#if NET8_0_OR_GREATER
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkAccess : BenchmarkBase
    {
        public static readonly A TestInstance = new();
        public static readonly Action<A, int> SetDelegate;
        public static readonly Func<A, int> GetDelegate;
        public static readonly PropertyInfo ValueProperty; public static readonly MethodInfo SetValueMethod; public static readonly MethodInfo GetValueMethod;

        public static readonly Func<A, int> GetValueExpressionFunc; public static readonly Action<A, int> SetValueExpressionAction; static BenchmarkAccess()
        {
            TestInstance = new(); ValueProperty = typeof(A).GetProperty("Value");
            SetValueMethod = ValueProperty.GetSetMethod();
            GetValueMethod = ValueProperty.GetGetMethod();
            SetDelegate = CreateSetDelegate();
            GetDelegate = CreateGetDelegate();
            GetValueExpressionFunc = CreateGetValueExpressionFunc();
            SetValueExpressionAction = CreateSetValueExpressionAction();
        }

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Value")]
        private static extern int GetValueUnsafe(A a);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Value")]
        private static extern void SetValueUnsafe(A a, int value);

        [Benchmark]
        public void UnsafeAccessor()
        {
            for (var i = 0; i < this.Count; i++)
            {
                SetValueUnsafe(TestInstance, 10);
                var value = GetValueUnsafe(TestInstance);
            }
        }

        [Benchmark]
        public void Reflection()
        {
            for (var i = 0; i < this.Count; i++)
            {
                SetValueMethod.Invoke(TestInstance, new object[] { 10 });
                var value = GetValueMethod.Invoke(TestInstance, new object[] { });
            }
        }

        [Benchmark]
        public void Emit()
        {
            for (var i = 0; i < this.Count; i++)
            {
                SetDelegate(TestInstance, 10);
                var value = GetDelegate(TestInstance);
            }
        }

        [Benchmark]
        public void ExpressionTrees()
        {
            for (var i = 0; i < this.Count; i++)
            {
                SetValueExpressionAction(TestInstance, 10);
                var value = GetValueExpressionFunc(TestInstance);
            }
        }

        [Benchmark]
        public void Direct()
        {
            for (var i = 0; i < this.Count; i++)
            {
                TestInstance.Value = 10;
                var value = TestInstance.Value;
            }
        }

        private static Action<A, int> CreateSetDelegate()
        {
            var dynamicMethod = new DynamicMethod("SetValue", null, new[] { typeof(A), typeof(int) }, typeof(A)); var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.EmitCall(OpCodes.Call, SetValueMethod, null); ilGenerator.Emit(OpCodes.Ret);
            return (Action<A, int>)dynamicMethod.CreateDelegate(typeof(Action<A, int>));
        }

        private static Func<A, int> CreateGetDelegate()
        {
            var dynamicMethod = new DynamicMethod("GetValue", typeof(int), new[] { typeof(A) }, typeof(A));
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.EmitCall(OpCodes.Call, GetValueMethod, null); ilGenerator.Emit(OpCodes.Ret);
            return (Func<A, int>)dynamicMethod.CreateDelegate(typeof(Func<A, int>));
        }

        private static Func<A, int> CreateGetValueExpressionFunc()
        {
            var instance = Expression.Parameter(typeof(A), "instance");
            var getValueExpression = Expression.Lambda<Func<A, int>>(Expression.Property(instance, ValueProperty), instance); return getValueExpression.Compile();
        }

        private static Action<A, int> CreateSetValueExpressionAction()
        {
            var instance = Expression.Parameter(typeof(A), "instance");
            var value = Expression.Parameter(typeof(int), "value");
            var setValueExpression = Expression.Lambda<Action<A, int>>(Expression.Call(instance, ValueProperty.GetSetMethod(true), value), instance, value);
            return setValueExpression.Compile();
        }
    }

    public class A
    {
        public int Value { get; set; }
    }

}

#endif