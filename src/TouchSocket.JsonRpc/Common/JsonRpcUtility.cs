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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcUtility
    /// </summary>
    public static class JsonRpcUtility
    {
        /// <summary>
        /// 是否属于JsonRpc请求
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static bool IsJsonRpcRequest(string jsonString)
        {
            if (jsonString.Contains("error") || jsonString.Contains("result"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ToJsonRpcWaitResult
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static JsonRpcWaitResult ToJsonRpcWaitResult(string jsonString)
        {
#if NET6_0_OR_GREATER
            return jsonString.FromJsonString<JsonRpcWaitResult>();
            return (JsonRpcWaitResult)System.Text.Json.JsonSerializer.Deserialize(jsonString, typeof(JsonRpcWaitResult), TouchSokcetJsonRpcSourceGenerationContext.Default);
#else
            return jsonString.FromJsonString<JsonRpcWaitResult>();
#endif
        }

        /// <summary>
        /// ToJsonRpcRequestContext
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static JsonRpcRequestContext ToJsonRpcRequestContext(string jsonString)
        {
#if NET6_0_OR_GREATER
            return jsonString.FromJsonString<JsonRpcRequestContext>();
            //return (JsonRpcRequestContext)System.Text.Json.JsonSerializer.Deserialize(jsonString, typeof(JsonRpcRequestContext), TouchSokcetJsonRpcSourceGenerationContext.Default);
#else
            return jsonString.FromJsonString<JsonRpcRequestContext>();
#endif
        }

        /// <summary>
        /// ToJsonRpcResponseString
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string ToJsonRpcResponseString(JsonRpcResponseBase response)
        {
#if NET6_0_OR_GREATER
            return JsonConvert.SerializeObject(response);
            return System.Text.Json.JsonSerializer.Serialize(response);
#else
            return JsonConvert.SerializeObject(response);
#endif
        }

        /// <summary>
        /// ResultParseToType
        /// </summary>
        /// <param name="result"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static object ResultParseToType(object result, Type returnType)
        {
            if (returnType.IsPrimitive || returnType == typeof(string))
            {
                return result.ToString().ParseToType(returnType);
            }
            else
            {
                return result.ToJsonString().FromJsonString(returnType);
            }
        }

        /// <summary>
        /// BuildRequestContext
        /// </summary>
        /// <param name="actionMap"></param>
        /// <param name="callContext"></param>
        /// <exception cref="RpcException"></exception>
        public static void BuildRequestContext(ActionMap actionMap, ref JsonRpcCallContextBase callContext)
        {
            var requestContext = JsonRpcUtility.ToJsonRpcRequestContext(callContext.JsonString);

            callContext.JsonRpcContext = requestContext;

            if (actionMap.TryGetMethodInstance(requestContext.Method, out var methodInstance))
            {
                callContext.MethodInstance = methodInstance;
                if (requestContext.Params == null)
                {
                    if (methodInstance.IncludeCallContext)
                    {
                        requestContext.Parameters = methodInstance.ParameterNames.Length > 1 ? throw new RpcException("调用参数计数不匹配") : (new object[] { callContext });
                    }
                    else
                    {
                        if (methodInstance.ParameterNames.Length != 0)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                    }
                }
                if (requestContext.Params is JObject obj)
                {
                    requestContext.Parameters = new object[methodInstance.ParameterNames.Length];
                    //内联
                    var i = 0;
                    if (methodInstance.IncludeCallContext)
                    {
                        requestContext.Parameters[0] = callContext;
                        i = 1;
                    }
                    for (; i < methodInstance.ParameterNames.Length; i++)
                    {
                        if (obj.TryGetValue(methodInstance.ParameterNames[i], out var jToken))
                        {
                            var type = methodInstance.ParameterTypes[i];
                            requestContext.Parameters[i] = jToken.ToJsonString().FromJsonString(type);
                        }
                        else
                        {
                            if (methodInstance.Parameters[i].HasDefaultValue)
                            {
                                requestContext.Parameters[i] = methodInstance.Parameters[i].DefaultValue;
                            }
                            else
                            {
                                throw new RpcException("调用参数计数不匹配");
                            }
                        }
                    }
                }
                else if (requestContext.Params is JArray array)
                {
                    if (methodInstance.IncludeCallContext)
                    {
                        if (array.Count != methodInstance.ParameterNames.Length - 1)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        requestContext.Parameters = new object[methodInstance.ParameterNames.Length];

                        requestContext.Parameters[0] = callContext;
                        for (var i = 0; i < array.Count; i++)
                        {
                            requestContext.Parameters[i + 1] = array[i].ToJsonString().FromJsonString(methodInstance.ParameterTypes[i + 1]);
                        }
                    }
                    else
                    {
                        if (array.Count != methodInstance.ParameterNames.Length)
                        {
                            throw new RpcException("调用参数计数不匹配");
                        }
                        requestContext.Parameters = new object[methodInstance.ParameterNames.Length];

                        for (var i = 0; i < array.Count; i++)
                        {
                            requestContext.Parameters[i] = array[i].ToJsonString().FromJsonString(methodInstance.ParameterTypes[i]);
                        }
                    }
                }
                else
                {
                    throw new RpcException("未知参数类型");
                }
            }
        }

        /// <summary>
        /// GetJsonRpcError
        /// </summary>
        /// <param name="invokeResult"></param>
        /// <returns></returns>
        public static JsonRpcError GetJsonRpcError(InvokeResult invokeResult)
        {
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
                    return default;
            }

            return error;
        }
    }
}