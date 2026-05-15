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

using System.Runtime.CompilerServices;
using TouchSocket.Core;

namespace TouchSocket.Semi;

/// <summary>
/// 表示包含指定值类型数组的 SECS-II 数据项基类。
/// </summary>
/// <typeparam name="T">数据元素类型（非托管值类型）。</typeparam>
public abstract class SecsItem<T> : SecsItem where T : unmanaged
{
    private ReadOnlyMemory<T> m_values;

    /// <summary>
    /// 初始化 <see cref="SecsItem{T}"/> 的新实例（用于反序列化）。
    /// </summary>
    protected SecsItem() { }

    /// <summary>
    /// 初始化 <see cref="SecsItem{T}"/> 的新实例，并设置初始值。
    /// </summary>
    /// <param name="values">数据元素集合。</param>
    protected SecsItem(ReadOnlyMemory<T> values)
    {
        this.m_values = values;
    }

    /// <summary>
    /// 获取数据项的值集合（只读视图）。
    /// </summary>
    public ReadOnlyMemory<T> Values => this.m_values;

    /// <summary>
    /// 获取单个元素的字节大小。
    /// </summary>
    protected int ElementSize => Unsafe.SizeOf<T>();

    /// <inheritdoc/>
    public override void Package<TWriter>(ref TWriter writer)
    {
        var dataLength = (uint)(this.m_values.Length * this.ElementSize);
        WriteHeader(ref writer, this.SecsFormat, dataLength);
        var size = this.ElementSize;
        foreach (var value in this.m_values.Span)
        {
            WriterExtension.WriteValue(ref writer, value, EndianType.Big);
        }
    }

    /// <inheritdoc/>
    public override void Unpackage<TReader>(ref TReader reader)
    {
        base.Unpackage(ref reader);

        var count = (int)(this.Length / this.ElementSize);
        var values = new T[count];
        var size = this.ElementSize;
        for (var i = 0; i < count; i++)
        {
            values[i] = TouchSocketBitConverter.BigEndian.To<T>(reader.GetSpan(size));
            reader.Advance(size);
        }
        this.m_values = values;
    }
}
