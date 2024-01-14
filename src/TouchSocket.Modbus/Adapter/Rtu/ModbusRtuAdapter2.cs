//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal class ModbusRtuAdapter2 : PeriodPackageAdapter
    {
        public ModbusRtuAdapter2()
        {
            this.CacheTimeout = TimeSpan.FromMilliseconds(100);
        }

        public override bool CanSendRequestInfo => true;

        protected override void GoReceived(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var response = new ModbusRtuResponse();
            response.SlaveId = byteBlock[0];

            var m_isError = false;
            var code = byteBlock[1];
            if ((code & 0x80) == 0)
            {
                response.FunctionCode = (FunctionCode)code;
            }
            else
            {
                code = code.SetBit(7, 0);
                response.FunctionCode = (FunctionCode)code;
                m_isError = true;
            }

            var crcLen = 0;
            if (m_isError)
            {
                response.ErrorCode = ((ModbusErrorCode)byteBlock[2]);
                response.Crc = byteBlock.Skip(3).Take(2).ToArray();
                crcLen = 3;
            }
            else
            {
                if ((byte)response.FunctionCode <= 4 || response.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
                {
                    var len = byteBlock[2];
                    response.SetValue(byteBlock.Skip(3).Take(len).ToArray());
                    response.Crc = byteBlock.Skip(3 + len).Take(2).ToArray();
                    crcLen = 3 + len;
                }
                else if (response.FunctionCode == FunctionCode.WriteSingleCoil || response.FunctionCode == FunctionCode.WriteSingleRegister)
                {
                    response.StartingAddress = TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 2);
                    response.SetValue(byteBlock.Skip(4).Take(2).ToArray());
                    response.Crc = byteBlock.Skip(6).Take(2).ToArray();
                    crcLen = 6;
                }
                else if (response.FunctionCode == FunctionCode.WriteMultipleCoils || response.FunctionCode == FunctionCode.WriteMultipleRegisters)
                {
                    response.StartingAddress = TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 2);
                    response.Quantity = TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 4);
                    response.Crc = byteBlock.Skip(6).Take(2).ToArray();
                    crcLen = 6;
                }
            }

            var crc = TouchSocketModbusUtility.ToModbusCrc(byteBlock.Buffer, 0, crcLen);
            if (crc.SequenceEqual(response.Crc))
            {
                base.GoReceived(null, response);
            }
        }
    }
}