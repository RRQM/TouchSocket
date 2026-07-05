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

internal class ModbusUdpAdapter : ModbusUdpCustomDataHandlingAdapter<ModbusTcpResponse>
{
    private readonly ModbusFunctionHandlerRegistry m_registry;

    internal ModbusUdpAdapter(ModbusFunctionHandlerRegistry registry)
    {
        this.m_registry = registry;
    }

    protected override FilterResult Filter<TReader>(ref TReader reader, ref ModbusTcpResponse request)
    {
        return ModbusTcpResponseParser.Filter(ref reader, ref request, this.m_registry);
    }
}