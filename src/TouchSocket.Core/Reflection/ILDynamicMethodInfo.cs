// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Reflection;
using System.Reflection.Emit;

namespace TouchSocket.Core;

internal class ILDynamicMethodInfo : DynamicMethodInfoBase
{
    private readonly Func<object, object[], object> m_func;
    public ILDynamicMethodInfo(MethodInfo method) : base(method)
    {
        this.m_func = CreateILInvoker(method);
    }

    public override object Invoke(object instance, object[] parameters)
    {
        return this.m_func(instance, parameters);
    }

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
            {
                il.Emit(OpCodes.Ldloca_S, locals[i]);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, locals[i]);
            }
        }
        if (methodInfo.IsStatic)
        {
            il.EmitCall(OpCodes.Call, methodInfo, null);
        }
        else
        {
            il.EmitCall(OpCodes.Callvirt, methodInfo, null);
        }

        if (methodInfo.ReturnType == typeof(void))
        {
            il.Emit(OpCodes.Ldnull);
        }
        else
        {
            EmitBoxIfNeeded(il, methodInfo.ReturnType);
        }

        for (var i = 0; i < paramTypes.Length; i++)
        {
            if (ps[i].ParameterType.IsByRef)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldloc, locals[i]);
                if (locals[i].LocalType.IsValueType)
                {
                    il.Emit(OpCodes.Box, locals[i].LocalType);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }
        }

        il.Emit(OpCodes.Ret);
        var invoker = (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
        return invoker;
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
