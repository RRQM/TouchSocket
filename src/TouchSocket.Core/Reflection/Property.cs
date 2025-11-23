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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 表示属性
/// </summary>
public sealed class Property
{
    /// <summary>
    /// 获取器
    /// </summary>
    private readonly MemberGetter m_geter;

    /// <summary>
    /// 设置器
    /// </summary>
    private readonly MemberSetter m_seter;

    /// <summary>
    /// 属性
    /// </summary>
    /// <param name="property">属性信息</param>
    public Property(PropertyInfo property)
    {
        this.Info = property;

        if (property.CanRead == true)
        {
            this.m_geter = new MemberGetter(property);
        }
        if (property.CanWrite == true)
        {
            this.m_seter = new MemberSetter(property);
        }
    }

    /// <summary>
    /// 获取属性信息
    /// </summary>
    public PropertyInfo Info { get; }

    /// <summary>
    /// 从类型的属性获取属性
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static Property[] GetProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
    {
        return type.GetProperties().Select(p => new Property(p)).ToArray();
    }

    /// <summary>
    /// 获取属性的值
    /// </summary>
    /// <param name="instance">实例</param>
    /// <exception cref="NotSupportedException"></exception>
    /// <returns></returns>
    public object GetValue(object instance)
    {
        return this.m_geter == null ? throw new NotSupportedException() : this.m_geter.Invoke(instance);
    }

    /// <summary>
    /// 设置属性的值
    /// </summary>
    /// <param name="instance">实例</param>
    /// <param name="value">值</param>
    /// <exception cref="NotSupportedException"></exception>
    public void SetValue(object instance, object value)
    {
        if (this.m_seter == null)
        {
            throw new NotSupportedException($"{this.Info.Name}不允许赋值");
        }
        this.m_seter.Invoke(instance, value);
    }
}