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
using TouchSocket.Core.ByteManager;
using TouchSocket.Http;
using TouchSocket.Rpc.TouchRpc;

namespace TouchSocket.Rpc.XmlRpc
{
    /// <summary>
    /// XmlRpc客户端
    /// </summary>
    public class XmlRpcClient : HttpClientBase, IXmlRpcClient
    {
        private readonly object m_invokeLocker = new object();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<IRpcClient, bool> TryCanInvoke { get; set; }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            lock (this.m_invokeLocker)
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                using (ByteBlock byteBlock = new ByteBlock(this.BufferLength))
                {
                    HttpRequest request = XmlDataTool.CreateRequest(this.RemoteIPHost.Host, this.RemoteIPHost.GetUrlPath(), method, parameters);

                    HttpResponse response = this.RequestContent(request, invokeOption.FeedbackType == FeedbackType.OnlySend, invokeOption.Timeout, invokeOption.Token);
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
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(response.GetBody());
                        XmlNode paramNode = xml.SelectSingleNode("methodResponse/params/param");
                        if (paramNode != null)
                        {
                            return (T)XmlDataTool.GetValue(paramNode.FirstChild.FirstChild, typeof(T));
                        }
                        return default;
                    }
                }
            }
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            lock (this.m_invokeLocker)
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }

                using (ByteBlock byteBlock = new ByteBlock(this.BufferLength))
                {
                    HttpRequest request = XmlDataTool.CreateRequest(this.RemoteIPHost.Host, this.RemoteIPHost.GetUrlPath(), method, parameters);
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

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke<T>(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="RpcSerializationException"></exception>
        /// <exception cref="RpcInvokeException"></exception>
        /// <exception cref="Exception"></exception>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                this.Invoke(method, invokeOption, parameters);
            });
        }

        /// <summary>
        /// 函数式调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return Task.Run(() =>
            {
                return this.Invoke<T>(method, invokeOption, parameters);
            });
        }
    }
}