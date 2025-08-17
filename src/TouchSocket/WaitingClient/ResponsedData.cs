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
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 表示响应数据的结构体，包含响应的字节数据和请求信息。
/// </summary>
public readonly struct ResponsedData : IDisposable
{
    private readonly ByteBlock m_byteBlock;

    /// <summary>
    /// 初始化 <see cref="ResponsedData"/> 结构体的新实例。
    /// </summary>
    /// <param name="memory">响应的字节数据。</param>
    /// <param name="requestInfo">请求信息。</param>
    public ResponsedData(ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
    {
        if (!memory.IsEmpty)
        {
            var data = memory.Span;
            this.m_byteBlock = new ByteBlock(data.Length);
            this.m_byteBlock.Write(data);
            this.m_byteBlock.SeekToStart();
        }

        this.RequestInfo = requestInfo;
    }

    /// <summary>
    /// 获取响应的字节数据。
    /// </summary>
    public ReadOnlyMemory<byte> Memory => this.m_byteBlock?.Memory ?? ReadOnlyMemory<byte>.Empty;

    /// <summary>
    /// 获取请求信息。
    /// </summary>
    public IRequestInfo RequestInfo { get; }

    /// <summary>
    /// 释放资源。
    /// </summary>
    public void Dispose()
    {
        this.m_byteBlock.SafeDispose();
    }
}