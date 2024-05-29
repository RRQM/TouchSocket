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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.XmlRpc
{
    /// <summary>
    /// XmlRpc客户端
    /// </summary>
    public class XmlRpcClient : HttpClientBase, IXmlRpcClient
    {
        private readonly object m_invokeLocker = new object();

        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }

        ///// <inheritdoc/>
        //public object Invoke(Type returnType, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        //{
        //    lock (this.m_invokeLocker)
        //    {
        //        if (invokeOption == default)
        //        {
        //            invokeOption = InvokeOption.WaitInvoke;
        //        }
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            var request = XmlDataTool.CreateRequest(this, method, parameters);

        //            using (var responseResult = this.ProtectedRequestContentAsync(request, invokeOption.Timeout, invokeOption.Token).GetFalseAwaitResult())
        //            {
        //                var response = responseResult.Response;

        //                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
        //                {
        //                    return default;
        //                }

        //                if (response.StatusCode != 200)
        //                {
        //                    throw new Exception(response.StatusMessage);
        //                }
        //                else
        //                {
        //                    var xml = new XmlDocument();
        //                    xml.LoadXml(response.GetBody());
        //                    var paramNode = xml.SelectSingleNode("methodResponse/params/param");
        //                    return paramNode != null ? XmlDataTool.GetValue(paramNode.FirstChild.FirstChild, returnType) : default;
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <inheritdoc/>
        //public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        //{
        //    lock (this.m_invokeLocker)
        //    {
        //        if (invokeOption == default)
        //        {
        //            invokeOption = InvokeOption.WaitInvoke;
        //        }

        //        using (var byteBlock = new ByteBlock())
        //        {
        //            var request = XmlDataTool.CreateRequest(this, method, parameters);
        //            using (var responseResult = this.ProtectedRequestContentAsync(request, invokeOption.Timeout, invokeOption.Token).GetFalseAwaitResult())
        //            {
        //                var response = responseResult.Response;
        //                if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
        //                {
        //                    return;
        //                }
        //                if (response.StatusCode != 200)
        //                {
        //                    throw new Exception(response.StatusMessage);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <inheritdoc/>
        //public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    this.Invoke(method, invokeOption, ref parameters, null);
        //}

        ///// <inheritdoc/>
        //public object Invoke(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    return this.Invoke(returnType, method, invokeOption, ref parameters, null);
        //}

        ///// <inheritdoc/>
        //public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    return Task.Run(() => this.Invoke(method, invokeOption, parameters));
        //}

        ///// <inheritdoc/>
        //public Task<object> InvokeAsync(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        //{
        //    return Task.Run(() => this.Invoke(returnType, method, invokeOption, parameters));
        //}

        public async Task<RpcResponse> InvokeAsync(RpcRequest rpcRequest)
        {
            var invokeOption = rpcRequest.InvokeOption ?? InvokeOption.WaitInvoke;

            using (var byteBlock = new ByteBlock())
            {
                var request = XmlDataTool.CreateRequest(this, rpcRequest.InvokeKey, rpcRequest.Parameters);

                using (var responseResult = await this.ProtectedRequestContentAsync(request, invokeOption.Timeout, invokeOption.Token).ConfigureFalseAwait())
                {
                    var response = responseResult.Response;

                    if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                    {
                        return default;
                    }

                    if (response.StatusCode != 200)
                    {
                        throw new Exception(response.StatusMessage);
                    }
                    else
                    {
                       
                        if (rpcRequest.HasReturn)
                        {
                            var xml = new XmlDocument();
                            xml.LoadXml(response.GetBody());
                            var paramNode = xml.SelectSingleNode("methodResponse/params/param");
                            return new RpcResponse(paramNode != null ? XmlDataTool.GetValue(paramNode.FirstChild.FirstChild, rpcRequest.ReturnType) : default);
                        }

                        return new RpcResponse();
                    }
                }
            }
        }
    }
}