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
/// IRpcServerProvider
/// </summary>
public interface IRpcServerProvider
{
    /// <summary>
    /// 异步执行Rpc
    /// </summary>
    /// <param name="callContext">调用上下文，包含本次调用的相关上下文信息</param>
    /// <param name="invokeResult"></param>
    /// <returns>返回一个任务，结果是InvokeResult类型，包含Rpc调用的结果信息</returns>
    Task<InvokeResult> ExecuteAsync(ICallContext callContext, InvokeResult invokeResult);

    /// <summary>
    /// 获取所有Method
    /// </summary>
    /// <returns></returns>
    RpcMethod[] GetMethods();
}