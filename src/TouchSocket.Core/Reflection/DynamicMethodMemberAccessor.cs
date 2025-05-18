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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// DynamicMethodMemberAccessor
/// </summary>
public class DynamicMethodMemberAccessor : IMemberAccessor
{
    private readonly ConcurrentDictionary<Type, IMemberAccessor> m_classAccessors = new ConcurrentDictionary<Type, IMemberAccessor>();

    static DynamicMethodMemberAccessor()
    {
        Default = new DynamicMethodMemberAccessor();
    }

    /// <summary>
    /// DynamicMethodMemberAccessor的默认实例。
    /// </summary>
    public static DynamicMethodMemberAccessor Default { get; private set; }

    /// <summary>
    /// 获取字段
    /// </summary>
    public Func<Type, FieldInfo[]> OnGetFieldInfes { get; set; }

    /// <summary>
    /// 获取属性
    /// </summary>
    public Func<Type, PropertyInfo[]> OnGetProperties { get; set; }

    /// <inheritdoc/>
    public object GetValue([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] object instance, string memberName)
    {
        return this.FindClassAccessor(instance).GetValue(instance, memberName);
    }

    /// <inheritdoc/>
    public void SetValue([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] object instance, string memberName, object newValue)
    {
        this.FindClassAccessor(instance).SetValue(instance, memberName, newValue);
    }

    private IMemberAccessor FindClassAccessor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] object instance)
    {
        var typeKey = instance.GetType();
        if (!this.m_classAccessors.TryGetValue(typeKey, out var classAccessor))
        {
            var memberAccessor = new MemberAccessor(typeKey);
            if (this.OnGetFieldInfes != null)
            {
                memberAccessor.OnGetFieldInfos = this.OnGetFieldInfes;
            }

            if (this.OnGetProperties != null)
            {
                memberAccessor.OnGetProperties = this.OnGetProperties;
            }
            memberAccessor.Build();
            classAccessor = memberAccessor;
            this.m_classAccessors.TryAdd(typeKey, classAccessor);
        }
        return classAccessor;
    }
}