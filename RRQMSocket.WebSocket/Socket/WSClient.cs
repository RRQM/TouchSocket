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
using RRQMCore;
using RRQMCore.ByteManager;

using RRQMCore.Run;
using RRQMSocket.Http;
using RRQMSocket.WebSocket.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// WebSocket用户终端。
    /// </summary>
    public abstract class WSClient : TcpClient, IWSClient
    {
        private bool isHandshaked;
        private WaitData<Http.HttpResponse> waitData;
        private string webSocketVersion;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WSClient()
        {
            this.waitData = new WaitData<Http.HttpResponse>();
            this.SetAdapter(new Http.HttpDataHandlingAdapter(2048, Http.HttpType.Client));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSetDataHandlingAdapter => false;

        /// <summary>
        /// WebSocket版本号
        /// </summary>
        public string WebSocketVersion
        {
            get { return this.webSocketVersion; }
            set { this.webSocketVersion = value; }
        }

        /// <summary>
        /// 发送Close报文，并关闭TCP。
        /// </summary>
        public override void Close()
        {
            base.Close();
        }

        /// <summary>
        /// 请求连接到WebSocket。
        /// </summary>
        /// <returns></returns>
        public override ITcpClient Connect()
        {
            return this.Connect(default);
        }

        /// <summary>
        /// 请求连接到WebSocket。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual ITcpClient Connect(CancellationToken token)
        {
            lock (this)
            {
                if (this.Online)
                {
                    return this;
                }

                this.ConnectService();
                this.BeginReceive();

                this.waitData.SetCancellationToken(token);
                string base64Key;
                var data = this.GetRequestHeader(out base64Key);
                if (this.UseSsl)
                {
                    this.GetStream().Write(data, 0, data.Length);
                }
                else
                {
                    this.MainSocket.Send(data);
                }

                switch (this.waitData.Wait(1000 * 10))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            Http.HttpResponse httpResponse = this.waitData.WaitResult;
                            if (httpResponse.GetHeader("Sec-WebSocket-Accept") != WSTools.CalculateBase64Key(base64Key, Encoding.UTF8))
                            {
                                this.MainSocket.Dispose();
                                throw new RRQMException("返回的应答码不正确。");
                            }

                            this.SetAdapter(new WebSocketDataHandlingAdapter());
                            this.isHandshaked = true;
                            this.online = true;
                            this.OnConnected(new MesEventArgs());
                            httpResponse.Flag = true;
                            break;
                        }
                    case WaitDataStatus.Overtime:
                        this.MainSocket.Dispose();
                        throw new TimeoutException("连接超时");
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        this.MainSocket.Dispose();
                        return this;
                }
                return this;
            }
        }

        /// <summary>
        /// 发送Ping报文。
        /// </summary>
        public bool Ping()
        {
            try
            {
                this.Send(new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 发送Pong报文。
        /// </summary>
        public bool Pong()
        {
            try
            {
                this.Send(new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override sealed void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.isHandshaked)
            {
                WSDataFrame dataFrame = (WSDataFrame)requestInfo;
                switch (dataFrame.Opcode)
                {
                    case WSDataType.Close:
                        this.Close();
                        break;

                    case WSDataType.Ping:
                        this.OnPing();
                        break;

                    case WSDataType.Pong:
                        this.OnPong();
                        break;

                    case WSDataType.Cont:
                    case WSDataType.Text:
                    case WSDataType.Binary:
                    default:
                        this.HandleWSDataFrame(dataFrame);
                        break;
                }
            }
            else
            {
                HttpResponse httpResponse = (HttpResponse)requestInfo;
                httpResponse.Flag = false;
                this.waitData.Set(httpResponse);
                SpinWait.SpinUntil(() =>
                {
                    return (bool)httpResponse.Flag;
                }, 1000);
            }
        }

        /// <summary>
        /// 处理WebSocket消息。
        /// </summary>
        /// <param name="dataFrame"></param>
        protected abstract void HandleWSDataFrame(WSDataFrame dataFrame);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnBreakOut()
        {
            this.isHandshaked = false;
            base.OnBreakOut();
        }

        /// <summary>
        /// 收到Ping报文。
        /// </summary>
        protected virtual void OnPing()
        {
        }

        /// <summary>
        /// 收到Pong报文。
        /// </summary>
        protected virtual void OnPong()
        {
        }

        private byte[] GetRequestHeader(out string base64Key)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("GET / HTTP/1.1");
            stringBuilder.AppendLine($"Host: {this.Name}");
            stringBuilder.AppendLine("Connection: Upgrade");
            stringBuilder.AppendLine("Pragma: no-cache");
            stringBuilder.AppendLine("User-Agent: RRQMSocket.WebSocket");
            stringBuilder.AppendLine("Upgrade: websocket");
            stringBuilder.AppendLine("Origin: RRQM");
            stringBuilder.AppendLine($"Sec-WebSocket-Version: {this.WebSocketVersion}");
            stringBuilder.AppendLine("Accept-Encoding: deflate, br");
            stringBuilder.AppendLine("Accept-Language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");

            base64Key = WSTools.CreateBase64Key();
            stringBuilder.AppendLine($"Sec-WebSocket-Key: {base64Key}");
            stringBuilder.AppendLine("Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits");
            stringBuilder.AppendLine();
            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        #region 同步发送

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Send(byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = new ByteBlock(length + 1024);
            try
            {
                WSDataFrame dataFrame = new WSDataFrame();
                dataFrame.AppendBinary(buffer, offset, length).BuildRequest(byteBlock);
                base.Send(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="byteBlock"></param>
        public override void Send(ByteBlock byteBlock)
        {
            base.Send(byteBlock);
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="buffer"></param>
        public override void Send(byte[] buffer)
        {
            base.Send(buffer);
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="transferBytes"></param>
        public override void Send(IList<TransferByte> transferBytes)
        {
            base.Send(transferBytes);
        }

        /// <summary>
        /// 发送文本。
        /// </summary>
        /// <param name="text"></param>
        public virtual void Send(string text)
        {
            ByteBlock byteBlock = new ByteBlock(text.Length + 1024);
            try
            {
                WSDataFrame dataFrame = new WSDataFrame();
                dataFrame.AppendText(text).BuildRequest(byteBlock);
                base.Send(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dataFrame"></param>
        public void Send(WSDataFrame dataFrame)
        {
            ByteBlock byteBlock = new ByteBlock(dataFrame.PayloadLength + 1024);
            try
            {
                dataFrame.BuildRequest(byteBlock);
                base.Send(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        #endregion 同步发送

        #region 异步发送

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void SendAsync(byte[] buffer, int offset, int length)
        {
            ByteBlock byteBlock = new ByteBlock(length + 1024);
            try
            {
                WSDataFrame dataFrame = new WSDataFrame();
                dataFrame.AppendBinary(buffer, offset, length).BuildRequest(byteBlock);
                base.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="byteBlock"></param>
        public override void SendAsync(ByteBlock byteBlock)
        {
            base.SendAsync(byteBlock);
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="buffer"></param>
        public override void SendAsync(byte[] buffer)
        {
            base.SendAsync(buffer);
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="transferBytes"></param>
        public override void SendAsync(IList<TransferByte> transferBytes)
        {
            base.SendAsync(transferBytes);
        }

        /// <summary>
        /// 发送文本。
        /// </summary>
        /// <param name="text"></param>
        public virtual void SendAsync(string text)
        {
            ByteBlock byteBlock = new ByteBlock(text.Length + 1024);
            try
            {
                WSDataFrame dataFrame = new WSDataFrame();
                dataFrame.AppendText(text).BuildRequest(byteBlock);
                base.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dataFrame"></param>
        public void SendAsync(WSDataFrame dataFrame)
        {
            ByteBlock byteBlock = new ByteBlock(dataFrame.PayloadLength + 1024);
            try
            {
                dataFrame.BuildRequest(byteBlock);
                base.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        #endregion 异步发送

        #region 同步分包发送

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="packageSize"></param>
        public void SubpackageSend(byte[] buffer, int offset, int length, int packageSize)
        {
            if (packageSize >= length)
            {
                this.Send(buffer, offset, length);
                return;
            }
            int sentLen = 0;
            WSDataFrame dataFrame = new WSDataFrame();
            dataFrame.SetMaskString("RRQM");
            dataFrame.Mask = true;
            dataFrame.FIN = false;
            dataFrame.Opcode = WSDataType.Binary;

            int count;
            if (length % packageSize == 0)
            {
                count = length / packageSize;
            }
            else
            {
                count = length / packageSize + 1;
            }

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    dataFrame.Opcode = WSDataType.Cont;
                }
                if (i == count - 1)//最后
                {
                    dataFrame.FIN = true;
                }
                ByteBlock byteBlock = new ByteBlock(packageSize + 1024);
                try
                {
                    int thisLen = Math.Min(packageSize, length - sentLen);
                    WSTools.Build(byteBlock, dataFrame, buffer, offset, thisLen);
                    base.Send(byteBlock.Buffer, 0, byteBlock.Len);
                    sentLen += thisLen;
                    offset += thisLen;
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packageSize"></param>
        public void SubpackageSend(byte[] buffer, int packageSize)
        {
            this.SubpackageSend(buffer, 0, buffer.Length, packageSize);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="packageSize"></param>
        public void SubpackageSend(ByteBlock byteBlock, int packageSize)
        {
            this.SubpackageSend(byteBlock.Buffer, 0, byteBlock.Len, packageSize);
        }

        #endregion 同步分包发送

        #region 异步分包发送

        /// <summary>
        /// 分包发送。
        /// <para>
        /// 消息分片，它的构成是由起始帧(FIN为0，opcode非0)，然后若干(0个或多个)帧(FIN为0，opcode为0)，然后结束帧(FIN为1，opcode为0)
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="packageSize"></param>
        public void SubpackageSendAsync(byte[] buffer, int offset, int length, int packageSize)
        {
            if (packageSize >= length)
            {
                this.SendAsync(buffer, offset, length);
                return;
            }
            int sentLen = 0;
            WSDataFrame dataFrame = new WSDataFrame();
            dataFrame.SetMaskString("RRQM");
            dataFrame.Mask = true;
            dataFrame.FIN = false;
            dataFrame.Opcode = WSDataType.Binary;

            int count;
            if (length % packageSize == 0)
            {
                count = length / packageSize;
            }
            else
            {
                count = length / packageSize + 1;
            }

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    dataFrame.Opcode = WSDataType.Cont;
                }
                if (i == count - 1)//最后
                {
                    dataFrame.FIN = true;
                }
                ByteBlock byteBlock = new ByteBlock(packageSize + 1024);
                try
                {
                    int thisLen = Math.Min(packageSize, length - sentLen);
                    WSTools.Build(byteBlock, dataFrame, buffer, offset, thisLen);
                    base.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
                    sentLen += thisLen;
                    offset += thisLen;
                }
                finally
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packageSize"></param>
        public void SubpackageSendAsync(byte[] buffer, int packageSize)
        {
            this.SubpackageSendAsync(buffer, 0, buffer.Length, packageSize);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="packageSize"></param>
        public void SubpackageSendAsync(ByteBlock byteBlock, int packageSize)
        {
            this.SubpackageSendAsync(byteBlock.Buffer, 0, byteBlock.Len, packageSize);
        }

        #endregion 异步分包发送
    }
}