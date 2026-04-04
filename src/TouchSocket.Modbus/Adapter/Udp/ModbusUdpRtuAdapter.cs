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

using System.Buffers;
using System.Net;

namespace TouchSocket.Modbus;

internal class ModbusUdpRtuAdapter : UdpDataHandlingAdapter
{
    private readonly ModbusFunctionHandlerRegistry m_registry;

    internal ModbusUdpRtuAdapter(ModbusFunctionHandlerRegistry registry)
    {
        this.m_registry = registry;
    }

    public override bool CanSendRequestInfo => true;

    protected override async Task PreviewReceivedAsync(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory)
    {
        if (memory.Length < 4)
        {
            return;
        }

        var reader = new BytesReader(memory);
        var slaveId = ReaderExtension.ReadValue<BytesReader, byte>(ref reader);
        var code = ReaderExtension.ReadValue<BytesReader, byte>(ref reader);

        var isError = (code & 0x80) != 0;
        if (isError)
        {
            code = code.SetBit(7, false);
        }
        var functionCode = (FunctionCode)code;

        if (isError)
        {
            if (reader.BytesRemaining < 3)
            {
                return;
            }
            var errorCode = (ModbusErrorCode)ReaderExtension.ReadValue<BytesReader, byte>(ref reader);
            var crcDataLength = (int)reader.BytesRead;
            var crc = ReaderExtension.ReadValue<BytesReader, ushort>(ref reader, EndianType.Big);
            var expectedCrc = TouchSocketModbusUtility.ToModbusCrcValue(memory.Span.Slice(0, crcDataLength));
            if (crc == expectedCrc)
            {
                var response = new ModbusRtuResponse()
                {
                    SlaveId = slaveId,
                    FunctionCode = functionCode,
                    ErrorCode = errorCode,
                };
                await base.GoReceived(remoteEndPoint, null, response).ConfigureDefaultAwait();
            }
            return;
        }

        var handler = this.m_registry.GetHandler(functionCode);
        if (handler == null)
        {
            return;
        }

        if (reader.BytesRemaining < 1)
        {
            return;
        }

        var firstByte = ReaderExtension.ReadValue<BytesReader, byte>(ref reader);
        var afterFirstByteLength = handler.GetRtuResponseAfterFirstByteLength(firstByte);

        if (reader.BytesRemaining < afterFirstByteLength + 2)
        {
            return;
        }

        var pduBodyLength = 1 + afterFirstByteLength;
        var pduBodyBuffer = ArrayPool<byte>.Shared.Rent(pduBodyLength);
        try
        {
            pduBodyBuffer[0] = firstByte;
            ReaderExtension.ReadToSpan(ref reader, afterFirstByteLength).CopyTo(pduBodyBuffer.AsSpan(1));

            var crcDataLength = (int)reader.BytesRead;
            var crc = ReaderExtension.ReadValue<BytesReader, ushort>(ref reader, EndianType.Big);
            var expectedCrc = TouchSocketModbusUtility.ToModbusCrcValue(memory.Span.Slice(0, crcDataLength));

            if (crc == expectedCrc)
            {
                var responseData = handler.ParseResponsePdu(pduBodyBuffer.AsSpan(0, pduBodyLength));
                var response = new ModbusRtuResponse()
                {
                    SlaveId = slaveId,
                    FunctionCode = functionCode,
                    Data = responseData.Data,
                    StartingAddress = responseData.StartingAddress,
                    Quantity = responseData.Quantity,
                };
                await base.GoReceived(remoteEndPoint, null, response).ConfigureDefaultAwait();
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pduBodyBuffer);
        }
    }
}