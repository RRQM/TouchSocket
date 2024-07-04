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

using System.Net;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    internal class ModbusUdpRtuAdapter : UdpDataHandlingAdapter
    {
        public override bool CanSendRequestInfo => true;

        protected override async Task PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            var response = new ModbusRtuResponse();
            response.SlaveId = byteBlock.ReadByte();
            response.FunctionCode = (FunctionCode)byteBlock.ReadByte();

            var crcLen = 0;
            if ((byte)response.FunctionCode <= 4)
            {
                var len = byteBlock.ReadByte();
                response.SetValue(byteBlock.ReadToSpan(len).ToArray());
                response.Crc = byteBlock.ReadUInt16(EndianType.Big);
                crcLen = 3 + len;
            }
            else if (response.FunctionCode == FunctionCode.WriteSingleCoil || response.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                response.StartingAddress = byteBlock.ReadUInt16(EndianType.Big);
                response.SetValue(byteBlock.ReadToSpan(2).ToArray());
                response.Crc = byteBlock.ReadUInt16(EndianType.Big);
                crcLen = 6;
            }
            else if (response.FunctionCode == FunctionCode.WriteMultipleCoils || response.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                response.StartingAddress = byteBlock.ReadUInt16(EndianType.Big);
                response.Quantity = byteBlock.ReadUInt16(EndianType.Big);
                response.Crc = byteBlock.ReadUInt16(EndianType.Big);
                crcLen = 6;
            }

            var crc = TouchSocketModbusUtility.ToModbusCrcValue(byteBlock.Span.Slice(0, crcLen));
            if (crc == (response.Crc))
            {
                await base.GoReceived(remoteEndPoint, null, response).ConfigureFalseAwait();
            }
        }
    }
}