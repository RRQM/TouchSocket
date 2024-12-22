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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 并发RPC调度器，用于管理RPC方法的并发调用。
    /// </summary>
    /// <typeparam name="TRpcActor">RPC行为的类型，必须是IDependencyObject的子类。</typeparam>
    /// <typeparam name="TCallContext">调用上下文的类型，必须是ICallContext的子类。</typeparam>
    public class ConcurrencyRpcDispatcher<TRpcActor, TCallContext> : IRpcDispatcher<TRpcActor, TCallContext>
        where TRpcActor : class
        where TCallContext : class, ICallContext
    {
        /// <inheritdoc/>
        public bool Reenterable => true;

        /// <inheritdoc/>
        public Task Dispatcher(TRpcActor actor, TCallContext callContext, Func<object, Task> func)
        {
            // 获取当前调用上下文中定义的RPC方法信息
            var rpcMethod = callContext.RpcMethod;

            // 如果RPC方法不允许重新进入，则直接执行该方法，避免并发问题
            if (rpcMethod != null && rpcMethod.Reenterable == false)
            {
                return func(callContext);
            }

            // 否则，使用Task.Factory.StartNew在新的线程上执行RPC方法，以支持并发调用
            _ = Task.Factory.StartNew(func, callContext);

            return EasyTask.CompletedTask;
        }
    }
}
