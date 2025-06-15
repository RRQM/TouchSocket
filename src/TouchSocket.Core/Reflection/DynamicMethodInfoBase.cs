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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TouchSocket.Core;

abstract class DynamicMethodInfoBase : IDynamicMethodInfo
{
    public DynamicMethodInfoBase(MethodInfo method)
    {
        if (method.ReturnType == typeof(void))
        {
            this.ReturnKind = MethodReturnKind.Void;
        }
        else if (IsTypeAwaitable(method.ReturnType, out var returnType))
        {
            if (returnType is null)
            {
                this.ReturnKind = MethodReturnKind.Awaitable;
            }
            else
            {
                this.RealReturnType = returnType;
                this.ReturnKind = MethodReturnKind.AwaitableObject;
            }
        }
        else if (method.ReturnType == typeof(Task) || method.ReturnType == typeof(ValueTask))
        {
            this.ReturnKind = MethodReturnKind.Awaitable;
        }
        else if (method.ReturnType.IsGenericType && (method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) || method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
        {
            this.RealReturnType = method.ReturnType.GetGenericArguments().First();
            this.ReturnKind = MethodReturnKind.AwaitableObject;
        }
        else
        {
            this.RealReturnType = method.ReturnType;
            this.ReturnKind = MethodReturnKind.Object;
        }
    }
    public Type RealReturnType { get; set; }

    public MethodReturnKind ReturnKind { get; set; }

    public async Task<object> GetResultAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] object result)
    {
        if (result is Task task)
        {
            await task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (this.ReturnKind == MethodReturnKind.AwaitableObject)
            {
                return DynamicMethodMemberAccessor.Default.GetValue(task, nameof(Task<object>.Result));
            }
            return null;
        }
        ThrowHelper.ThrowException("当源生成无法使用时，无法处理非Task的Awaitable对象。");
        return null;
    }

    public abstract object Invoke(object instance, object[] parameters);

    private static bool IsTypeAwaitable([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, out Type returnType)
    {
        returnType = null;
        // 1. 查找GetAwaiter实例方法（无参数）
        var getAwaiterMethod = type.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
        if (getAwaiterMethod == null)
        {
            return false;
        }

        // 2. 获取Awaiter类型
        var awaiterType = getAwaiterMethod.ReturnType;

        // 3. 验证Awaiter是否实现必要的接口
        var inotifyCompletion = typeof(System.Runtime.CompilerServices.INotifyCompletion);
        var icriticalNotifyCompletion = typeof(System.Runtime.CompilerServices.ICriticalNotifyCompletion);

        var implementsInterface = awaiterType.GetInterfaces().Any(i =>
            i == inotifyCompletion || i == icriticalNotifyCompletion);
        if (!implementsInterface)
        {
            return false;
        }

        // 4. 检查IsCompleted属性
        var isCompletedProp = awaiterType.GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance);
        if (isCompletedProp == null ||
            isCompletedProp.PropertyType != typeof(bool) ||
            !isCompletedProp.CanRead)
        {
            return false;
        }

        // 5. 检查GetResult方法
        var getResultMethod = awaiterType.GetMethod("GetResult", BindingFlags.Public | BindingFlags.Instance);
        if (getResultMethod == null)
        {
            return false;
        }

        // 6. 检查GetResult方法的返回类型
        if (getResultMethod.ReturnType == typeof(void))
        {
            returnType = null;
        }
        else
        {
            returnType = getResultMethod.ReturnType;
        }

        return true;
    }

}
