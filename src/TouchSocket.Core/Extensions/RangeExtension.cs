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

namespace TouchSocket.Core;

#if NET6_0_OR_GREATER
/// <summary>
/// RangeExtension
/// </summary>
public static class RangeExtension
{
    /// <summary>
    /// 枚举扩展
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static CustomIntEnumerator GetEnumerator(this Range range)
    {
        return new CustomIntEnumerator(range);
    }
}

/// <summary>
/// CustomIntEnumerator
/// </summary>
public ref struct CustomIntEnumerator
{
    private int m_current;
    private readonly int m_end;

    /// <summary>
    /// CustomIntEnumerator
    /// </summary>
    /// <param name="range"></param>
    public CustomIntEnumerator(Range range)
    {
        if (range.End.IsFromEnd)
        {
            throw new NotSupportedException("不支持无限枚举。");
        }
        this.m_current = range.Start.Value - 1;
        this.m_end = range.End.Value;
    }

    /// <summary>
    /// Current
    /// </summary>
    public int Current => this.m_current;

    /// <summary>
    /// MoveNext
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        this.m_current++;
        return this.m_current <= this.m_end;
    }
}
#endif
