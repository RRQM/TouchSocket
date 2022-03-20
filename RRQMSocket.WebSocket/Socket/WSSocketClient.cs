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
using RRQMSocket.Http;
using RRQMSocket.WebSocket.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// WebSocket服务器辅助类
    /// </summary>
    public abstract class WSSocketClient : SocketClient, IWSClientBase
    {
        private bool isHandshaked;

        private string webSocketVersion;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WSSocketClient()
        {
            this.SetAdapter(new Http.HttpDataHandlingAdapter(2048, Http.HttpType.Server));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override sealed bool CanSetDataHandlingAdapter => false;

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
            try
            {
                WSDataFrame dataFrame = new WSDataFrame();
                dataFrame.FIN = true;
                dataFrame.Opcode = WSDataType.Close;
                this.Send(dataFrame);
                base.Close();
            }
            catch
            {
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
                try
                {
                    HttpRequest httpRequest = (HttpRequest)requestInfo;
                    if ((httpRequest.GetHeader("Upgrade").ToLower().Trim() == "websocket") && (httpRequest.GetHeader("Connection").ToLower().Trim() == "upgrade") && (!string.IsNullOrEmpty(httpRequest.GetHeader("Sec-WebSocket-Key"))))
                    {
                        this.WebSocketVersion = httpRequest.GetHeader("Sec-WebSocket-Version");
                        ClientOperationEventArgs clientArgs = new ClientOperationEventArgs();

                        if (this.UseSsl)
                        {
                            var data = GetResponse(httpRequest);
                            this.GetStream().Write(data, 0, data.Length);
                        }
                        else
                        {
                            this.MainSocket.Send(GetResponse(httpRequest));
                        }
                        this.online = true;
                        this.isHandshaked = true;
                        this.SetAdapter(new WebSocketDataHandlingAdapter());
                        this.OnConnected(new MesEventArgs("WebSocket连接成功"));
                    }
                    else
                    {
                        this.BreakOut("WebSocket连接协议不正确");
                    }
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message);
                }
            }
        }

        /// <summary>
        /// 处理WebSocket消息。
        /// </summary>
        /// <param name="dataFrame"></param>
        protected abstract void HandleWSDataFrame(WSDataFrame dataFrame);

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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void PreviewConnected(MesEventArgs e)
        {
            this.BeginReceive();
            RRQMCore.Run.EasyAction.DelayRun(10 * 1000, () =>
              {
                  if (!this.isHandshaked)
                  {
                      this.BreakOut("WebSocket验证超时");
                  }
              });
        }

        private static byte[] GetResponse(HttpRequest httpRequest)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
            stringBuilder.AppendLine("Connection: Upgrade");
            stringBuilder.AppendLine("Upgrade: websocket");
            stringBuilder.AppendLine($"Sec-WebSocket-Accept: {WSTools.CalculateBase64Key(httpRequest.GetHeader("Sec-WebSocket-Key"), httpRequest.Encoding)}");
            stringBuilder.AppendLine();
            return httpRequest.Encoding.GetBytes(stringBuilder.ToString());
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
                dataFrame.AppendBinary(buffer, offset, length).BuildResponse(byteBlock);
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
                dataFrame.AppendText(text).BuildResponse(byteBlock);
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
                dataFrame.BuildResponse(byteBlock);
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
                dataFrame.AppendBinary(buffer, offset, length).BuildResponse(byteBlock);
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
                dataFrame.AppendText(text).BuildResponse(byteBlock);
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
                dataFrame.BuildResponse(byteBlock);
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