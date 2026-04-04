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

/// <summary>
/// 由<see cref="IModbusFunctionHandler"/>解析响应PDU后产生的响应数据结构
/// </summary>
public readonly struct ModbusResponseData
{
    /// <summary>
    /// 初始化仅含响应数据的实例（读操作使用）
    /// </summary>
    /// <param name="data">响应数据</param>
    public ModbusResponseData(ReadOnlyMemory<byte> data)
    {
        this.Data = data;
        this.StartingAddress = 0;
        this.Quantity = 0;
    }

    /// <summary>
    /// 初始化包含起始地址和数量的实例（写操作响应使用）
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="startingAddress">起始地址</param>
    /// <param name="quantity">数量</param>
    public ModbusResponseData(ReadOnlyMemory<byte> data, ushort startingAddress, ushort quantity)
    {
        this.Data = data;
        this.StartingAddress = startingAddress;
        this.Quantity = quantity;
    }

    /// <summary>
    /// 响应数据
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// 起始地址（写操作响应时有效）
    /// </summary>
    public ushort StartingAddress { get; }

    /// <summary>
    /// 数量（写多个操作响应时有效）
    /// </summary>
    public ushort Quantity { get; }
}
