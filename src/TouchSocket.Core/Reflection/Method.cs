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
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// Task类型
    /// </summary>
    public enum TaskReturnType
    {
        /// <summary>
        /// 没有Task
        /// </summary>
        None,

        /// <summary>
        /// 仅返回Task
        /// </summary>
        Task,

        /// <summary>
        /// 返回Task的值
        /// </summary>
        TaskObject
    }

    /// <summary>
    /// 表示方法
    /// </summary>
    public class Method
    {
        /// <summary>
        /// 方法执行委托
        /// </summary>
        private readonly FastInvokeHandler m_invoker;

        /// <summary>
        /// 方法
        /// </summary>
        /// <param name="method">方法信息</param>
        public Method(MethodInfo method)
        {
            this.Info = method ?? throw new ArgumentNullException(nameof(method));
            this.Name = method.Name;
            this.Static = method.IsStatic;
            foreach (var item in method.GetParameters())
            {
                if (item.ParameterType.IsByRef)
                {
                    this.IsByRef = true;
                }
            }
            if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
            {
                //unity
            }
            else
            {
                this.m_invoker = GetMethodInvoker(method);
            }

            if (method.ReturnType == typeof(Task))
            {
                this.HasReturn = false;
                this.TaskType = TaskReturnType.Task;
            }
            else if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                this.HasReturn = true;
                this.ReturnType = method.ReturnType.GetGenericArguments()[0];
                this.TaskType = TaskReturnType.TaskObject;
            }
            else if (method.ReturnType == typeof(void))
            {
                this.HasReturn = false;
                this.TaskType = TaskReturnType.None;
            }
            else
            {
                this.HasReturn = true;
                this.TaskType = TaskReturnType.None;
                this.ReturnType = method.ReturnType;
            }
        }

        /// <summary>
        /// FastInvokeHandler
        /// </summary>
        /// <param name="target"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public delegate object FastInvokeHandler(object target, object[] paramters);

        /// <summary>
        /// 是否具有返回值。当返回值为Task时，也会认为没有返回值。
        /// </summary>
        public bool HasReturn { get; private set; }

        /// <summary>
        /// 方法信息
        /// </summary>
        public MethodInfo Info { get; private set; }

        /// <summary>
        /// 是否有引用类型
        /// </summary>
        public bool IsByRef { get; private set; }

        /// <summary>
        /// 获取方法名
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 返回值类型。
        /// <para>当方法为void或task时，为null</para>
        /// <para>当方法为task泛型时，为泛型元素类型</para>
        /// </summary>
        public Type ReturnType { get; private set; }

        /// <summary>
        /// 是否为静态函数
        /// </summary>
        public bool Static { get; private set; }

        /// <summary>
        /// 返回值的Task类型。
        /// </summary>
        public TaskReturnType TaskType { get; private set; }

        /// <summary>
        /// 执行方法。
        /// <para>当方法为void或task时，会返回null</para>
        /// <para>当方法为task泛型时，会wait后的值</para>
        /// <para>注意：当调用方为UI主线程时，调用异步方法，则极有可能发生死锁。</para>
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public object Invoke(object instance, params object[] parameters)
        {
            switch (this.TaskType)
            {
                case TaskReturnType.None:
                    {
                        object re;
                        if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
                        {
                            re = this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            re = this.m_invoker.Invoke(instance, parameters);
                        }

                        return re;
                    }
                case TaskReturnType.Task:
                    {
                        object re;
                        if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
                        {
                            re = this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            re = this.m_invoker.Invoke(instance, parameters);
                        }

                        var task = (Task)re;
                        task.ConfigureAwait(false).GetAwaiter().GetResult();
                        return default;
                    }
                case TaskReturnType.TaskObject:
                    {
                        object re;
                        if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
                        {
                            re = this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            re = this.m_invoker.Invoke(instance, parameters);
                        }
                        var task = (Task)re;
                        task.ConfigureAwait(false).GetAwaiter().GetResult();
                        return DynamicMethodMemberAccessor.Default.GetValue(task, "Result");
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task InvokeAsync(object instance, params object[] parameters)
        {
            switch (this.TaskType)
            {
                case TaskReturnType.None:
                    {
                        throw new Exception("该方法不包含Task。");
                    }
                case TaskReturnType.Task:
                    {
                        object re;
                        if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
                        {
                            re = this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            re = this.m_invoker.Invoke(instance, parameters);
                        }
                        return (Task)re;
                    }
                case TaskReturnType.TaskObject:
                    {
                        object re;
                        if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
                        {
                            re = this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            re = this.m_invoker.Invoke(instance, parameters);
                        }
                        return (Task)re;
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// 调用异步结果
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<TResult> InvokeAsync<TResult>(object instance, params object[] parameters)
        {
            switch (this.TaskType)
            {
                case TaskReturnType.None:
                    {
                        throw new Exception($"{this.Info}不包含任何Task<>返回值。他可能是个同步函数。");
                    }
                case TaskReturnType.Task:
                    {
                        throw new Exception($"{this.Info}不包含任何可等待的Task<>返回值。");
                    }
                case TaskReturnType.TaskObject:
                    {
                        object re;
                        if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
                        {
                            re = this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            re = this.m_invoker.Invoke(instance, parameters);
                        }
                        return (Task<TResult>)re;
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// 执行方法。
        /// <para>当方法为void或task时，会异常</para>
        /// <para>当方法为task泛型时，会await后的值</para>
        /// <para>支持调用方为UI主线程。</para>
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public async Task<object> InvokeObjectAsync(object instance, params object[] parameters)
        {
            switch (this.TaskType)
            {
                case TaskReturnType.None:
                    {
                        throw new Exception($"{this.Info}不包含任何Task<>返回值。他可能是个同步函数。");
                    }
                case TaskReturnType.Task:
                    {
                        throw new Exception($"{this.Info}不包含任何可等待的Task<>返回值。");
                    }
                case TaskReturnType.TaskObject:
                    {
                        Task task;
                        if (GlobalEnvironment.OptimizedPlatforms.HasFlag(OptimizedPlatforms.Unity))
                        {
                            task = (Task)this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            task = (Task)this.m_invoker.Invoke(instance, parameters);
                        }
                        await task;
                        return DynamicMethodMemberAccessor.Default.GetValue(task, "Result");
                    }
                default:
                    return default;
            }
        }

        private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitCastToReference(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;

                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;

                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;

                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;

                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;

                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;

                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;

                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;

                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;

                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }

        private static FastInvokeHandler GetMethodInvoker(MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object), typeof(object[])
    }, methodInfo.DeclaringType.Module);
            var il = dynamicMethod.GetILGenerator();
            var ps = methodInfo.GetParameters();
            var paramTypes = new Type[ps.Length];
            for (var i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = ps[i].ParameterType.IsByRef ? ps[i].ParameterType.GetElementType() : ps[i].ParameterType;
            }
            var locals = new LocalBuilder[paramTypes.Length];

            for (var i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (var i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            for (var i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }
            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (var i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            var invoder = (FastInvokeHandler)dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));
            return invoder;
        }

        /// <summary>
        /// 生成方法的调用委托
        /// </summary>
        /// <param name="method">方法成员信息</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        private Func<object, object[], object> CreateInvoker(MethodInfo method)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var parameters = Expression.Parameter(typeof(object[]), "parameters");

            var instanceCast = method.IsStatic ? null : Expression.Convert(instance, method.DeclaringType);
            var parametersCast = method.GetParameters().Select((p, i) =>
            {
                var parameter = Expression.ArrayIndex(parameters, Expression.Constant(i));
                return Expression.Convert(parameter, p.ParameterType);
            });

            var body = Expression.Call(instanceCast, method, parametersCast);

            if (method.ReturnType == typeof(Task))
            {
                this.HasReturn = false;
                this.TaskType = TaskReturnType.Task;
                var bodyCast = Expression.Convert(body, typeof(object));
                return Expression.Lambda<Func<object, object[], object>>(bodyCast, instance, parameters).Compile();
            }
            else if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                this.TaskType = TaskReturnType.TaskObject;
                this.HasReturn = true;
                this.ReturnType = method.ReturnType.GetGenericArguments()[0];
                var bodyCast = Expression.Convert(body, typeof(object));
                return Expression.Lambda<Func<object, object[], object>>(bodyCast, instance, parameters).Compile();
            }
            else if (method.ReturnType == typeof(void))
            {
                this.HasReturn = false;
                this.TaskType = TaskReturnType.None;
                var action = Expression.Lambda<Action<object, object[]>>(body, instance, parameters).Compile();
                return (_instance, _parameters) =>
                {
                    action.Invoke(_instance, _parameters);
                    return null;
                };
            }
            else
            {
                this.HasReturn = true;
                this.TaskType = TaskReturnType.None;
                this.ReturnType = method.ReturnType;
                var bodyCast = Expression.Convert(body, typeof(object));
                return Expression.Lambda<Func<object, object[], object>>(bodyCast, instance, parameters).Compile();
            }
        }
    }
}