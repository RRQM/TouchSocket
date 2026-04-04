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

namespace TouchSocket.Modbus;

internal class ModbusRtuAdapter : CustomDataHandlingAdapter<ModbusRtuResponse>
{
    private readonly ModbusFunctionHandlerRegistry m_registry;

    internal ModbusRtuAdapter(ModbusFunctionHandlerRegistry registry)
    {
        this.m_registry = registry;
    }

    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref ModbusRtuResponse request)
    {
        // 至少需要：SlaveId(1) + FuncCode(1) + 第一个数据字节(1) = 3字节
        if (reader.BytesRemaining < 3)
        {
            return FilterResult.Cache;
        }

        var pos = reader.BytesRead;

        var slaveId = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        var code = ReaderExtension.ReadValue<TReader, byte>(ref reader);

        var isError = (code & 0x80) != 0;
        if (isError)
        {
            code = code.SetBit(7, false);
        }
        var functionCode = (FunctionCode)code;

        if (isError)
        {
            // 错误响应格式：ErrorCode(1) + CRC(2) = 3字节（此时已读了2字节，剩余还需3字节）
            if (reader.BytesRemaining < 3)
            {
                reader.BytesRead = pos;
                return FilterResult.Cache;
            }

            var errorCode = (ModbusErrorCode)ReaderExtension.ReadValue<TReader, byte>(ref reader);
            var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(pos, reader.BytesRead - pos));
            var crc = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);

            request = new ModbusRtuResponse()
            {
                SlaveId = slaveId,
                FunctionCode = functionCode,
                ErrorCode = crc == newCrc ? errorCode : ModbusErrorCode.ResponseMemoryVerificationError,
            };
            return FilterResult.Success;
        }

        var handler = this.m_registry.GetHandler(functionCode)
            ?? throw new NotSupportedException($"不支持的Modbus功能码: {functionCode}");

        var firstByte = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        var afterFirstByteLength = handler.GetRtuResponseAfterFirstByteLength(firstByte);

        // 需要：afterFirstByteLength字节PDU体 + CRC(2)
        if (reader.BytesRemaining < afterFirstByteLength + 2)
        {
            reader.BytesRead = pos;
            return FilterResult.Cache;
        }

        // 合并firstByte与后续数据构建完整PDU体，使用ArrayPool避免大量栈分配
        var pduBodyLength = 1 + afterFirstByteLength;
        var pduBodyBuffer = ArrayPool<byte>.Shared.Rent(pduBodyLength);
        try
        {
            pduBodyBuffer[0] = firstByte;
            reader.GetSpan(afterFirstByteLength).Slice(0, afterFirstByteLength).CopyTo(pduBodyBuffer.AsSpan(1));
            reader.Advance(afterFirstByteLength);

            var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(pos, reader.BytesRead - pos));
            var crc = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);

            var responseData = handler.ParseResponsePdu(pduBodyBuffer.AsSpan(0, pduBodyLength));

            request = new ModbusRtuResponse()
            {
                SlaveId = slaveId,
                FunctionCode = functionCode,
                ErrorCode = crc == newCrc ? ModbusErrorCode.Success : ModbusErrorCode.ResponseMemoryVerificationError,
                Data = responseData.Data,
                StartingAddress = responseData.StartingAddress,
                Quantity = responseData.Quantity,
            };
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pduBodyBuffer);
        }

        return FilterResult.Success;
    }
}