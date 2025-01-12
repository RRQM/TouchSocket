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
        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            invokeOption ??= InvokeOption.WaitInvoke;

            using (var byteBlock = new ByteBlock())
            {
                var request = XmlDataTool.CreateRequest(this, invokeKey, parameters);

                using (var responseResult = await this.ProtectedRequestContentAsync(request, invokeOption.Timeout, invokeOption.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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

                        if (returnType != null)
                        {
                            var xml = new XmlDocument();
                            xml.LoadXml(await response.GetBodyAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext));
                            var paramNode = xml.SelectSingleNode("methodResponse/params/param");
                            return paramNode != null ? XmlDataTool.GetValue(paramNode.FirstChild.FirstChild, returnType) : default;
                        }

                        return default;
                    }
                }
            }
        }
    }
}