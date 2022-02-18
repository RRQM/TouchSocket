//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Run;
using RRQMSocket.Http;
using RRQMSocket.RPC.RRQMRPC;
using System;
using System.Xml;

namespace RRQMSocket.RPC.XmlRpc
{
    /// <summary>
    /// XmlRpc客户端
    /// </summary>
    public class XmlRpcClient : TcpClient, IRpcClient
    {
        private int maxPackageSize;

        private WaitData<HttpResponse> singleWaitHandle;

        private int timeout;

        /// <summary>
        /// 构造函数
        /// </summary>
        public XmlRpcClient()
        {
            singleWaitHandle = new WaitData<HttpResponse>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSetDataHandlingAdapter => false;

        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public int MaxPackageSize
        {
            get { return maxPackageSize; }
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            HttpResponse response;
            try
            {
                XmlDataTool.CreateRequest(byteBlock, this.Name, method, parameters);
                response = this.WaitSend(byteBlock);
                if (response.StatusCode != "200")
                {
                    throw new RRQMException(response.StatusMessage);
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(response.Body);
                XmlNode paramNode = xml.SelectSingleNode("methodResponse/params/param");
                if (paramNode != null)
                {
                    return (T)XmlDataTool.GetValue(paramNode.FirstChild.FirstChild, typeof(T));
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
            return default;
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, InvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            ByteBlock byteBlock = BytePool.GetByteBlock(this.BufferLength);
            HttpResponse response;
            try
            {
                XmlDataTool.CreateRequest(byteBlock, this.Name, method, parameters);
                response = this.WaitSend(byteBlock);
                if (response.StatusCode != "200")
                {
                    throw new RRQMException(response.StatusMessage);
                }
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        public void Invoke(string method, InvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="RRQMTimeoutException"></exception>
        /// <exception cref="RRQMRPCInvokeException"></exception>
        /// <exception cref="RRQMException"></exception>
        /// <returns></returns>
        public T Invoke<T>(string method, InvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke<T>(method, invokeOption, ref parameters, null);
        }

        /// <summary>
        /// 禁用适配器赋值
        /// </summary>
        /// <param name="adapter"></param>
        public override sealed void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            throw new RRQMException($"{nameof(XmlRpcSocketClient)}不允许设置适配器。");
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.singleWaitHandle.Set((HttpResponse)requestInfo);
        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            this.timeout = (int)clientConfig.GetValue(XmlRpcClientConfig.TimeoutProperty);
            this.maxPackageSize = (int)clientConfig.GetValue(XmlRpcClientConfig.MaxPackageSizeProperty);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ClientConnectingEventArgs e)
        {
            base.SetAdapter(new HttpDataHandlingAdapter(this.maxPackageSize, HttpType.Client));
            base.OnConnecting(e);
        }

        private HttpResponse WaitSend(ByteBlock byteBlock)
        {
            lock (this)
            {
                this.Send(byteBlock.Buffer, 0, byteBlock.Len);
                if (this.singleWaitHandle.Wait(1000 * this.timeout) == WaitDataStatus.SetRunning)
                {
                    return this.singleWaitHandle.WaitResult;
                }
                throw new RRQMTimeoutException("超时接收");
            }
        }
    }
}