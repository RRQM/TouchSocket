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
using TouchSocket.Core;

namespace TouchSocket.Rpc;

/// <summary>
/// 立即执行RPC调度器类，用于同步执行RPC调用。
/// </summary>
/// <typeparam name="TRpcActor">RPC行为者类型。</typeparam>
/// <typeparam name="TCallContext">调用上下文类型，必须是<see cref="ICallContext"/>的子类。</typeparam>
public class ImmediateRpcDispatcher<TRpcActor, TCallContext> : DisposableObject, IRpcDispatcher<TRpcActor, TCallContext>
    where TRpcActor : class
    where TCallContext : class, ICallContext
{

    /// <inheritdoc/>
    public bool Reenterable => false;

    /// <inheritdoc/>
    public Task Dispatcher(TRpcActor actor, TCallContext callContext, Func<TCallContext, Task> func)
    {
        // 直接调用传入的函数并传递调用上下文，不进行任何处理或延迟执行。
        return func(callContext);
    }
}