//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 一个动态调用方法
    /// </summary>
    public class Method
    {
        private readonly MethodInfo m_info;
        private Func<object, object[], object> m_invoker;

        private readonly string m_generatorTypeNamespace = "TouchSocket.Core.__Internals";

        /// <summary>
        /// 初始化一个动态调用方法
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <param name="dynamicBuilderType">指定构建的类型</param>
        public Method(MethodInfo method, DynamicBuilderType? dynamicBuilderType = default)
        {
            this.m_info = ThrowHelper.ThrowArgumentNullExceptionIf(method, nameof(method));
            this.Name = method.Name;

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

            if (dynamicBuilderType.HasValue)
            {
                switch (dynamicBuilderType.Value)
                {
                    case DynamicBuilderType.IL:
                        if (!CreateInvokeFromIL())
                        {
                            ThrowHelper.ThrowNotSupportedException($"当前环境不支持{dynamicBuilderType.Value}");
                        }
                        break;
                    case DynamicBuilderType.Expression:
                        if (!CreateInvokeFromExpression())
                        {
                            ThrowHelper.ThrowNotSupportedException($"当前环境不支持{dynamicBuilderType.Value}");
                        }
                        break;
                    case DynamicBuilderType.SourceGenerator:
                        if (!CreateInvokeFromSG())
                        {
                            ThrowHelper.ThrowNotSupportedException($"当前环境不支持{dynamicBuilderType.Value}");
                        }
                        break;
                }
            }
            else
            {
                this.CreateInvokeFromSG();
                this.CreateInvokeFromIL();
                this.CreateInvokeFromExpression();
            }

            if (this.m_invoker == null)
            {
                this.DynamicBuilderType = DynamicBuilderType.Reflect;
            }
        }
        private bool CreateInvokeFromIL()
        {
            if (this.m_invoker != null)
            {
                return false;
            }
            try
            {
                this.m_invoker = CreateILInvoker(this.Info);
                this.DynamicBuilderType = DynamicBuilderType.IL;
                return true;
            }
            catch
            {
                return false;
            }
        }


        private bool CreateInvokeFromExpression()
        {
            if (this.m_invoker != null)
            {
                return false;
            }
            try
            {
                this.m_invoker = this.CreateExpressionInvoker(this.Info);
                this.DynamicBuilderType = DynamicBuilderType.Expression;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CreateInvokeFromSG()
        {
            if (this.m_invoker!=null)
            {
                return false;
            }

            var typeName = $"{this.m_generatorTypeNamespace}.__{StringExtension.MakeIdentifier(this.Info.DeclaringType.FullName)}MethodExtension";

            var type = this.Info.DeclaringType.Assembly.GetType(typeName);
            if (type == null)
            {
                return false;
            }

            var methodName = $"{this.Info.GetDeterminantName()}Func";
            var property = type.GetProperty(methodName, BindingFlags.Public | BindingFlags.Static);
            if (property == null)
            {
                return false;
            }

            this.m_invoker = (Func<object, object[], object>)property.GetValue(null);
            if (this.m_invoker == null)
            {
                return false;
            }

            this.DynamicBuilderType = DynamicBuilderType.SourceGenerator;
            return true;
        }

        /// <summary>
        /// 获取调用器的构建类型。
        /// </summary>
        public DynamicBuilderType DynamicBuilderType { get; private set; }

        /// <summary>
        /// 是否具有返回值。当返回值为Task时，也会认为没有返回值。
        /// </summary>
        public bool HasReturn { get; private set; }

        /// <summary>
        /// 方法信息
        /// </summary>
        public MethodInfo Info => this.m_info;

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
        /// 返回值的Task类型。
        /// </summary>
        public TaskReturnType TaskType { get; private set; }

        /// <summary>
        /// 执行方法。
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public object Invoke(object instance, params object[] parameters)
        {
            if (this.m_invoker == null)
            {
                return this.Info.Invoke(instance, parameters);
            }
            else
            {
                return this.m_invoker.Invoke(instance, parameters);
            }
        }

        /// <summary>
        /// 异步执行方法。
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回一个表示异步操作的任务。</returns>
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
                        if (this.m_invoker == null)
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
                        if (this.m_invoker == null)
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
                        if (this.m_invoker == null)
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
                        if (this.m_invoker == null)
                        {
                            task = (Task)this.Info.Invoke(instance, parameters);
                        }
                        else
                        {
                            task = (Task)this.m_invoker.Invoke(instance, parameters);
                        }

                        await task.ConfigureAwait(false);
                        return DynamicMethodMemberAccessor.Default.GetValue(task, "Result");
                    }
                default:
                    return default;
            }
        }

        /// <summary>
        /// 构建IL调用
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        protected static Func<object, object[], object> CreateILInvoker(MethodInfo methodInfo)
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
            var invoder = (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
            return invoder;
        }

        /// <summary>
        /// 构建表达式树调用
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        protected Func<object, object[], object> CreateExpressionInvoker(MethodInfo method)
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
    }
}