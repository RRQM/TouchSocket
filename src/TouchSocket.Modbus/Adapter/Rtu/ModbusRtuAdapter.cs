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

using TouchSocket.Core;

namespace TouchSocket.Modbus;

internal class ModbusRtuAdapter : CustomDataHandlingAdapter<ModbusRtuResponse>
{
    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref ModbusRtuResponse request)
    {
        if (reader.BytesRemaining < 3)
        {
            return FilterResult.Cache;
        }

        var pos = reader.BytesRead;

        var slaveId = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        FunctionCode functionCode;
        var isError = false;
        var code = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        if ((code & 0x80) == 0)
        {
            functionCode = (FunctionCode)code;
        }
        else
        {
            code = code.SetBit(7, false);
            functionCode = (FunctionCode)code;
            isError = true;
        }

        ModbusErrorCode errorCode;
        byte[] data;
        ushort startingAddress;

        int bodyLength;
        if (isError)
        {
            errorCode = (ModbusErrorCode)ReaderExtension.ReadValue<TReader, byte>(ref reader);
            bodyLength = 2;

            if (reader.BytesRemaining < bodyLength)
            {
                reader.BytesRead = pos;
                return FilterResult.Cache;
            }

            var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(pos, reader.BytesRead - pos));
            var crc = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);

            //下面crc验证失败时，不再抛出错误，而是返回错误码。
            //https://gitee.com/RRQM_Home/TouchSocket/issues/IBC1J2

            if (crc == newCrc)
            {
                request = new ModbusRtuResponse()
                {
                    SlaveId = slaveId,
                    ErrorCode = errorCode,
                    FunctionCode = functionCode,
                };

                return FilterResult.Success;
            }
            else
            {
                request = new ModbusRtuResponse()
                {
                    SlaveId = slaveId,
                    ErrorCode = ModbusErrorCode.ResponseMemoryVerificationError,
                    FunctionCode = functionCode,
                };

                return FilterResult.Success;
            }
        }
        else
        {
            if ((byte)functionCode <= 4 || functionCode == FunctionCode.ReadWriteMultipleRegisters)
            {
                bodyLength = ReaderExtension.ReadValue<TReader, byte>(ref reader) + 2;

                if (reader.BytesRemaining < bodyLength)
                {
                    reader.BytesRead = pos;
                    return FilterResult.Cache;
                }

                var len = bodyLength - 2;
                data = reader.GetSpan(len).Slice(0, len).ToArray();
                reader.Advance(len);

                var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(pos, reader.BytesRead - pos));
                var crc = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);

                if (crc == newCrc)
                {
                    request = new ModbusRtuResponse()
                    {
                        SlaveId = slaveId,
                        FunctionCode = functionCode,
                        Data = data,
                    };
                    return FilterResult.Success;
                }
                else
                {
                    request = new ModbusRtuResponse()
                    {
                        SlaveId = slaveId,
                        FunctionCode = functionCode,
                        ErrorCode = ModbusErrorCode.ResponseMemoryVerificationError,
                        Data = data,
                    };
                    return FilterResult.Success;
                }
            }
            else if (functionCode == FunctionCode.WriteSingleCoil || functionCode == FunctionCode.WriteSingleRegister)
            {
                bodyLength = 6;
                if (reader.BytesRemaining < bodyLength)
                {
                    reader.BytesRead = pos;
                    return FilterResult.Cache;
                }
                startingAddress = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);

                var len = bodyLength - 4;
                data = reader.GetSpan(len).Slice(0, len).ToArray();
                reader.Advance(len);
                var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(pos, reader.BytesRead - pos));
                var crc = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);

                if (crc == newCrc)
                {
                    request = new ModbusRtuResponse()
                    {
                        SlaveId = slaveId,
                        FunctionCode = functionCode,
                        StartingAddress = startingAddress,
                        Data = data,
                    };
                    return FilterResult.Success;
                }
                else
                {
                    request = new ModbusRtuResponse()
                    {
                        SlaveId = slaveId,
                        FunctionCode = functionCode,
                        StartingAddress = startingAddress,
                        ErrorCode = ModbusErrorCode.ResponseMemoryVerificationError,
                        Data = data,
                    };
                    return FilterResult.Success;
                }

                //this.m_headerLength = byteBlock.Position - pos;
                //return true;
            }
            else if (functionCode == FunctionCode.WriteMultipleCoils || functionCode == FunctionCode.WriteMultipleRegisters)
            {
                bodyLength = 6;
                if (reader.BytesRemaining < bodyLength)
                {
                    reader.BytesRead = pos;
                    return FilterResult.Cache;
                }
                startingAddress = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                var quantity = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                data = new byte[0];

                var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(pos, reader.BytesRead - pos));
                var crc = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);

                if (crc == newCrc)
                {
                    request = new ModbusRtuResponse()
                    {
                        SlaveId = slaveId,
                        FunctionCode = functionCode,
                        StartingAddress = startingAddress,
                        Quantity = quantity,
                        Data = data,
                    };
                    return FilterResult.Success;
                }
                else
                {
                    request = new ModbusRtuResponse()
                    {
                        SlaveId = slaveId,
                        FunctionCode = functionCode,
                        StartingAddress = startingAddress,
                        Quantity = quantity,
                        ErrorCode = ModbusErrorCode.ResponseMemoryVerificationError,
                        Data = data,
                    };
                    return FilterResult.Success;
                }
            }
            else
            {
                throw new System.Exception("无法识别的功能码");
            }
        }
    }
}