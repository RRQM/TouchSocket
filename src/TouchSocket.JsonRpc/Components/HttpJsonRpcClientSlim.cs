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

//#if SystemNetHttp
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using TouchSocket.Core;
//using TouchSocket.Rpc;

//namespace TouchSocket.JsonRpc
//{
//    /// <summary>
//    /// 基于<see cref="System.Net.Http.HttpClient"/>通讯模型的JsonRpc客户端
//    /// </summary>
//    public class HttpJsonRpcClientSlim : Http.HttpClientSlim, IHttpJsonRpcClientSlim
//    {
//        /// <summary>
//        /// 基于<see cref="System.Net.Http.HttpClient"/>通讯模型的JsonRpc客户端
//        /// </summary>
//        /// <param name="httpClient"></param>
//        public HttpJsonRpcClientSlim(System.Net.Http.HttpClient httpClient = default) : base(httpClient)
//        {
//        }

//        private readonly WaitHandlePool<IWaitResult> m_waitHandle = new WaitHandlePool<IWaitResult>();

//        /// <inheritdoc/>
//        public object Invoke(Type returnType, string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
//        {
//            var context = new JsonRpcWaitResult();
//            var waitData = this.m_waitHandle.GetWaitData(context);

//            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
//            {
//                if (invokeOption == default)
//                {
//                    invokeOption = InvokeOption.WaitInvoke;
//                }

//                parameters ??= new object[0];
//                var jsonRpcRequest = new JsonRpcRequest
//                {
//                    Method = method,
//                    Params = parameters,
//                    Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null
//                };
//                var request = new HttpRequestMessage();
//                request.Method = HttpMethod.Post;
//                request.RequestUri = (this.RemoteIPHost.PathAndQuery);
//                request.FromJson(jsonRpcRequest.ToJson());
//                request.Build(byteBlock);
//                switch (invokeOption.FeedbackType)
//                {
//                    case FeedbackType.OnlySend:
//                        {
//                            this.HttpClient.po(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return default;
//                        }
//                    case FeedbackType.WaitSend:
//                        {
//                            this.Send(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return default;
//                        }
//                    case FeedbackType.WaitInvoke:
//                        {
//                            this.Send(byteBlock);
//                            waitData.Wait(invokeOption.Timeout);
//                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
//                            this.m_waitHandle.Destroy(waitData);

//                            if (resultContext.Status == 0)
//                            {
//                                throw new TimeoutException("等待结果超时");
//                            }
//                            if (resultContext.Error != null)
//                            {
//                                throw new RpcException(resultContext.Error.Message);
//                            }

//                            if (resultContext.Return == null)
//                            {
//                                return default;
//                            }
//                            else
//                            {
//                                if (returnType.IsPrimitive || returnType == typeof(string))
//                                {
//                                    return resultContext.Return.ToString().ParseToType(returnType);
//                                }
//                                else
//                                {
//                                    return resultContext.Return.ToJson().FromJson(returnType);
//                                }
//                            }
//                        }
//                    default:
//                        return default;
//                }
//            }
//        }

//        /// <inheritdoc/>
//        public void Invoke(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
//        {
//            var context = new JsonRpcWaitResult();
//            var waitData = this.m_waitHandle.GetWaitData(context);

//            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
//            {
//                if (invokeOption == default)
//                {
//                    invokeOption = InvokeOption.WaitInvoke;
//                }
//                parameters ??= new object[0];
//                var jsonRpcRequest = new JsonRpcRequest()
//                {
//                    Method = method,
//                    Params = parameters
//                };

//                jsonRpcRequest.Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null;
//                var request = new HttpRequest();
//                request.Method = HttpMethod.Post;
//                request.SetUrl(this.RemoteIPHost.PathAndQuery);
//                request.FromJson(jsonRpcRequest.ToJson());
//                request.Build(byteBlock);
//                switch (invokeOption.FeedbackType)
//                {
//                    case FeedbackType.OnlySend:
//                        {
//                            this.Send(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return;
//                        }
//                    case FeedbackType.WaitSend:
//                        {
//                            this.Send(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return;
//                        }
//                    case FeedbackType.WaitInvoke:
//                        {
//                            this.Send(byteBlock);
//                            waitData.Wait(invokeOption.Timeout);
//                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
//                            this.m_waitHandle.Destroy(waitData);

//                            if (resultContext.Status == 0)
//                            {
//                                throw new TimeoutException("等待结果超时");
//                            }
//                            if (resultContext.Error != null)
//                            {
//                                throw new RpcException(resultContext.Error.Message);
//                            }
//                            break;
//                        }
//                    default:
//                        return;
//                }
//            }
//        }

//        /// <inheritdoc/>
//        public void Invoke(string method, InvokeOption invokeOption, params object[] parameters)
//        {
//            this.Invoke(method, invokeOption, ref parameters, null);
//        }

//        /// <inheritdoc/>
//        public object Invoke(Type returnType, string method, InvokeOption invokeOption, params object[] parameters)
//        {
//            return this.Invoke(returnType, method, invokeOption, ref parameters, null);
//        }

//        /// <inheritdoc/>
//        public async Task InvokeAsync(string method, InvokeOption invokeOption, params object[] parameters)
//        {
//            var context = new JsonRpcWaitResult();
//            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

//            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
//            {
//                if (invokeOption == default)
//                {
//                    invokeOption = InvokeOption.WaitInvoke;
//                }
//                parameters ??= new object[0];
//                var jsonRpcRequest = new JsonRpcRequest()
//                {
//                    Method = method,
//                    Params = parameters
//                };

//                jsonRpcRequest.Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null;
//                var request = new HttpRequest();
//                request.Method = HttpMethod.Post;
//                request.SetUrl(this.RemoteIPHost.PathAndQuery);
//                request.FromJson(jsonRpcRequest.ToJson());
//                request.Build(byteBlock);
//                switch (invokeOption.FeedbackType)
//                {
//                    case FeedbackType.OnlySend:
//                        {
//                            this.Send(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return;
//                        }
//                    case FeedbackType.WaitSend:
//                        {
//                            this.Send(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return;
//                        }
//                    case FeedbackType.WaitInvoke:
//                        {
//                            this.Send(byteBlock);
//                            await waitData.WaitAsync(invokeOption.Timeout);
//                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
//                            this.m_waitHandle.Destroy(waitData);

//                            if (resultContext.Status == 0)
//                            {
//                                throw new TimeoutException("等待结果超时");
//                            }
//                            if (resultContext.Error != null)
//                            {
//                                throw new RpcException(resultContext.Error.Message);
//                            }
//                            break;
//                        }
//                    default:
//                        return;
//                }
//            }
//        }

//        /// <inheritdoc/>
//        public async Task<object> InvokeAsync(Type returnType, string method, InvokeOption invokeOption, params object[] parameters)
//        {
//            var context = new JsonRpcWaitResult();
//            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

//            using (var byteBlock = BytePool.Default.GetByteBlock(this.BufferLength))
//            {
//                if (invokeOption == default)
//                {
//                    invokeOption = InvokeOption.WaitInvoke;
//                }
//                parameters ??= new object[0];
//                var jsonRpcRequest = new JsonRpcRequest
//                {
//                    Method = method,
//                    Params = parameters,
//                    Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null
//                };
//                var request = new HttpRequest();
//                request.Method = HttpMethod.Post;
//                request.SetUrl(this.RemoteIPHost.PathAndQuery);
//                request.FromJson(jsonRpcRequest.ToJson());
//                request.Build(byteBlock);
//                switch (invokeOption.FeedbackType)
//                {
//                    case FeedbackType.OnlySend:
//                        {
//                            this.Send(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return default;
//                        }
//                    case FeedbackType.WaitSend:
//                        {
//                            this.Send(byteBlock);
//                            this.m_waitHandle.Destroy(waitData);
//                            return default;
//                        }
//                    case FeedbackType.WaitInvoke:
//                        {
//                            this.Send(byteBlock);
//                            await waitData.WaitAsync(invokeOption.Timeout);
//                            var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
//                            this.m_waitHandle.Destroy(waitData);

//                            if (resultContext.Status == 0)
//                            {
//                                throw new TimeoutException("等待结果超时");
//                            }
//                            if (resultContext.Error != null)
//                            {
//                                throw new RpcException(resultContext.Error.Message);
//                            }

//                            if (resultContext.Return == null)
//                            {
//                                return default;
//                            }
//                            else
//                            {
//                                if (returnType.IsPrimitive || returnType == typeof(string))
//                                {
//                                    return resultContext.Return.ToString().ParseToType(returnType);
//                                }
//                                else
//                                {
//                                    return resultContext.Return.ToJson().FromJson(returnType);
//                                }
//                            }
//                        }
//                    default:
//                        return default;
//                }
//            }
//        }

//    }
//}

//#endif