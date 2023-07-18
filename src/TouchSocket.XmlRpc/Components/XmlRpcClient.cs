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
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            lock (this.m_invokeLocker)
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                using (var byteBlock = new ByteBlock(this.BufferLength))
                {
                    var request = XmlDataTool.CreateRequest(this.RemoteIPHost.Host, this.RemoteIPHost.PathAndQuery, method, parameters);

                    var response = this.RequestContent(request, invokeOption.FeedbackType == FeedbackType.OnlySend, invokeOption.Timeout, invokeOption.Token);
                    if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                    {
                        return default;
                    }

                    if (response.StatusCode != "200")
                    {
                        throw new Exception(response.StatusMessage);
                    }
                    else
                    {
                        var xml = new XmlDocument();
                        xml.LoadXml(response.GetBody());
                        var paramNode = xml.SelectSingleNode("methodResponse/params/param");
                        return paramNode != null ? XmlDataTool.GetValue(paramNode.FirstChild.FirstChild, returnType) : default;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            lock (this.m_invokeLocker)
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }

                using (var byteBlock = new ByteBlock(this.BufferLength))
                {
                    var request = XmlDataTool.CreateRequest(this.RemoteIPHost.Host, this.RemoteIPHost.PathAndQuery, method, parameters);
                    var response = this.RequestContent(request, invokeOption.FeedbackType == FeedbackType.OnlySend, invokeOption.Timeout, invokeOption.Token);
                    if (invokeOption.FeedbackType != FeedbackType.WaitInvoke)
                    {
                        return;
                    }
                    if (response.StatusCode != "200")
                    {
                        throw new Exception(response.StatusMessage);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke(returnType, method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        /// <inheritdoc/>
        public Task<object> InvokeAsync(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke(returnType, method, invokeOption, parameters);
            });
        }
    }
}