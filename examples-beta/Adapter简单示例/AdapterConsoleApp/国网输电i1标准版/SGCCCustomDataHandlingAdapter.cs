using TouchSocket.Core;
using TouchSocket.Sockets;

namespace AdapterConsoleApp
{
    /// <summary>
    /// 国网输电i1标准版
    /// </summary>
    internal class SGCCCustomDataHandlingAdapter : CustomFixedHeaderDataHandlingAdapter<SGCCRequestInfo>
    {
        public override int HeaderLength => 30;

        public override bool CanSendRequestInfo => false;

        protected override SGCCRequestInfo GetInstance()
        {
            return new SGCCRequestInfo();
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new System.NotImplementedException();
        }
    }

    public class SGCCRequestInfo : IFixedHeaderRequestInfo
    {
        private byte[] m_sync;
        private byte[] m_cMDID;
        private byte[] m_sample;
        private byte[] m_cRC16;

        public int BodyLength { get; private set; }

        /// <summary>
        /// 报文头:5AA5
        /// </summary>
        public byte[] Sync { get => this.m_sync; set => this.m_sync = value; }

        /// <summary>
        /// 报文长度
        /// </summary>
        public ushort PacketLength { get => (ushort)(this.BodyLength - 3); }

        /// <summary>
        /// 状态监测装置ID(17位编码)
        /// </summary>
        public byte[] CMDID { get => this.m_cMDID; set => this.m_cMDID = value; }

        /// <summary>
        /// 帧类型—参考附表C8-1相关含义
        /// </summary>
        public byte FrameType { get; set; }

        /// <summary>
        /// 报文类型—参考附表C8-2相关含义
        /// </summary>
        public byte PacketType { get; set; }

        /// <summary>
        /// 帧序列号（无符号整数)
        /// </summary>
        public byte FrameNo { get; set; }

        /// <summary>
        /// 通道号—表示采集装置上的摄像机编号。如:一个装连接⒉部摄像机，则分别标号为1、2
        /// </summary>
        public byte ChannelNo { get; set; }

        /// <summary>
        /// 预置位号—即云台摄像所设置的预置位号,不带云台摄像机，预置位号为255
        /// </summary>
        public byte PresettingNo { get; set; }

        /// <summary>
        /// 总包数（无符号整数，取值范围:大于等于0)
        /// </summary>
        public ushort PacketNo { get; set; }

        /// <summary>
        /// 子包包号（无符号整数，取值范围:大于等于0>
        /// </summary>
        public ushort SubpacketNo { get; set; }

        /// <summary>
        /// 数据区
        /// </summary>
        public byte[] Sample { get => this.m_sample; set => this.m_sample = value; }

        /// <summary>
        /// 校验位
        /// </summary>
        public byte[] CRC16 { get => this.m_cRC16; }

        /// <summary>
        /// 报文尾:0x96
        /// </summary>
        public byte End { get; set; }

        public bool OnParsingHeader(byte[] header)
        {
            if (header.Length == 30)
            {
                using (var byteBlock = new ByteBlock(header))
                {
                    byteBlock.Pos = 0;
                    byteBlock.Read(out this.m_sync, 2);

                    byteBlock.Read(out var lenBuffer, 2);

                    this.BodyLength = TouchSocketBitConverter.LittleEndian.ToUInt16(lenBuffer, 0) + 3 - 6;//先把crc校验和end都获取。
                    byteBlock.Read(out this.m_cMDID, 17);
                    this.FrameType = (byte)byteBlock.ReadByte();
                    this.PacketType = (byte)byteBlock.ReadByte();
                    this.FrameNo = (byte)byteBlock.ReadByte();
                    this.ChannelNo = (byte)byteBlock.ReadByte();
                    this.PresettingNo = (byte)byteBlock.ReadByte();
                    this.PacketNo = byteBlock.ReadUInt16();
                    this.SubpacketNo = byteBlock.ReadUInt16();

                    return true;
                }
            }
            return false;
        }

        public bool OnParsingBody(byte[] body)
        {
            if (body.Length == this.BodyLength && body[^1] == 150)
            {
                using (var byteBlock = new ByteBlock(body))
                {
                    byteBlock.Read(out this.m_sample, this.BodyLength - 3);
                    byteBlock.Read(out this.m_cRC16, 2);
                    this.End = (byte)byteBlock.ReadByte();
                }
                return true;
            }
            return false;
        }
    }
}