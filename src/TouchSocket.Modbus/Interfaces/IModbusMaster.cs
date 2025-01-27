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

using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Modbus;

/// <summary>
/// 提供Modbus的操作接口
/// </summary>
public interface IModbusMaster
{
    /// <summary>
    /// 异步发送Modbus请求
    /// </summary>
    /// <param name="request">要发送的Modbus请求对象</param>
    /// <param name="millisecondsTimeout">请求的超时时间（以毫秒为单位）</param>
    /// <param name="token">用于取消操作的CancellationToken</param>
    /// <returns>返回一个任务，该任务完成后将包含相应的Modbus响应</returns>
    Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, int millisecondsTimeout, CancellationToken token);
}