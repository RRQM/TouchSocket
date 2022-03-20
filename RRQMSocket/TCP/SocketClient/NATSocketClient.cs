//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Log;
using System;
using System.Net.Sockets;

namespace RRQMSocket
{
    /// <summary>
    /// 端口转发辅助
    /// </summary>
    public class NATSocketClient : SocketClient
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override bool CanSetDataHandlingAdapter => false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public NATSocketClient()
        {
            this.SetAdapter(new NormalDataHandlingAdapter());
        }

        private Socket[] targetSockets;

        internal void BeginRunTargetSocket(NATMode mode, Socket[] sockets)
        {
            this.targetSockets = sockets;

            if (mode == NATMode.OneWay)
            {
                return;
            }
            foreach (var socket in sockets)
            {
                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += this.EventArgs_Completed;
                ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
                eventArgs.UserToken = new NATModel(socket, byteBlock);
                eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                if (!socket.ReceiveAsync(eventArgs))
                {
                    this.ProcessReceived(eventArgs);
                }
            }
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                this.ProcessReceived(e);
            }
            catch (Exception ex)
            {
                this.Close(ex.Message);
            }
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                NATModel model = (NATModel)e.UserToken;
                ByteBlock byteBlock = model.ByteBlock;
                byteBlock.SetLength(e.BytesTransferred);
                this.HandleReceivedDataFromTarget(byteBlock);

                try
                {
                    ByteBlock newByteBlock = BytePool.GetByteBlock(this.BufferLength);
                    model.ByteBlock = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                    if (!model.Socket.ReceiveAsync(e))
                    {
                        this.ProcessReceived(e);
                    }
                }
                catch (Exception ex)
                {
                    this.Close(ex.Message);
                }
            }
            else
            {
                this.Close("远程终端主动断开");
            }
        }

        /// <summary>
        /// 处理从目标服务器接收的数据。
        /// </summary>
        /// <param name="byteBlock"></param>
        protected virtual void HandleReceivedDataFromTarget(ByteBlock byteBlock)
        {
            if (this.disposedValue)
            {
                return;
            }
            try
            {
                this.Send(byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, "在处理数据时发生错误", ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.disposedValue || this.targetSockets == null)
            {
                return;
            }

            foreach (var socket in this.targetSockets)
            {
                try
                {
                    socket.Send(byteBlock.Buffer, 0, byteBlock.Len, SocketFlags.None);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.targetSockets != null)
            {
                foreach (var socket in this.targetSockets)
                {
                    socket.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}