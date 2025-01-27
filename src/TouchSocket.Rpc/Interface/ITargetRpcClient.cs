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
using System.Threading.Tasks;

namespace TouchSocket.Rpc;

/// <summary>
/// 定义了远程过程调用(RPC)客户端的行为。
/// 该接口用于在不同的进程或机器之间调用方法，提供了一种统一的方式来执行远程操作并返回结果。
/// </summary>
public interface ITargetRpcClient
{
    /// <summary>
    /// 异步调用远程目标方法。
    /// </summary>
    /// <param name="targetId">目标标识符，用于标识要调用的目标服务或实例。</param>
    /// <param name="invokeKey">调用方法的键，用于确定要执行的具体方法。</param>
    /// <param name="returnType">返回值的类型，用于反序列化调用结果。</param>
    /// <param name="invokeOption">调用选项，可能包含超时、重试等调用策略。</param>
    /// <param name="parameters">调用方法的参数，按顺序传递给远程方法。</param>
    /// <returns>返回一个异步任务，包含调用结果对象。返回类型由returnType参数指定。</returns>
    Task<object> InvokeAsync(string targetId, string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters);
}