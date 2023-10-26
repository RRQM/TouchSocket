//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
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
    [PluginOption(Singleton = true, NotRegister = false)]
    public abstract class JsonRpcParserPluginBase : PluginBase, IRpcParser
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRpcParserPluginBase(IContainer container)
        {
            if (container.IsRegistered(typeof(RpcStore)))
            {
                this.RpcStore = container.Resolve<RpcStore>();
            }
            else
            {
                this.RpcStore = new RpcStore(container);
            }

            this.ActionMap = new ActionMap(true);
            this.RpcStore.AddRpcParser(this);
        }

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcStore RpcStore { get; private set; }

        /// <summary>
        /// 处理响应结果。
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        protected abstract void Response(JsonRpcCallContextBase callContext, object result, JsonRpcError error);

        /// <summary>
        /// 调用JsonRpc
        /// </summary>
        protected async Task ThisInvoke(object obj)
        {
            var callContext = (JsonRpcCallContextBase)obj;
            var invokeResult = new InvokeResult();

            try
            {
                JsonRpcUtility.BuildRequestContext(this.ActionMap, ref callContext);
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
            }

            if (callContext.MethodInstance != null)
            {
                if (!callContext.MethodInstance.IsEnable)
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
                var rpcServer = callContext.MethodInstance.ServerFactory.Create(callContext, callContext.JsonRpcContext.Parameters);
                if (rpcServer is ITransientRpcServer transientRpcServer)
                {
                    transientRpcServer.CallContext = callContext;
                }

                invokeResult = await RpcStore.ExecuteAsync(rpcServer, callContext.JsonRpcContext.Parameters, callContext);
            }

            if (!callContext.JsonRpcContext.Id.HasValue)
            {
                return;
            }
            var error = JsonRpcUtility.GetJsonRpcError(invokeResult);
            this.Response(callContext, invokeResult.Result, error);
        }

        #region Rpc解析器

        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Remove(attribute.GetInvokenKey(methodInstance));
                }
            }
        }

        #endregion Rpc解析器
    }
}