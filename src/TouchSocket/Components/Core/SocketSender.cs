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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    internal sealed class SocketSender : SocketAwaitableEventArgs
    {
        private List<ArraySegment<byte>> m_bufferList;

        public ValueTask<SocketOperationResult> SendAsync(Socket socket, in ReadOnlySequence<byte> buffers)
        {
            if (buffers.IsSingleSegment)
            {
                return this.SendAsync(socket, buffers.First);
            }

            this.SetBufferList(buffers);

            if (socket.SendAsync(this))
            {
                return new ValueTask<SocketOperationResult>(this, 0);
            }

            var bytesTransferred = this.BytesTransferred;
            var error = this.SocketError;

            return error == SocketError.Success
                ? new ValueTask<SocketOperationResult>(new SocketOperationResult(bytesTransferred))
                : new ValueTask<SocketOperationResult>(new SocketOperationResult(CreateException(error)));
        }

        public void Reset()
        {
            if (this.BufferList != null)
            {
                this.BufferList = null;

                this.m_bufferList?.Clear();
            }
            else
            {
                this.SetBuffer(null, 0, 0);
            }
        }

        public ValueTask<SocketOperationResult> SendAsync(Socket socket,in ReadOnlyMemory<byte> memory)
        {
#if NET6_0_OR_GREATER
            this.SetBuffer(MemoryMarshal.AsMemory(memory));
#else
            var segment = MemoryMarshal.AsMemory(memory).GetArray();

            this.SetBuffer(segment.Array, segment.Offset, segment.Count);
#endif
            if (socket.SendAsync(this))
            {
                return new ValueTask<SocketOperationResult>(this, 0);
            }

            var bytesTransferred = this.BytesTransferred;
            var error = this.SocketError;

            return error == SocketError.Success
                ? new ValueTask<SocketOperationResult>(new SocketOperationResult(bytesTransferred))
                : new ValueTask<SocketOperationResult>(new SocketOperationResult(CreateException(error)));
        }

        private void SetBufferList(in ReadOnlySequence<byte> buffer)
        {
            this.m_bufferList ??= new List<ArraySegment<byte>>();

            foreach (var b in buffer)
            {
                this.m_bufferList.Add(b.GetArray());
            }
            this.BufferList = this.m_bufferList;
        }
    }
}