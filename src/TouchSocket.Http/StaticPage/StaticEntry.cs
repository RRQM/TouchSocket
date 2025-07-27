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
using System.IO;

namespace TouchSocket.Http;

/// <summary>
/// 表示一个静态条目，可以是字节缓存或文件信息。
/// </summary>
public class StaticEntry
{
    /// <summary>
    /// 使用字节数组和超时时间初始化静态条目。
    /// </summary>
    /// <param name="value">条目的值，如果条目代表的是字节缓存。</param>
    /// <param name="timeout">条目的超时时间。</param>
    public StaticEntry(byte[] value, TimeSpan timeout)
    {
        this.Value = value;
        this.Timespan = timeout;
    }

    /// <summary>
    /// 使用FileInfo和超时时间初始化静态条目。
    /// </summary>
    /// <param name="fileInfo">条目代表的文件信息。</param>
    /// <param name="timespan">条目的超时时间。</param>
    public StaticEntry(FileInfo fileInfo, TimeSpan timespan)
    {
        this.FileInfo = fileInfo;
        this.Timespan = timespan;
    }

    /// <summary>
    /// 获取一个值，指示当前条目是否为字节缓存。
    /// </summary>
    /// <value>如果条目的值非空，则为<see langword="true"/>；否则为<see langword="false"/>。</value>
    public bool IsCacheBytes => this.Value != null;

    /// <summary>
    /// 获取或设置与条目关联的文件信息。
    /// </summary>
    public FileInfo FileInfo { get; set; }

    /// <summary>
    /// 获取与条目关联的字节数组。该值在初始化后不可更改。
    /// </summary>
    public byte[] Value { get; }

    /// <summary>
    /// 获取或设置条目的超时时间。
    /// </summary>
    public TimeSpan Timespan { get; set; }
}