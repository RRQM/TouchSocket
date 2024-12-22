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

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 定义了一个接口，用于在RPC（远程过程调用）环境中分发调用请求。
    /// </summary>
    /// <typeparam name="TRpcActor">RPC行为的类型，必须是类类型。</typeparam>
    /// <typeparam name="TCallContext">调用上下文的类型，必须是类类型并且实现ICallContext接口。</typeparam>
    public interface IRpcDispatcher<TRpcActor, TCallContext>
        where TRpcActor : class
        where TCallContext : class, ICallContext
    {

        /// <summary>
        /// 分发并处理RPC调用请求。
        /// </summary>
        /// <param name="actor">具体的RPC行为实例。</param>
        /// <param name="callContext">调用的上下文信息，包含调用相关的元数据。</param>
        /// <param name="func">一个函数委托，表示实际执行的异步操作。</param>
        /// <returns>一个任务，表示异步操作的完成。</returns>
        Task Dispatcher(TRpcActor actor, TCallContext callContext, Func<object, Task> func);

        /// <summary>
        /// 获取一个值，指示是否可重新进入。
        /// </summary>
        bool Reenterable { get; }
    }
}
