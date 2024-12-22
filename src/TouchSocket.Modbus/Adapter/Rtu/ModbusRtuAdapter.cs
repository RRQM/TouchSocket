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

namespace TouchSocket.Modbus
{
    internal class ModbusRtuAdapter : CustomDataHandlingAdapter<ModbusRtuResponse>
    {
        protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref ModbusRtuResponse request, ref int tempCapacity)
        {
            if (byteBlock.CanReadLength < 3)
            {
                return FilterResult.Cache;
            }

            var pos = byteBlock.Position;

            var slaveId = byteBlock.ReadByte();
            FunctionCode functionCode;
            var isError = false;
            var code = byteBlock.ReadByte();
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

            var bodyLength = 0;
            byte[] data;
            ushort startingAddress;
            if (isError)
            {
                errorCode = (ModbusErrorCode)byteBlock.ReadByte();
                bodyLength = 2;

                if (byteBlock.CanReadLength < bodyLength)
                {
                    byteBlock.Position = pos;
                    return FilterResult.Cache;
                }

                var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(byteBlock.Span.Slice(pos, byteBlock.Position - pos));
                var crc = byteBlock.ReadUInt16(EndianType.Big);

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
                    bodyLength = byteBlock.ReadByte() + 2;

                    if (byteBlock.CanReadLength < bodyLength)
                    {
                        byteBlock.Position = pos;
                        return FilterResult.Cache;
                    }

                    data = byteBlock.ReadToSpan(bodyLength - 2).ToArray();

                    var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(byteBlock.Span.Slice(pos, byteBlock.Position - pos));
                    var crc = byteBlock.ReadUInt16(EndianType.Big);

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
                    if (byteBlock.CanReadLength < bodyLength)
                    {
                        byteBlock.Position = pos;
                        return FilterResult.Cache;
                    }
                    startingAddress = byteBlock.ReadUInt16(EndianType.Big);
                    data = byteBlock.ReadToSpan(bodyLength - 4).ToArray();
                    var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(byteBlock.Span.Slice(pos, byteBlock.Position - pos));
                    var crc = byteBlock.ReadUInt16(EndianType.Big);

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
                    if (byteBlock.CanReadLength < bodyLength)
                    {
                        byteBlock.Position = pos;
                        return FilterResult.Cache;
                    }
                    startingAddress = byteBlock.ReadUInt16(EndianType.Big);
                    var quantity = byteBlock.ReadUInt16(EndianType.Big);
                    data = new byte[0];

                    var newCrc = TouchSocketModbusUtility.ToModbusCrcValue(byteBlock.Span.Slice(pos, byteBlock.Position - pos));
                    var crc = byteBlock.ReadUInt16(EndianType.Big);

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
}