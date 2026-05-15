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

using TouchSocket.Core;

namespace TouchSocket.Semi;

/// <summary>
/// 表示 SECS-II Binary 格式数据项。
/// </summary>
public class BinarySecsItem : SecsItem
{
    private ReadOnlyMemory<byte> m_data;

    /// <summary>
    /// 初始化 <see cref="BinarySecsItem"/> 的新实例（用于反序列化）。
    /// </summary>
    public BinarySecsItem() { }

    /// <summary>
    /// 初始化 <see cref="BinarySecsItem"/> 的新实例，并设置二进制数据。
    /// </summary>
    /// <param name="data">二进制数据。</param>
    public BinarySecsItem(ReadOnlyMemory<byte> data)
    {
        this.m_data = data;
    }

    /// <inheritdoc/>
    public override SecsFormat SecsFormat => SecsFormat.Binary;

    /// <summary>
    /// 获取二进制数据（只读视图）。
    /// </summary>
    public ReadOnlyMemory<byte> Data => this.m_data;

    /// <inheritdoc/>
    public override void Package<TWriter>(ref TWriter writer)
    {
        WriteHeader(ref writer, SecsFormat.Binary, (uint)this.m_data.Length);
        var span = writer.GetSpan(this.m_data.Length);
        this.m_data.Span.CopyTo(span);
        writer.Advance(this.m_data.Length);
    }

    /// <inheritdoc/>
    public override void Unpackage<TReader>(ref TReader reader)
    {
        base.Unpackage(ref reader);

        var length = (int)this.Length;
        this.m_data = reader.GetSpan(length).Slice(0, length).ToArray();
        reader.Advance(length);
    }
}
