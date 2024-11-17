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
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal sealed class ModbusTcpResponse : ModbusTcpBase, IFixedHeaderRequestInfo, IWaitHandle, IModbusResponse
    {
        private int m_bodyLength;
        private bool m_isError;
        int IFixedHeaderRequestInfo.BodyLength => this.m_bodyLength;

        public ModbusErrorCode ErrorCode { get; private set; }

        int IWaitHandle.Sign { get => this.TransactionId; set => this.TransactionId = (ushort)value; }

        bool IFixedHeaderRequestInfo.OnParsingBody(ReadOnlySpan<byte> body)
        {
            if (this.m_isError)
            {
                this.ErrorCode = (ModbusErrorCode)body[0];
                return true;
            }
            if ((byte)this.FunctionCode <= 4 || this.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
            {
                var len = body[0];

                if (body.Length - 1 == len)
                {
                    this.Data = body.Slice(1).ToArray();
                    return true;
                }
            }
            else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                this.StartingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(body);
                this.Data = body.Slice(2).ToArray();
                return true;
            }
            else if (this.FunctionCode == FunctionCode.WriteMultipleCoils || this.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                this.StartingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(body);
                this.Quantity = TouchSocketBitConverter.BigEndian.To<ushort>(body.Slice(2));
                this.Data = new byte[0];
                return true;
            }
            return false;
        }

        bool IFixedHeaderRequestInfo.OnParsingHeader(ReadOnlySpan<byte> header)
        {
            if (header.Length == 8)
            {
                this.TransactionId = TouchSocketBitConverter.BigEndian.To<ushort>(header);
                this.ProtocolId = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(2));
                this.m_bodyLength = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(4)) - 2;
                this.SlaveId = header[6];

                var code = header[7];
                if ((code & 0x80) == 0)
                {
                    this.FunctionCode = (FunctionCode)code;
                }
                else
                {
                    code = code.SetBit(7, false);
                    this.FunctionCode = (FunctionCode)code;
                    this.m_isError = true;
                }
                return true;
            }

            return false;
        }
    }
}