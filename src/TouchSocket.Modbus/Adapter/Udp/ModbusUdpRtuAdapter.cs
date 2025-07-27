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

using System.IO;
using System.Net;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus;

internal class ModbusUdpRtuAdapter : UdpDataHandlingAdapter
{
    public override bool CanSendRequestInfo => true;

    protected override async Task PreviewReceived(EndPoint remoteEndPoint, IByteBlockReader reader)
    {
        var response = new ModbusRtuResponse();
        response.SlaveId = ReaderExtension.ReadValue<IByteBlockReader, byte>(ref reader);
        response.FunctionCode = (FunctionCode)ReaderExtension.ReadValue<IByteBlockReader, byte>(ref reader);

        var crcLen = 0;
        if ((byte)response.FunctionCode <= 4)
        {
            var len = ReaderExtension.ReadValue<IByteBlockReader, byte>(ref reader);
            response.SetValue(ReaderExtension.ReadToSpan(ref reader,len).ToArray());
            response.Crc = ReaderExtension.ReadValue<IByteBlockReader, ushort>(ref reader,EndianType.Big);
            crcLen = 3 + len;
        }
        else if (response.FunctionCode == FunctionCode.WriteSingleCoil || response.FunctionCode == FunctionCode.WriteSingleRegister)
        {
            response.StartingAddress = ReaderExtension.ReadValue<IByteBlockReader, ushort>(ref reader,EndianType.Big);
            response.SetValue(ReaderExtension.ReadToSpan(ref reader,2).ToArray());
            response.Crc = ReaderExtension.ReadValue<IByteBlockReader,ushort>(ref reader,EndianType.Big);
            crcLen = 6;
        }
        else if (response.FunctionCode == FunctionCode.WriteMultipleCoils || response.FunctionCode == FunctionCode.WriteMultipleRegisters)
        {
            response.StartingAddress = ReaderExtension.ReadValue<IByteBlockReader, ushort>(ref reader,EndianType.Big);
            response.Quantity = ReaderExtension.ReadValue<IByteBlockReader,ushort>(ref reader,EndianType.Big);
            response.Crc = ReaderExtension.ReadValue<IByteBlockReader,ushort>(ref reader,EndianType.Big);
            crcLen = 6;
        }

        var crc = TouchSocketModbusUtility.ToModbusCrcValue(reader.Span.Slice(0, crcLen));
        if (crc == (response.Crc))
        {
            await base.GoReceived(remoteEndPoint, null, response).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }
}