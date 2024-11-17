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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Http协议的JsonRpc客户端
    /// </summary>
    public class HttpJsonRpcClient : HttpClientBase, IHttpJsonRpcClient
    {
        private int m_idCount;

        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            invokeOption ??= InvokeOption.WaitInvoke;
            parameters ??= new object[0];

            var id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? Interlocked.Increment(ref this.m_idCount) : 0;

            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = invokeKey,
                Params = parameters,
                Id = id
            };
            var request = new HttpRequest();
            request.Method = HttpMethod.Post;
            request.SetUrl(this.RemoteIPHost.PathAndQuery);
            request.FromJson(jsonRpcRequest.ToJsonString());

            using (var responseResult = await base.ProtectedRequestAsync(request).ConfigureAwait(false))
            {
                var response = responseResult.Response;

                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            var responseJsonString = await response.GetBodyAsync().ConfigureAwait(false);

                            ThrowHelper.ThrowArgumentNullExceptionIf(responseJsonString, nameof(responseJsonString));

                            var resultContext = JsonRpcUtility.ToJsonRpcWaitResult(responseJsonString);

                            if (resultContext.Error != null)
                            {
                                ThrowHelper.ThrowRpcException(resultContext.Error.Message);
                            }

                            if (resultContext.Result == null)
                            {
                                return default;
                            }
                            else
                            {
                                if (returnType != null)
                                {
                                    if (returnType.IsPrimitive || returnType == typeof(string))
                                    {
                                        return resultContext.Result.ToString().ParseToType(returnType);
                                    }
                                    else
                                    {
                                        return resultContext.Result.ToJsonString().FromJsonString(returnType);
                                    }
                                }
                                else
                                {
                                    return default;
                                }
                            }
                        }
                    default:
                        return default;
                }
            }
        }
    }
}