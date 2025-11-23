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
/// 序列化转换器接口。
/// </summary>
public interface ISerializerFormatter<TSource, TState>
{
    /// <summary>
    /// 转换器执行顺序。
    /// <para>该属性值越小，越靠前执行。值相等时，按添加先后顺序。</para>
    /// <para>该属性效果，仅在 <see cref="TouchSocketSerializerConverter{TSource, TState}.Add(ISerializerFormatter{TSource, TState})"/> 之前设置有效。</para>
    /// </summary>
    int Order { get; set; }

    /// <summary>
    /// 尝试将源数据反序列化为目标类型对象。
    /// </summary>
    /// <param name="state">转换器状态。</param>
    /// <param name="source">源数据。</param>
    /// <param name="targetType">目标类型。</param>
    /// <param name="target">转换后的目标对象。</param>
    /// <returns>操作是否成功。</returns>
    bool TryDeserialize(TState state, in TSource source, [DynamicallyAccessedMembers(AOT.SerializerFormatterMemberType)] Type targetType, out object target);

    /// <summary>
    /// 尝试将目标类型对象序列化为源数据。
    /// </summary>
    /// <typeparam name="TTarget">目标对象类型。</typeparam>
    /// <param name="state">转换器状态。</param>
    /// <param name="target">目标对象。</param>
    /// <param name="source">序列化后的源数据。</param>
    /// <returns>操作是否成功。</returns>
    bool TrySerialize<TTarget>(TState state, in TTarget target, out TSource source);
}