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

using System.Linq.Expressions;
using System.Reflection;

namespace TouchSocket.Core;

internal class MemberSetter
{

    private readonly Action<object, object> setFunc;
    
    /// <summary>
    /// 是否为静态成员
    /// </summary>
    private readonly bool m_isStatic;


    public MemberSetter(PropertyInfo property)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }
        var setMethod = property.GetSetMethod(true);
        this.m_isStatic = setMethod != null && setMethod.IsStatic;
        this.setFunc = CreateSetterDelegate(property, this.m_isStatic);
    }

    public MemberSetter(FieldInfo fieldInfo)
    {
        if (fieldInfo == null)
        {
            throw new ArgumentNullException(nameof(fieldInfo));
        }
        this.m_isStatic = fieldInfo.IsStatic;
        this.setFunc = CreateSetterDelegate(fieldInfo, this.m_isStatic);
    }


    public void Invoke(object instance, object value)
    {
        this.setFunc.Invoke(instance, value);
    }

    private static Action<object, object> CreateSetterDelegate(PropertyInfo property, bool isStatic)
    {
        var param_instance = Expression.Parameter(typeof(object));
        var param_value = Expression.Parameter(typeof(object));

        var body_value = Expression.Convert(param_value, property.PropertyType);
        
        if (isStatic)
        {
            var body_call = Expression.Call(null, property.GetSetMethod(true), body_value);
            return Expression.Lambda<Action<object, object>>(body_call, param_instance, param_value).Compile();
        }
        else
        {
            var body_instance = Expression.Convert(param_instance, property.DeclaringType);
            var body_call = Expression.Call(body_instance, property.GetSetMethod(true), body_value);
            return Expression.Lambda<Action<object, object>>(body_call, param_instance, param_value).Compile();
        }
    }

    private static Action<object, object> CreateSetterDelegate(FieldInfo fieldInfo, bool isStatic)
    {
        var param_instance = Expression.Parameter(typeof(object));
        var param_value = Expression.Parameter(typeof(object));

        var body_value = Expression.Convert(param_value, fieldInfo.FieldType);
        
        if (isStatic)
        {
            var body_field = Expression.Field(null, fieldInfo);
            var body_assign = Expression.Assign(body_field, body_value);
            return Expression.Lambda<Action<object, object>>(body_assign, param_instance, param_value).Compile();
        }
        else
        {
            var body_instance = Expression.Convert(param_instance, fieldInfo.DeclaringType);
            var body_field = Expression.Field(body_instance, fieldInfo);
            var body_assign = Expression.Assign(body_field, body_value);
            return Expression.Lambda<Action<object, object>>(body_assign, param_instance, param_value).Compile();
        }
    }
}