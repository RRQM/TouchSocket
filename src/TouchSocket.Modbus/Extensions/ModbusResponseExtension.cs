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

/// <summary>
/// ModbusResponseExtension
/// </summary>
public static class ModbusResponseExtension
{
    /// <summary>
    /// 获取一个读取器。
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns></returns>
    public static BytesReader CreateReader(this IModbusResponse response)
    {
        // 使用response的数据创建并返回一个新的ValueByteBlock对象
        return new BytesReader(response.Data.Span);
    }
}