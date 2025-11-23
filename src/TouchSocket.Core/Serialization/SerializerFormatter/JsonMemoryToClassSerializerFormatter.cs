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

using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Core;

/// <summary>
/// 定义一个类 JsonMemoryToClassSerializerFormatter，用于将只读内存中的字节序列反序列化为指定的状态类。
/// 该类实现了 ISerializerFormatter 接口，特化于 ReadOnlyMemory{byte} 类型的输入和 TState 类型的输出。
/// </summary>
/// <typeparam name="TState">要反序列化的状态类类型。</typeparam>
public class JsonMemoryToClassSerializerFormatter<TState> : ISerializerFormatter<ReadOnlyMemory<byte>, TState>
{
    /// <summary>
    /// JsonSettings
    /// </summary>
    public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

    /// <inheritdoc/>
    public int Order { get; set; }

    /// <inheritdoc/>
    public bool TryDeserialize(TState state, in ReadOnlyMemory<byte> source, [DynamicallyAccessedMembers(AOT.SerializerFormatterMemberType)] Type targetType, out object target)
    {
        try
        {
            target = JsonConvert.DeserializeObject(source.Span.ToString(Encoding.UTF8), targetType, this.JsonSettings);
            return true;
        }
        catch
        {
            target = default;
            return false;
        }
    }

    /// <inheritdoc/>
    public bool TrySerialize<TTarget>(TState state, in TTarget target, out ReadOnlyMemory<byte> source)
    {
        try
        {
            source = JsonConvert.SerializeObject(target, this.JsonSettings).ToUtf8Bytes();
            return true;
        }
        catch (Exception)
        {
            source = null;
            return false;
        }
    }
}