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
/// 表示 SECS-II List 格式数据项，包含子数据项列表。
/// </summary>
public class ListSecsItem : SecsItem
{
    private SecsItem[] m_items = [];

    /// <inheritdoc/>
    public override SecsFormat SecsFormat => SecsFormat.List;

    /// <summary>
    /// 获取子数据项集合（只读视图）。
    /// </summary>
    public ReadOnlyMemory<SecsItem> Items => this.m_items;

    /// <inheritdoc/>
    public override void Package<TWriter>(ref TWriter writer)
    {
        WriteHeader(ref writer, SecsFormat.List, (uint)this.m_items.Length);
        foreach (var item in this.m_items)
        {
            item.Package(ref writer);
        }
    }

    /// <inheritdoc/>
    public override void Unpackage<TReader>(ref TReader reader)
    {
        base.Unpackage(ref reader);

        var items = new SecsItem[this.Length];
        for (var i = 0; i < this.Length; i++)
        {
            items[i] = ReadSecsItem(ref reader);
        }
        this.m_items = items;
    }
}
