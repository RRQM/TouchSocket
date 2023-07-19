using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 基于Dmtp协议的消息。
    /// |*2*|**4**|***************n***********|
    /// <para></para>
    /// |ProtocolFlags|Length|Data|
    /// </summary>
    public class DmtpMessage : DisposableObject, IFixedHeaderByteBlockRequestInfo
    {
        private int m_bodyLength;

        /// <summary>
        /// 基于Dmtp协议的消息。
        /// |*2*|**4**|***************n***********|
        /// <para></para>
        /// |ProtocolFlags|Length|Data|
        /// </summary>
        public DmtpMessage()
        {
        }

        /// <summary>
        /// 基于Dmtp协议的消息。
        /// |*2*|**4**|***************n***********|
        /// <para></para>
        /// |ProtocolFlags|Length|Data|
        /// </summary>
        /// <param name="protocolFlags"></param>
        public DmtpMessage(ushort protocolFlags)
        {
            this.ProtocolFlags = protocolFlags;
        }

        /// <summary>
        /// 实际使用的Body数据。
        /// </summary>
        public ByteBlock BodyByteBlock { get; set; }

        int IFixedHeaderByteBlockRequestInfo.BodyLength => this.m_bodyLength;

        /// <summary>
        /// 协议标识
        /// </summary>
        public ushort ProtocolFlags { get; set; }

        /// <summary>
        /// 构建数据到<see cref="ByteBlock"/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Build(ByteBlock byteBlock)
        {
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(this.ProtocolFlags));
            if (this.BodyByteBlock == null)
            {
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(0));
            }
            else
            {
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(this.BodyByteBlock.Len));
                byteBlock.Write(this.BodyByteBlock);
            }
        }

        /// <summary>
        /// 构建数据到<see langword="byte[]" />
        /// </summary>
        /// <returns></returns>
        public byte[] BuildAsBytes()
        {
            using (var byteBlock = new ByteBlock(this.GetLength()))
            {
                this.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }

        /// <summary>
        /// 将<see cref="BodyByteBlock"/>的有效数据转换为utf-8的字符串。
        /// </summary>
        /// <returns></returns>
        public string GetBodyString()
        {
            if (this.BodyByteBlock == null || this.BodyByteBlock.Len == 0)
            {
                return default;
            }
            else
            {
                return Encoding.UTF8.GetString(this.BodyByteBlock.Buffer, 0, this.BodyByteBlock.Len);
            }
        }

        /// <summary>
        /// 获取整个<see cref="DmtpMessage"/>的数据长度。
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return this.BodyByteBlock == null ? 6 : this.BodyByteBlock.Len + 6;
        }

        bool IFixedHeaderByteBlockRequestInfo.OnParsingBody(ByteBlock byteBlock)
        {
            if (byteBlock.Len == this.m_bodyLength)
            {
                this.BodyByteBlock = byteBlock;
                return true;
            }

            return false;
        }

        bool IFixedHeaderByteBlockRequestInfo.OnParsingHeader(byte[] header)
        {
            if (header.Length == 6)
            {
                this.ProtocolFlags = TouchSocketBitConverter.BigEndian.ToUInt16(header, 0);
                this.m_bodyLength = TouchSocketBitConverter.BigEndian.ToInt32(header, 2);
                return true;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (disposing)
            {
                this.BodyByteBlock.SafeDispose();
            }

            base.Dispose(disposing);
        }
    }
}