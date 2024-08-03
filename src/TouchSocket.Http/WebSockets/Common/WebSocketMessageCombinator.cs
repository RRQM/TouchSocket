using System;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    public struct WebSocketMessage : IDisposable
    {
        private readonly Action m_disposeAction;

        public WebSocketMessage(WSDataType opcode, ByteBlock payloadData, Action disposeAction)
        {
            this.Opcode = opcode;
            this.PayloadData = payloadData;
            this.m_disposeAction = disposeAction;
        }

        public WSDataType Opcode { get; }
        public ByteBlock PayloadData { get; }

        public void Dispose()
        {
            this.m_disposeAction.Invoke();
        }
    }

    /// <summary>
    /// WebSocket消息合并器。其作用是合并具有中继数据的消息
    /// </summary>
    public sealed class WebSocketMessageCombinator
    {
        private ByteBlock m_byteBlock = default;//中继包缓存
        private bool m_combining;
        private WSDataType m_wSDataType;

        public bool TryCombine(WSDataFrame dataFrame, out WebSocketMessage webSocketMessage)
        {
            var data = dataFrame.PayloadData;

            switch (dataFrame.Opcode)
            {
                case WSDataType.Cont:
                    {
                        //先写入数据
                        this.m_byteBlock.Write(data.Span);

                        //判断中继包
                        if (dataFrame.FIN)//判断是否为最终包
                        {
                            webSocketMessage = new WebSocketMessage(this.m_wSDataType, this.m_byteBlock, this.PrivateClear);
                            return true;
                        }
                        else
                        {
                            webSocketMessage = default;
                            return false;
                        }
                    }
                case WSDataType.Close:
                case WSDataType.Ping:
                case WSDataType.Pong:
                    {
                        webSocketMessage = default;
                        return false;
                    }
                default:
                    {
                        if (dataFrame.FIN)//判断是不是最后的包
                        {
                            //是，则直接输出

                            //如果上次并有中继数据缓存，则说明合并出现了无法处理的情况
                            if (this.m_combining)
                            {
                                ThrowHelper.ThrowInvalidOperationException("上个合并数据没有完成处理");
                            }
                            webSocketMessage = new WebSocketMessage(dataFrame.Opcode, dataFrame.PayloadData, this.PrivateClear);
                            return true;
                        }
                        else
                        {
                            //先保存数据类型
                            this.m_wSDataType = dataFrame.Opcode;
                            this.m_combining = true;

                            //否，则说明数据太大了，分中继包了。
                            //则，初始化缓存容器
                            this.m_byteBlock ??= new ByteBlock(1024 * 64);

                            this.m_byteBlock.Write(data.Span);

                            webSocketMessage = default;
                            return false;
                        }
                    }
            }
        }

        private void PrivateClear()
        {
            var byteBlock = this.m_byteBlock;
            byteBlock?.Dispose();
            this.m_byteBlock = default;
            this.m_combining = false;
            this.m_wSDataType = default;
        }

        /// <summary>
        /// 清空所有缓存状态及数据。
        /// </summary>
        public void Clear()
        {
            this.PrivateClear();
        }
    }
}