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

internal static class ModbusTcpResponseParser
{
    public const int HeaderLength = 8;

    public static FilterResult Filter<TReader>(ref TReader reader, ref ModbusTcpResponse request, ModbusFunctionHandlerRegistry registry)
        where TReader : IBytesReader
    {
        if (reader.BytesRemaining < HeaderLength)
        {
            return FilterResult.Cache;
        }

        var pos = reader.BytesRead;
        var header = reader.GetSpan(HeaderLength);
        var bodyLength = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(4)) - 2;
        if (bodyLength < 0)
        {
            reader.Advance(1);
            return FilterResult.GoOn;
        }

        var packageLength = HeaderLength + bodyLength;
        if (reader.BytesRemaining < packageLength)
        {
            reader.BytesRead = pos;
            return FilterResult.Cache;
        }

        var transactionId = TouchSocketBitConverter.BigEndian.To<ushort>(header);
        var protocolId = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(2));
        var slaveId = header[6];
        var code = header[7];
        var isError = (code & 0x80) != 0;
        if (isError)
        {
            code = code.SetBit(7, false);
        }
        var functionCode = (FunctionCode)code;

        reader.Advance(HeaderLength);
        var body = reader.GetSpan(bodyLength);
        var responseMemory = reader.TotalSequence.Slice(pos, packageLength).ToArray();

        if (isError)
        {
            if (bodyLength < 1)
            {
                reader.BytesRead = pos;
                reader.Advance(1);
                return FilterResult.GoOn;
            }

            request = new ModbusTcpResponse(transactionId, protocolId, slaveId, functionCode, (ModbusErrorCode)body[0], responseMemory);
            reader.Advance(bodyLength);
            return FilterResult.Success;
        }

        var handler = registry.GetHandler(functionCode);
        if (handler == null)
        {
            reader.BytesRead = pos;
            reader.Advance(1);
            return FilterResult.GoOn;
        }

        var responseData = handler.ParseResponsePdu(body);
        request = new ModbusTcpResponse(transactionId, protocolId, slaveId, functionCode, ModbusErrorCode.Success, responseMemory)
        {
            Data = responseData.Data,
            StartingAddress = responseData.StartingAddress,
            Quantity = responseData.Quantity
        };
        reader.Advance(bodyLength);
        return FilterResult.Success;
    }
}