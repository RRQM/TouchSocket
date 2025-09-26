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

using System.Net;
using System.Net.Sockets;

namespace TouchSocket.Sockets;

internal sealed class UdpSocketReceiver : SocketAwaitableEventArgs<UdpOperationResult>
{
    public ValueTask<UdpOperationResult> ReceiveAsync(Socket socket, EndPoint endPoint, Memory<byte> buffer)
    {
        this.m_core.Reset();
#if NET6_0_OR_GREATER
        this.SetBuffer(buffer);
#else
        var segment = buffer.GetArray();

        this.SetBuffer(segment.Array, segment.Offset, segment.Count);
#endif
        this.RemoteEndPoint = endPoint;
        if (socket.ReceiveFromAsync(this))
        {
            return new ValueTask<UdpOperationResult>(this, this.m_core.Version);
        }

        return new ValueTask<UdpOperationResult>(this.GetResult());
    }

    protected override UdpOperationResult GetResult()
    {
        return new UdpOperationResult(this.BytesTransferred, this.RemoteEndPoint, this.SocketError, this.ReceiveMessageFromPacketInfo);
    }
}