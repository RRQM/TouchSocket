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

using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    internal sealed partial class InternalWebSocket : IWebSocket
    {
        private readonly HttpClientBase m_httpClientBase;
        private readonly HttpSessionClient m_httpSocketClient;
        private readonly bool m_isServer;
        private bool m_allowAsyncRead;
        private bool m_isCont;

        public InternalWebSocket(HttpClientBase httpClientBase)
        {
            this.m_isServer = false;
            this.m_httpClientBase = httpClientBase;
            m_receiverResult = new WebSocketReceiveResult(this.ComplateRead);
        }

        public InternalWebSocket(HttpSessionClient httpSocketClient)
        {
            this.m_isServer = true;
            this.m_httpSocketClient = httpSocketClient;
            m_receiverResult = new WebSocketReceiveResult(this.ComplateRead);
        }

        public bool AllowAsyncRead { get => this.m_allowAsyncRead; set => this.m_allowAsyncRead = value; }

        public IHttpSession Client => this.m_isServer ? this.m_httpSocketClient : this.m_httpClientBase;
        public bool Online { get; set; }

        public string Version { get; set; }

        public async Task CloseAsync(string msg)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close }.AppendText(msg))
            {
                await this.SendAsync(frame).ConfigureFalseAwait();
            }
            this.m_httpClientBase.TryShutdown();
            await this.m_httpClientBase.SafeCloseAsync(msg).ConfigureFalseAwait();

            this.m_httpSocketClient.TryShutdown();
            await this.m_httpSocketClient.SafeCloseAsync(msg).ConfigureFalseAwait();
            this.m_allowAsyncRead = false;
        }

        //public void Ping()
        //{
        //    this.Send(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping });
        //}

        public async Task PingAsync()
        {
            await this.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping }).ConfigureFalseAwait();
        }

        //public void Pong()
        //{
        //    this.Send(new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong });
        //}

        public async Task PongAsync()
        {
            await this.SendAsync(new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong }).ConfigureFalseAwait();
        }

        #region 发送

        //public void Send(WSDataFrame dataFrame, bool endOfMessage = true)
        //{
        //    WSDataType dataType;
        //    if (this.m_isCont)
        //    {
        //        dataType = WSDataType.Cont;
        //        if (endOfMessage)
        //        {
        //            this.m_isCont = false;
        //        }
        //    }
        //    else
        //    {
        //        dataType = dataFrame.Opcode;
        //        if (!endOfMessage)
        //        {
        //            this.m_isCont = true;
        //        }
        //    }
        //    dataFrame.Opcode = dataType;
        //    using (var byteBlock = new ByteBlock(dataFrame.GetTotalSize()))
        //    {
        //        if (this.m_isServer)
        //        {
        //            dataFrame.BuildResponse(byteBlock);
        //            this.m_httpSocketClient.InternalSend(byteBlock.Buffer, 0, byteBlock.Len);
        //        }
        //        else
        //        {
        //            dataFrame.BuildRequest(byteBlock);
        //            this.m_httpClientBase.InternalSend(byteBlock.Buffer, 0, byteBlock.Len);
        //        }
        //    }
        //}

        //public void Send(string text, bool endOfMessage = true)
        //{
        //    using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Text }.AppendText(text))
        //    {
        //        this.Send(frame, endOfMessage);
        //    }
        //}

        //public void Send(byte[] buffer, int offset, int length, bool endOfMessage = true)
        //{
        //    using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
        //    {
        //        if (offset == 0)
        //        {
        //            frame.PayloadData = new ByteBlock(buffer, length);
        //        }
        //        else
        //        {
        //            frame.AppendBinary(buffer, offset, length);
        //        }
        //        this.Send(frame, endOfMessage);
        //    }
        //}

        //public void Send(ByteBlock byteBlock, bool endOfMessage = true)
        //{
        //    using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
        //    {
        //        frame.PayloadData = byteBlock;
        //        this.Send(frame, endOfMessage);
        //    }
        //}

        //public void Send(byte[] buffer, bool endOfMessage = true)
        //{
        //    this.Send(buffer, 0, buffer.Length, endOfMessage);
        //}

        public async Task SendAsync(string text, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Text }.AppendText(text))
            {
                await this.SendAsync(frame, endOfMessage).ConfigureFalseAwait();
            }
        }

        public Task SendAsync(byte[] buffer, bool endOfMessage = true)
        {
            return this.SendAsync(buffer, 0, buffer.Length, endOfMessage);
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
                await this.SendAsync(frame, endOfMessage).ConfigureFalseAwait();
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
                    this.m_isCont = false;
                }
            }
            else
            {
                dataType = dataFrame.Opcode;
                if (!endOfMessage)
                {
                    this.m_isCont = true;
                }
            }
            dataFrame.Opcode = dataType;
            using (var byteBlock = new ByteBlock(dataFrame.GetTotalSize()))
            {
                if (this.m_isServer)
                {
                    dataFrame.BuildResponse(byteBlock);
                    await this.m_httpSocketClient.InternalSendAsync(byteBlock.Memory).ConfigureFalseAwait();
                }
                else
                {
                    dataFrame.BuildRequest(byteBlock);
                    await this.m_httpClientBase.InternalSendAsync(byteBlock.Memory).ConfigureFalseAwait();
                }
            }
        }

        #endregion 发送

        protected override void Dispose(bool disposing)
        {
            this.m_resetEventForComplateRead.Set();
            this.m_resetEventForComplateRead.SafeDispose();
            base.Dispose(disposing);
        }
    }
}