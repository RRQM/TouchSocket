using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
