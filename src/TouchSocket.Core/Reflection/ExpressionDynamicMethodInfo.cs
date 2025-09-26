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

using System.Linq.Expressions;
using System.Reflection;

namespace TouchSocket.Core;

internal class ExpressionDynamicMethodInfo : DynamicMethodInfoBase
{
    private readonly Func<object, object[], object> m_func;
    public ExpressionDynamicMethodInfo(MethodInfo method) : base(method)
    {
        this.m_func = this.CreateExpressionInvoker(method);
    }

    public override object Invoke(object instance, object[] parameters)
    {
        return this.m_func.Invoke(instance, parameters);
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

        switch (this.ReturnKind)
        {
            case MethodReturnKind.Void:
                {
                    var action = Expression.Lambda<Action<object, object[]>>(body, instance, parameters).Compile();
                    return (_instance, _parameters) =>
                    {
                        action.Invoke(_instance, _parameters);
                        return null;
                    };
                }
            default:
                {
                    var bodyCast = Expression.Convert(body, typeof(object));
                    return Expression.Lambda<Func<object, object[], object>>(bodyCast, instance, parameters).Compile();
                }
        }
    }


}
