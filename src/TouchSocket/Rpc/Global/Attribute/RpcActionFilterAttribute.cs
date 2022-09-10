//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcActionFilterAttribute
    /// </summary>
    public abstract class RpcActionFilterAttribute : Attribute, IRpcActionFilter
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="invokeResult"></param>
        public virtual void Executed(ICallContext callContext, ref InvokeResult invokeResult)
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="invokeResult"></param>
        /// <returns></returns>
        public virtual Task ExecutedAsync(ICallContext callContext, ref InvokeResult invokeResult)
        {
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="invokeResult"></param>
        /// <param name="exception"></param>
        public virtual void ExecutException(ICallContext callContext, ref InvokeResult invokeResult, Exception exception)
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="invokeResult"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public virtual Task ExecutExceptionAsync(ICallContext callContext, ref InvokeResult invokeResult, Exception exception)
        {
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="invokeResult"></param>
        public virtual void Executing(ICallContext callContext, ref InvokeResult invokeResult)
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="invokeResult"></param>
        /// <returns></returns>
        public virtual Task ExecutingAsync(ICallContext callContext, ref InvokeResult invokeResult)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
