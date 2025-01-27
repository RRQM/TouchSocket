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
/// TouchSocketModbusThrowHelper
/// </summary>
public class TouchSocketModbusThrowHelper
{
    /// <summary>
    /// 判断Modbus状态，非成功状态将抛出异常。
    /// </summary>
    /// <param name="errorCode">要判断的Modbus错误代码。</param>
    /// <exception cref="ModbusResponseException">当errorCode非Success时抛出此异常。</exception>
    public static void ThrowIfNotSuccess(ModbusErrorCode errorCode)
    {
        // 根据错误代码判断是否需要抛出异常
        switch (errorCode)
        {
            // 如果错误代码表示成功，则不执行任何操作
            case ModbusErrorCode.Success:
                break;

            // 对于其他任何错误代码，均抛出ModbusResponseException异常
            default:
                throw new ModbusResponseException(errorCode);
        }
    }
}