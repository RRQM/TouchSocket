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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    internal sealed class InternalWebSocket : DisposableObject, IWebSocket
    {
        private readonly IHttpClientBase m_client;
        private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);
        private readonly AsyncAutoResetEvent m_resetEventForRead = new AsyncAutoResetEvent(false);
        private WSDataFrame m_dataFrame;
        private bool m_allowAsyncRead;
        private bool m_isCont;
        public InternalWebSocket(IHttpClientBase client)
        {
            this.m_client = client;
        }
        public bool AllowAsyncRead { get => m_allowAsyncRead; set => m_allowAsyncRead = value; }

        public bool IsHandshaked { get;set; }

        public string Version { get; set; }

        public IHttpClientBase Client => this.m_client;

        public void Close(string msg)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close }.AppendText(msg))
            {
                this.Send(frame);
            }
            this.m_client.TryShutdown();
            this.m_client.SafeClose(msg);
            this.m_allowAsyncRead = false;
        }

        public async Task CloseAsync(string msg)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close }.AppendText(msg))
            {
               await this.SendAsync(frame);
            }
            this.m_client.TryShutdown();
            this.m_client.SafeClose(msg);
            this.m_allowAsyncRead = false;
        }

        public void Ping()
        {
            Send(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping });
        }

        public async Task PingAsync()
        {
           await SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping });
        }

        public void Pong()
        {
            this.Send( new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong });
        }

        public async Task PongAsync()
        {
           await this.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong });
        }

        public async Task<WebSocketReceiveResult> ReadAsync(CancellationToken token)
        {
            if (!this.m_allowAsyncRead)
            {
                return new WebSocketReceiveResult(this.ComplateRead, null);
            }
            await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
            return new WebSocketReceiveResult(this.ComplateRead, this.m_dataFrame);
        }

#if ValueTask
        public async ValueTask<WebSocketReceiveResult> ValueReadAsync(CancellationToken token)
        {
            if (!this.m_allowAsyncRead)
            {
                return new WebSocketReceiveResult(this.ComplateRead, null);
            }
            await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
            return new WebSocketReceiveResult(this.ComplateRead, this.m_dataFrame);
        }
#endif

        #region 发送

        public void Send(WSDataFrame dataFrame, bool endOfMessage = true)
        {
            WSDataType dataType;
            if (this.m_isCont)
            {
                dataType = WSDataType.Cont;
                if (endOfMessage)
                {
                    m_isCont=false;
                }
            }
            else
            {
                dataType = dataFrame.Opcode;
                if (!endOfMessage)
                {
                    m_isCont = true;
                }
            }
            dataFrame.Opcode = dataType;
            using (var byteBlock = new ByteBlock(dataFrame.GetTotalSize()))
            {
                if (m_client.IsClient)
                {
                    dataFrame.BuildRequest(byteBlock);
                }
                else
                {
                    dataFrame.BuildResponse(byteBlock);
                }
                m_client.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        public void Send(string text, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Text }.AppendText(text))
            {
                this.Send(frame, endOfMessage);
            }
        }

        public async Task SendAsync(string text, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Text }.AppendText(text))
            {
                await this.SendAsync(frame, endOfMessage);
            }
        }

        public void Send(byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
            {
                if (offset == 0)
                {
                    frame.PayloadData = new ByteBlock(buffer, length);
                }
                else
                {
                    frame.AppendBinary(buffer, offset, length);
                }
                this.Send(frame, endOfMessage);
            }
        }

        public void Send(ByteBlock byteBlock, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
            {
                frame.PayloadData = byteBlock;
                this.Send(frame, endOfMessage);
            }
        }

        public Task SendAsync(byte[] buffer, bool endOfMessage = true)
        {
            return this.SendAsync(buffer,0,buffer.Length, endOfMessage);
        }

        public void Send(byte[] buffer, bool endOfMessage = true)
        {
            this.Send(buffer,0,buffer.Length, endOfMessage);
        }

        public async Task SendAsync(byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
            {
                if (offset == 0)
                {
                    frame.PayloadData = new ByteBlock(buffer, length);
                }
                else
                {
                    frame.AppendBinary(buffer, offset, length);
                }
                await this.SendAsync(frame, endOfMessage);
            }
        }

        public async Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true)
        {
            WSDataType dataType;
            if (this.m_isCont)
            {
                dataType = WSDataType.Cont;
                if (endOfMessage)
                {
                    m_isCont = false;
                }
            }
            else
            {
                dataType = dataFrame.Opcode;
                if (!endOfMessage)
                {
                    m_isCont = true;
                }
            }
            dataFrame.Opcode = dataType;
            using (var byteBlock = new ByteBlock(dataFrame.GetTotalSize()))
            {
                if (m_client.IsClient)
                {
                    dataFrame.BuildRequest(byteBlock);
                }
                else
                {
                    dataFrame.BuildResponse(byteBlock);
                }
                await m_client.DefaultSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        #endregion 发送

        internal async Task<bool> TryInputReceiveAsync(WSDataFrame dataFrame)
        {
            if (!this.m_allowAsyncRead)
            {
                return false;
            }
            this.m_dataFrame = dataFrame;
            this.m_resetEventForRead.Set();
            if (dataFrame == null)
            {
                return true;
            }
            if (await this.m_resetEventForComplateRead.WaitOneAsync(TimeSpan.FromSeconds(10)).ConfigureFalseAwait())
            {
                return true;
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            //this.m_client.RemoveValue(WebSocketClientExtension.WebSocketProperty);
            this.m_resetEventForComplateRead.SafeDispose();
            this.m_resetEventForRead.SafeDispose();
            this.m_dataFrame = null;
            base.Dispose(disposing);
        }

        private void ComplateRead()
        {
            this.m_dataFrame = default;
            this.m_resetEventForComplateRead.Set();
        }
    }
}