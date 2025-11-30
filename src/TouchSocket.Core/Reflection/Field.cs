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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个字段的封装。
/// </summary>
public sealed class Field
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
    /// 字段
    /// </summary>
    /// <param name="fieldInfo">字段信息。</param>
    public Field(FieldInfo fieldInfo)
    {
        this.Info = fieldInfo;

        this.m_geter = new MemberGetter(fieldInfo);

        if (!fieldInfo.IsInitOnly)
        {
            this.m_seter = new MemberSetter(fieldInfo);
        }
    }

    /// <summary>
    /// 获取字段信息。
    /// </summary>
    public FieldInfo Info { get; }

    /// <summary>
    /// 从类型的字段获取字段。
    /// </summary>
    /// <param name="type">类型。</param>
    /// <returns>字段数组。</returns>
    public static Field[] GetFields([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type)
    {
        return type.GetFields().Select(p => new Field(p)).ToArray();
    }

    /// <summary>
    /// 获取字段的值。
    /// </summary>
    /// <param name="instance">实例。</param>
    /// <returns>字段的值。</returns>
    /// <exception cref="NotSupportedException">当字段不支持获取值时抛出。</exception>
    public object GetValue(object instance)
    {
        return this.m_geter == null ? throw new NotSupportedException() : this.m_geter.Invoke(instance);
    }

    /// <summary>
    /// 设置字段的值。
    /// </summary>
    /// <param name="instance">实例。</param>
    /// <param name="value">值。</param>
    /// <exception cref="NotSupportedException">当字段不支持设置值时抛出。</exception>
    public void SetValue(object instance, object value)
    {
        if (this.m_seter == null)
        {
            throw new NotSupportedException($"{this.Info.Name}不允许赋值");
        }
        this.m_seter.Invoke(instance, value);
    }
}

