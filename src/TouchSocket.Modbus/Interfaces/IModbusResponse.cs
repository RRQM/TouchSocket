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

using System;

namespace TouchSocket.Modbus;

/// <summary>
/// Modbus响应
/// </summary>
public interface IModbusResponse
{
    /// <summary>
    /// 站点号
    /// </summary>
    public byte SlaveId { get; }

    /// <summary>
    /// 数据
    /// </summary>
    ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// 功能码
    /// </summary>
    FunctionCode FunctionCode { get; }

    /// <summary>
    /// 错误码
    /// </summary>
    ModbusErrorCode ErrorCode { get; }

    /// <summary>
    /// 指示响应是否成功
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// 获取和当前响应对应的请求
    /// </summary>
    IModbusRequest Request { get; }
}