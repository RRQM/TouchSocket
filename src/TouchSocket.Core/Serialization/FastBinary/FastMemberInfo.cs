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
using System.Reflection;

namespace TouchSocket.Core;

internal class FastMemberInfo
{
    public byte Index;
    private readonly PropertyInfo m_propertyInfo;
    private readonly FieldInfo m_fieldInfo;
    private readonly bool m_isField;

    public FastMemberInfo(MemberInfo memberInfo, bool enableIndex)
    {
        if (enableIndex)
        {
            this.Index = memberInfo.GetCustomAttribute(typeof(FastMemberAttribute), false) is FastMemberAttribute fastMamberAttribute
                ? fastMamberAttribute.Index
                : throw new Exception($"成员{memberInfo.Name}未标识{nameof(FastMemberAttribute)}特性。");
        }

        if (memberInfo is PropertyInfo propertyInfo)
        {
            this.m_propertyInfo = propertyInfo;
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            this.m_isField = true;
            this.m_fieldInfo = fieldInfo;
        }
    }

    public string Name => this.m_isField ? this.m_fieldInfo.Name : this.m_propertyInfo.Name;

    public Type Type => this.m_isField ? this.m_fieldInfo.FieldType : this.m_propertyInfo.PropertyType;

    public void SetValue(ref object instance, object obj)
    {
        if (this.m_isField)
        {
            this.m_fieldInfo.SetValue(instance, obj);
        }
        else
        {
            this.m_propertyInfo.SetValue(instance, obj);
        }
    }
}