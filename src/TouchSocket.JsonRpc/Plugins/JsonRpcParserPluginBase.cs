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
using System.Collections;
using System.Collections.Generic;
using TouchSocket.Core;
using TouchSocket.Http;
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

        #region RPC解析器

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

        #endregion RPC解析器

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
        /// <param name="callContext"></param>
        protected void ThisInvoke(JsonRpcCallContextBase callContext)
        {
            var invokeResult = new InvokeResult();

            try
            {
                this.BuildRequestContext(ref callContext);
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

                invokeResult = RpcStore.Execute(rpcServer, callContext.JsonRpcContext.Parameters, callContext);
            }

            if (!callContext.JsonRpcContext.NeedResponse)
            {
                return;
            }
            JsonRpcError error = null;
            switch (invokeResult.Status)
            {
                case InvokeStatus.Success:
                    {
                        break;
                    }
                case InvokeStatus.UnFound:
                    {
                        error = new JsonRpcError
                        {
                            Code = -32601,
                            Message = "函数未找到"
                        };
                        break;
                    }
                case InvokeStatus.UnEnable:
                    {
                        error = new JsonRpcError
                        {
                            Code = -32601,
                            Message = "函数已被禁用"
                        };
                        break;
                    }
                case InvokeStatus.InvocationException:
                    {
                        error = new JsonRpcError
                        {
                            Code = -32603,
                            Message = "函数内部异常"
                        };
                        break;
                    }
                case InvokeStatus.Exception:
                    {
                        error = new JsonRpcError
                        {
                            Code = -32602,
                            Message = invokeResult.Message
                        };
                        break;
                    }
                default:
                    return;
            }

            this.Response(callContext, invokeResult.Result, error);
        }

        private void BuildRequestContext(ref JsonRpcCallContextBase callContext)
        {
            var jsonRpcContext = SerializeConvert.JsonDeserializeFromString<JsonRpcContext>(callContext.JsonString);
            callContext.JsonRpcContext = jsonRpcContext;
            if (jsonRpcContext.Id != null)
            {
                jsonRpcContext.NeedResponse = true;
            }

            if (this.ActionMap.TryGetMethodInstance(jsonRpcContext.Method, out var methodInstance))
            {
                callContext.MethodInstance = methodInstance;
                if (jsonRpcContext.Params == null)
                {
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        jsonRpcContext.Parameters = methodInstance.ParameterNames.Length > 1 ? throw new RpcException("调用参数计数不匹配") : (new object[] { callContext });
                    }
                    else
                    {
                        if (methodInstance.ParameterNames.Length != 0)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                    }
                }
                if (jsonRpcContext.Params is Dictionary<string, object> obj)
                {
                    jsonRpcContext.Parameters = new object[methodInstance.ParameterNames.Length];
                    //内联
                    var i = 0;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        jsonRpcContext.Parameters[0] = callContext;
                        i = 1;
                    }
                    for (; i < methodInstance.ParameterNames.Length; i++)
                    {
                        if (obj.TryGetValue(methodInstance.ParameterNames[i], out var jToken))
                        {
                            var type = methodInstance.ParameterTypes[i];
                            jsonRpcContext.Parameters[i] = jToken.ToJsonString().FromJsonString(type);
                        }
                        else
                        {
                            if (methodInstance.Parameters[i].HasDefaultValue)
                            {
                                jsonRpcContext.Parameters[i] = methodInstance.Parameters[i].DefaultValue;
                            }
                            else
                            {
                                throw new RpcException("调用参数计数不匹配");
                            }
                        }
                    }
                }
                else
                {
                    var array = (IList)jsonRpcContext.Params;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        if (array.Count != methodInstance.ParameterNames.Length - 1)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcContext.Parameters = new object[methodInstance.ParameterNames.Length];

                        jsonRpcContext.Parameters[0] = callContext;
                        for (var i = 0; i < array.Count; i++)
                        {
                            jsonRpcContext.Parameters[i + 1] = array[i].ToJsonString().FromJsonString(methodInstance.ParameterTypes[i + 1]);
                        }
                    }
                    else
                    {
                        if (array.Count != methodInstance.ParameterNames.Length)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        jsonRpcContext.Parameters = new object[methodInstance.ParameterNames.Length];

                        for (var i = 0; i < array.Count; i++)
                        {
                            jsonRpcContext.Parameters[i] = array[i].ToJsonString().FromJsonString(methodInstance.ParameterTypes[i]);
                        }
                    }
                }
            }
        }
    }
}