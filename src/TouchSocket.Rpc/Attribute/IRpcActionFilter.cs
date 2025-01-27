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
/// Rpc行为过滤器。
/// </summary>
public interface IRpcActionFilter
{
    /// <summary>
    /// 互斥访问类型。
    /// <para>
    /// 当互斥访问类型或其派生类和本类型同时添加特性时，只有优先级更高的会生效。
    /// </para>
    /// </summary>
    Type[] MutexAccessTypes { get; }

    /// <summary>
    /// 执行Rpc之后。
    /// <para>如果修改<paramref name="invokeResult"/>的InvokeStatus，或Result。则会影响Rpc最终结果</para>
    /// </summary>
    /// <param name="callContext">调用上下文，包含有关Rpc调用的上下文信息</param>
    /// <param name="parameters">Rpc调用的参数</param>
    /// <param name="invokeResult">Rpc调用的结果，可以通过此参数修改Rpc的最终结果</param>
    /// <param name="exception">在Rpc调用期间发生的任何异常</param>
    /// <returns>返回一个<see cref="Task{InvokeResult}"/>，该任务表示Rpc调用的最终结果</returns>
    Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception);

    /// <summary>
    /// 在执行Rpc之前。
    /// <para>当<paramref name="invokeResult"/>的InvokeStatus不为<see cref="InvokeStatus.Ready"/>。则不会执行Rpc</para>
    /// <para>同时，当<paramref name="invokeResult"/>的InvokeStatus为<see cref="InvokeStatus.Success"/>。会直接返回结果</para>
    /// </summary>
    /// <param name="callContext">调用上下文，包含有关Rpc调用的信息和上下文</param>
    /// <param name="parameters">Rpc调用的参数，以对象数组的形式提供</param>
    /// <param name="invokeResult">Rpc调用的结果，包含调用状态和结果数据</param>
    /// <returns>返回一个<see cref="Task"/>，该任务完成后将返回Rpc调用的结果<see cref="InvokeResult"/></returns>
    Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult);
}