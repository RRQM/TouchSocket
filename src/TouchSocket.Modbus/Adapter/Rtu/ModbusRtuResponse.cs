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
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal class ModbusRtuResponse : ModbusRtuBase, IModbusResponse, IUnfixedHeaderRequestInfo
    {
        private int m_bodyLength;
        private int m_headerLength;
        private bool m_isError;
        int IUnfixedHeaderRequestInfo.BodyLength => m_bodyLength;
        public ModbusErrorCode ErrorCode { get; set; }
        int IUnfixedHeaderRequestInfo.HeaderLength => m_headerLength;

        bool IUnfixedHeaderRequestInfo.OnParsingBody(ReadOnlySpan<byte> body)
        {
            if (this.m_isError)
            {
                return true;
            }

            if ((byte)this.FunctionCode <= 4 || this.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
            {
                this.Data = body.Slice(0, body.Length - 2).ToArray();
                return true;
            }
            else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                this.StartingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(body);
                this.Data = body.Slice(2, body.Length - 4).ToArray();
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

        bool IUnfixedHeaderRequestInfo.OnParsingHeader<TByteBlock>(ref TByteBlock byteBlock)
        {
            if (byteBlock.CanReadLength < 3)
            {
                return false;
            }

            var pos = byteBlock.Position;

            this.SlaveId = byteBlock.ReadByte();

            var code = byteBlock.ReadByte();
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

            if (this.m_isError)
            {
                this.ErrorCode = (ModbusErrorCode)byteBlock.ReadByte();

                this.m_bodyLength = 2;
                this.m_headerLength = byteBlock.Position - pos;
                return true;
            }
            else
            {
                if ((byte)this.FunctionCode <= 4 || this.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
                {
                    this.m_bodyLength = byteBlock.ReadByte() + 2;
                    this.m_headerLength = byteBlock.Position - pos;
                    return true;
                }
                else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
                {
                    //byteBlock.Position--;//回退一个游标
                    this.m_bodyLength = 6;
                    this.m_headerLength = byteBlock.Position - pos;
                    return true;
                }
                else if (this.FunctionCode == FunctionCode.WriteMultipleCoils || this.FunctionCode == FunctionCode.WriteMultipleRegisters)
                {
                    this.m_bodyLength = 6;
                    this.m_headerLength = byteBlock.Position - pos;
                    return true;
                }
            }


            return false;
        }
    }
}