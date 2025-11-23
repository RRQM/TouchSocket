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

namespace TouchSocket.Core;

/// <summary>
/// String值转换为基础类型。
/// </summary>
public class StringToPrimitiveSerializerFormatter<TState> : ISerializerFormatter<string, TState>
{
    /// <inheritdoc/>
    public int Order { get; set; }

    /// <inheritdoc/>
    public virtual bool TryDeserialize(TState state, in string source, [DynamicallyAccessedMembers(AOT.SerializerFormatterMemberType)] Type targetType, out object target)
    {
        if (targetType.IsPrimitive())
        {
            return StringExtension.TryParseToType(source, targetType, out target);
        }
        target = default;
        return false;
    }

    /// <inheritdoc/>
    public virtual bool TrySerialize<TTarget>(TState state, in TTarget target, out string source)
    {
        if (target != null)
        {
            var type = target.GetType();
            if (type.IsPrimitive())
            {
                source = target.ToString();
                return true;
            }
        }

        source = null;
        return false;
    }
}