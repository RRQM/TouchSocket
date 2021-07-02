//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RRQMSocket.RPC.XmlRpc
{
    /// <summary>
    /// XmlRPC客户端
    /// </summary>
    public class XmlRPCClient : TcpClient, IRPCClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public XmlRPCClient()
        {
            singleWaitHandle = new WaitData<HttpResponse>();
        }
        private WaitData<HttpResponse> singleWaitHandle;
        private int timeout;
        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            this.SetDataHandlingAdapter(new HttpDataHandlingAdapter(this.bufferLength, HttpType.Client));
            this.timeout = (int)clientConfig.GetValue(XmlRPCClientConfig.TimeoutProperty);
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
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            HttpResponse response;
            try
            {
                XmlDataTool.CreateRequest(byteBlock, this.Name, method, parameters);
                response = this.WaitSend(byteBlock);
                if (response.StatusCode!="200")
                {
                    throw new RRQMException("调用错误");
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(response.BodyString);
                XmlNode paramNode = xml.SelectSingleNode("methodResponse/params/param");
                if (paramNode != null)
                {
                    return (T)XmlDataTool.GetValue(paramNode.FirstChild.FirstChild, typeof(T));
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            HttpResponse response;
            try
            {
                XmlDataTool.CreateRequest(byteBlock, this.Name, method, parameters);
                response = this.WaitSend(byteBlock);
                if (response.StatusCode != "200")
                {
                    throw new RRQMException("调用错误");
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
        /// 处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            this.singleWaitHandle.Set((HttpResponse)obj);
        }

        private HttpResponse WaitSend(ByteBlock byteBlock)
        {
            lock (locker)
            {
                this.Send(byteBlock.Buffer, 0, (int)byteBlock.Length);
                if (this.singleWaitHandle.Wait(1000 * this.timeout))
                {
                    return this.singleWaitHandle.WaitResult;
                }
                throw new RRQMTimeoutException("超时接收");
            }
        }
    }
}
