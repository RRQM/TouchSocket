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
using System.Net.Sockets;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// Socket的扩展方法类
/// </summary>
public static class SocketExtension
{
    /// <summary>
    /// 绝对发送数据。
    /// 该方法使用指定的Socket对象，将数据从缓冲区发送到远程主机。
    /// 它确保所有数据都被发送，即使需要多次调用Socket的Send方法。
    /// </summary>
    /// <param name="socket">用于发送数据的Socket对象。</param>
    /// <param name="buffer">包含要发送的数据的字节数组。</param>
    /// <param name="offset">字节数组中开始发送数据的索引。</param>
    /// <param name="length">要发送的数据长度。</param>
    /// <exception cref="System.Net.Sockets.SocketException">当数据发送失败时抛出异常。</exception>
    [Obsolete("此方法可能会与内部发生逻辑产生歧义，已被弃用")]
    public static void AbsoluteSend(this Socket socket, byte[] buffer, int offset, int length)
    {
        // 对Socket对象加锁，以确保线程安全。
        lock (socket)
        {
            // 循环直到所有数据被发送。
            while (length > 0)
            {
                // 尝试发送数据，返回实际发送的字节数。
                var r = socket.Send(buffer, offset, length, SocketFlags.None);
                // 如果发送失败（发送字节数为0）且仍有数据待发送，则抛出异常。
                if (r == 0 && length > 0)
                {
                    ThrowHelper.ThrowException(TouchSocketResource.IncompleteDataTransmission);
                }
                // 更新缓冲区索引和剩余待发送长度。
                offset += r;
                length -= r;
            }
        }
    }
    /// <summary>
    /// 尝试关闭<see cref="Socket"/>。不会抛出异常。
    /// </summary>
    /// <param name="socket"></param>
    public static Result SafeClose(this Socket socket)
    {
        try
        {
            if (socket.Connected)
            {
                socket.Close();
            }
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex.Message);
        }
    }
}