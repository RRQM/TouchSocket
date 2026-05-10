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

using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Semi;

/// <summary>
/// 表示基于字符串编码的 SECS-II 数据项抽象基类。
/// </summary>
public abstract class StringSecsItem : SecsItem
{
    private string m_value = string.Empty;

    /// <summary>
    /// 初始化 <see cref="StringSecsItem"/> 的新实例。
    /// </summary>
    /// <param name="encoding">字符串编码。</param>
    protected StringSecsItem(Encoding encoding)
    {
        this.Encoding = encoding;
    }

    /// <summary>
    /// 获取所使用的字符串编码。
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    /// 获取字符串值。
    /// </summary>
    public string Value => this.m_value;

    /// <inheritdoc/>
    public override void Package<TWriter>(ref TWriter writer)
    {
        var bytes = this.Encoding.GetBytes(this.m_value);
        WriteHeader(ref writer, this.SecsFormat, (uint)bytes.Length);
        var span = writer.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        writer.Advance(bytes.Length);
    }

    /// <inheritdoc/>
    public override void Unpackage<TReader>(ref TReader reader)
    {
        base.Unpackage(ref reader);

        var length = (int)this.Length;
        var span = reader.GetSpan(length).Slice(0, length);
        this.m_value = this.Encoding.GetString(span.ToArray());
        reader.Advance(length);
    }
}
