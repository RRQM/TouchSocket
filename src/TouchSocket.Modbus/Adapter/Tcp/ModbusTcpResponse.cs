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

namespace TouchSocket.Modbus;

internal sealed class ModbusTcpResponse : ModbusTcpBase, IRequestInfo, IWaitHandle, IModbusResponse
{
    internal ModbusTcpResponse(ushort transactionId, ushort protocolId, byte slaveId, FunctionCode functionCode, ModbusErrorCode errorCode, ReadOnlyMemory<byte> responseMemory)
    {
        this.TransactionId = transactionId;
        this.ProtocolId = protocolId;
        this.SlaveId = slaveId;
        this.FunctionCode = functionCode;
        this.ErrorCode = errorCode;
        this.ResponseMemory = responseMemory;
    }

    public ModbusErrorCode ErrorCode { get; }

    public ReadOnlyMemory<byte> ResponseMemory { get; }

    int IWaitHandle.Sign { get => this.TransactionId; set => this.TransactionId = (ushort)value; }

    public IModbusRequest Request { get; set; }

    public bool IsSuccess => this.ErrorCode == ModbusErrorCode.Success;
}