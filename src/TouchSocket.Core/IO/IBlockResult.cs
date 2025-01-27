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

/// <summary>
/// 定义了一个泛型接口，用于表示一块不可变内存的处理结果。
/// </summary>
/// <typeparam name="T">内存中元素的类型。</typeparam>
public interface IBlockResult<T> : IDisposable
{
    /// <summary>
    /// 获取只读内存块。
    /// </summary>
    ReadOnlyMemory<T> Memory { get; }

    /// <summary>
    /// 获取表示内存处理是否完成的布尔值。
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// 获取处理结果的消息。
    /// </summary>
    string Message { get; }
}