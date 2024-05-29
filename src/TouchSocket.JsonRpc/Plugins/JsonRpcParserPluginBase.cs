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
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcParser解析器插件
    /// </summary>
    [PluginOption(Singleton = true)]
    public abstract class JsonRpcParserPluginBase : PluginBase
    {
        private readonly IRpcServerProvider m_rpcServerProvider;
        private readonly IResolver m_resolver;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParserPluginBase(IRpcServerProvider rpcServerProvider, IResolver resolver)
        {
            this.ActionMap = new ActionMap(true);
            this.RegisterServer(rpcServerProvider.GetMethods());
            this.m_rpcServerProvider = rpcServerProvider;
            this.m_resolver = resolver;
        }

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap { get; private set; }

        /// <summary>
        /// 处理响应结果。
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        protected abstract Task ResponseAsync(JsonRpcCallContextBase callContext, object result, JsonRpcError error);

        /// <summary>
        /// 调用JsonRpc
        /// </summary>
        protected async Task ThisInvokeAsync(object obj)
        {
            var callContext = (JsonRpcCallContextBase)obj;
            var invokeResult = new InvokeResult();

            try
            {
                JsonRpcUtility.BuildRequestContext(this.m_resolver, this.ActionMap, ref callContext);
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
            }

            if (callContext.RpcMethod != null)
            {
                if (!callContext.RpcMethod.IsEnable)
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
            }
            else
            {
                invokeResult.Status = InvokeStatus.UnFound;
            }

            if (invokeResult.Status == InvokeStatus.Ready)
            {
                invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, callContext.JsonRpcContext.Parameters).ConfigureFalseAwait();
            }

            if (!callContext.JsonRpcContext.Id.HasValue)
            {
                return;
            }
            var error = JsonRpcUtility.GetJsonRpcError(invokeResult);
            await this.ResponseAsync(callContext, invokeResult.Result, error).ConfigureFalseAwait();
        }

        private void RegisterServer(RpcMethod[] rpcMethods)
        {
            foreach (var rpcMethod in rpcMethods)
            {
                if (rpcMethod.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(rpcMethod), rpcMethod);
                }
            }
        }
    }
}