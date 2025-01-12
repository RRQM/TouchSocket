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

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocket适配器
    /// </summary>
    public sealed class WebSocketDataHandlingAdapter : SingleStreamDataHandlingAdapter
    {
        private WSDataFrame m_dataFrameTemp;

        /// <summary>
        /// 数据包剩余长度
        /// </summary>
        private int m_surPlusLength = 0;

        /// <summary>
        /// 临时包
        /// </summary>
        private ByteBlock m_tempByteBlock;

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="dataFrame"></param>
        /// <returns></returns>
        public FilterResult DecodingFromBytes(byte[] dataBuffer, ref int offset, int length, out WSDataFrame dataFrame)
        {
            var index = offset;
            dataFrame = new WSDataFrame
            {
                RSV1 = dataBuffer[offset].GetBit(6),
                RSV2 = dataBuffer[offset].GetBit(5),
                RSV3 = dataBuffer[offset].GetBit(4),
                FIN = (dataBuffer[offset] >> 7) == 1,
                Opcode = (WSDataType)(dataBuffer[offset] & 0xf),
                Mask = (dataBuffer[++offset] >> 7) == 1
            };

            var payloadLength = dataBuffer[offset] & 0x7f;
            if (payloadLength < 126)
            {
                offset++;
            }
            else if (payloadLength == 126)
            {
                if (length < 4)
                {
                    offset = index;
                    return FilterResult.Cache;
                }
                payloadLength = TouchSocketBitConverter.BigEndian.ToUInt16(dataBuffer, ++offset);
                offset += 2;
            }
            else if (payloadLength == 127)
            {
                if (length < 12)
                {
                    this.m_tempByteBlock ??= new ByteBlock();
                    this.m_tempByteBlock.Write(new System.ReadOnlySpan<byte>(dataBuffer, index, length));
                    offset = index;
                    return FilterResult.GoOn;
                }
                payloadLength = (int)TouchSocketBitConverter.BigEndian.ToUInt64(dataBuffer, ++offset);
                offset += 8;
            }

            dataFrame.PayloadLength = payloadLength;

            if (dataFrame.Mask)
            {
                if (length < (offset - index) + 4)
                {
                    this.m_tempByteBlock ??= new ByteBlock();
                    this.m_tempByteBlock.Write(new System.ReadOnlySpan<byte>(dataBuffer, index, length));
                    offset = index;
                    return FilterResult.GoOn;
                }
                dataFrame.MaskingKey = new byte[4];
                dataFrame.MaskingKey[0] = dataBuffer[offset++];
                dataFrame.MaskingKey[1] = dataBuffer[offset++];
                dataFrame.MaskingKey[2] = dataBuffer[offset++];
                dataFrame.MaskingKey[3] = dataBuffer[offset++];
            }

            var byteBlock = new ByteBlock(payloadLength);
            dataFrame.PayloadData = byteBlock;

            var surlen = length - (offset - index);
            if (payloadLength <= surlen)
            {
                byteBlock.Write(new System.ReadOnlySpan<byte>(dataBuffer, offset, payloadLength));
                offset += payloadLength;
            }
            else
            {
                byteBlock.Write(new System.ReadOnlySpan<byte>(dataBuffer, offset, surlen));
                offset += surlen;
            }

            return FilterResult.Success;
        }

        /// <summary>
        /// 当接收到数据时处理数据
        /// </summary>
        /// <param name="byteBlock">数据流</param>
        protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
        {
            var buffer = byteBlock.Memory.GetArray().Array;
            var r = byteBlock.Length;

            if (this.m_tempByteBlock != null)
            {
                this.m_tempByteBlock.Write(new System.ReadOnlySpan<byte>(buffer, 0, r));
                buffer = this.m_tempByteBlock.ToArray();
                r = this.m_tempByteBlock.Position;
                this.m_tempByteBlock.Dispose();
                this.m_tempByteBlock = null;
            }

            if (this.m_dataFrameTemp == null)
            {
                await this.SplitPackageAsync(buffer, 0, r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            else
            {
                if (this.m_surPlusLength == r)
                {
                    this.m_dataFrameTemp.PayloadData.Write(new System.ReadOnlySpan<byte>(buffer, 0, this.m_surPlusLength));
                    await this.PreviewHandle(this.m_dataFrameTemp).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    this.m_dataFrameTemp = null;
                    this.m_surPlusLength = 0;
                }
                else if (this.m_surPlusLength < r)
                {
                    this.m_dataFrameTemp.PayloadData.Write(new System.ReadOnlySpan<byte>(buffer, 0, this.m_surPlusLength));
                    await this.PreviewHandle(this.m_dataFrameTemp).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    this.m_dataFrameTemp = null;
                    await this.SplitPackageAsync(buffer, this.m_surPlusLength, r).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                else
                {
                    this.m_dataFrameTemp.PayloadData.Write(new System.ReadOnlySpan<byte>(buffer, 0, r));
                    this.m_surPlusLength -= r;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            this.m_tempByteBlock.SafeDispose();
            this.m_tempByteBlock = null;
            this.m_dataFrameTemp = null;
            this.m_surPlusLength = 0;
            base.Reset();
        }

        private async Task PreviewHandle(WSDataFrame dataFrame)
        {
            try
            {
                if (dataFrame.Mask)
                {
                    WSTools.DoMask(dataFrame.PayloadData.TotalMemory.Span, dataFrame.PayloadData.Memory.Span, dataFrame.MaskingKey);
                }
                await this.GoReceivedAsync(null, dataFrame).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                dataFrame.Dispose();
            }
        }

        /// <summary>
        /// 分解包
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        private async Task SplitPackageAsync(byte[] dataBuffer, int offset, int length)
        {
            while (offset < length)
            {
                if (length - offset < 2)
                {
                    this.m_tempByteBlock ??= new ByteBlock();
                    this.m_tempByteBlock.Write(new System.ReadOnlySpan<byte>(dataBuffer, offset, length - offset));
                    return;
                }

                switch (this.DecodingFromBytes(dataBuffer, ref offset, length - offset, out var dataFrame))
                {
                    case FilterResult.Cache:
                        {
                            this.m_tempByteBlock ??= new ByteBlock();
                            this.m_tempByteBlock.Write(new System.ReadOnlySpan<byte>(dataBuffer, offset, length - offset));
                            return;
                        }
                    case FilterResult.Success:
                        {
                            if (dataFrame.PayloadLength == dataFrame.PayloadData.Length)
                            {
                                await this.PreviewHandle(dataFrame).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            }
                            else
                            {
                                this.m_surPlusLength = dataFrame.PayloadLength - dataFrame.PayloadData.Length;
                                this.m_dataFrameTemp = dataFrame;
                            }
                        }
                        break;

                    case FilterResult.GoOn:
                    default:
                        return;
                }
            }
        }
    }
}