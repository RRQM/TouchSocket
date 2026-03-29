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
using System.Text.Json;

namespace TouchSocket.Core;

/// <summary>
/// 基于 <see cref="System.Text.Json.JsonSerializer"/> 实现的 <see cref="ISerializerFormatter{TSource,TState}"/>，
/// 在 <see cref="ReadOnlyMemory{T}"/> (byte) 与对象之间进行 JSON 序列化/反序列化。
/// </summary>
/// <typeparam name="TState">转换器状态类型。</typeparam>
public class JsonBytesToClassSerializerFormatter<TState> : SystemTextJsonSerializerFormatter<ReadOnlyMemory<byte>, TState>
{
    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "targetType 参数已通过 DynamicallyAccessedMembers(All) 标注，可确保所有必要成员被保留。")]
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050",
        Justification = "targetType 参数已通过 DynamicallyAccessedMembers(All) 标注，可确保所有必要成员被保留。")]
    public override bool TryDeserialize(TState state, in ReadOnlyMemory<byte> source, [DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] Type targetType, out object target)
    {
        try
        {
            if (source.IsEmpty)
            {
                target = targetType.GetDefault();
                return true;
            }
            target = JsonSerializer.Deserialize(source.Span, targetType, this.JsonSettings);
            return true;
        }
        catch
        {
            target = null;
            return false;
        }
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "此格式化器专为基于反射的 JSON 序列化设计，调用方需自行确保运行时类型可被裁剪工具保留。")]
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050",
        Justification = "此格式化器专为基于反射的 JSON 序列化设计，不适用于 Native AOT 场景。")]
    public override bool TrySerialize<TTarget>(TState state, in TTarget target, out ReadOnlyMemory<byte> source)
    {
        try
        {
            if (target is null)
            {
                source = default;
                return true;
            }
            source = JsonSerializer.Serialize(target, target.GetType(), this.JsonSettings).ToUtf8Bytes();
            return true;
        }
        catch
        {
            source = null;
            return false;
        }
    }
}
