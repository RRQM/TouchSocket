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

namespace TouchSocket.Rpc;


/// <summary>
/// 定义了远程过程调用(RPC)客户端的基本操作。
/// 该接口提供了发起RPC请求的方法。
/// </summary>
public interface IRpcClient
{
    /// <summary>
    /// 异步调用一个操作。
    /// </summary>
    /// <param name="invokeKey">操作的标识键。</param>
    /// <param name="returnType">返回值的类型。</param>
    /// <param name="invokeOption">调用选项，用于指定调用的特定选项。</param>
    /// <param name="parameters">传递给操作的参数。</param>
    /// <returns>一个任务，其结果是操作的返回值。</returns>
    Task<object> InvokeAsync(string invokeKey, Type returnType, InvokeOption invokeOption, params object[] parameters);
}