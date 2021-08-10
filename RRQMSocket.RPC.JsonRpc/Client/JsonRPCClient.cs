//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System.Text;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// JsonRPC客户端
    /// </summary>
    public class JsonRPCClient : TcpClient, IJsonRPCClient
    {
        private JsonFormatConverter jsonFormatConverter;

        private JsonRpcProtocolType protocolType;

        private RRQMWaitHandle<WaitResult> waitHandle;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JsonRPCClient()
        {
            waitHandle = new RRQMWaitHandle<WaitResult>();
        }
       
        /// <summary>
        /// Json格式化器
        /// </summary>
        public JsonFormatConverter JsonFormatConverter => jsonFormatConverter;
       
        /// <summary>
        /// 协议类型
        /// </summary>
        public JsonRpcProtocolType ProtocolType
        {
            get { return protocolType; }
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
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);

            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                JsonRequestContext requestContext = new JsonRequestContext();
                requestContext.jsonrpc = "2.0";
                requestContext.@params = parameters;
                requestContext.method = method;
                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    requestContext.id = context.Sign.ToString();
                }

                switch (this.protocolType)
                {
                    case JsonRpcProtocolType.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jsonFormatConverter.Serialize(requestContext)));
                            break;
                        }
                    case JsonRpcProtocolType.Http:
                        {
                            HttpRequest httpRequest = new HttpRequest();
                            httpRequest.Method = "POST";
                            httpRequest.FromJson(jsonFormatConverter.Serialize(requestContext));
                            httpRequest.Build(byteBlock);
                        }
                        break;
                }

                this.Send(byteBlock);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        if (resultContext.error != null)
                        {
                            throw new RRQMRPCException(resultContext.error.message);
                        }
                        try
                        {
                            return (T)this.jsonFormatConverter.Deserialize(resultContext.ReturnJsonString, typeof(T));
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                default:
                    return default;
            }
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
            JsonRpcWaitContext context = new JsonRpcWaitContext();
            WaitData<WaitResult> waitData = this.waitHandle.GetWaitData(context);

            ByteBlock byteBlock = this.BytePool.GetByteBlock(this.BufferLength);
            if (invokeOption == null)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            try
            {
                JsonRequestContext requestContext = new JsonRequestContext();
                requestContext.jsonrpc = "2.0";
                requestContext.@params = parameters;
                requestContext.method = method;
                if (invokeOption.FeedbackType == FeedbackType.WaitInvoke)
                {
                    requestContext.id = context.Sign.ToString();
                }
                switch (this.protocolType)
                {
                    case JsonRpcProtocolType.Tcp:
                        {
                            byteBlock.Write(Encoding.UTF8.GetBytes(jsonFormatConverter.Serialize(requestContext)));
                            break;
                        }
                    case JsonRpcProtocolType.Http:
                        {
                            HttpRequest httpRequest = new HttpRequest();
                            httpRequest.Method = "POST";
                            httpRequest.FromJson(jsonFormatConverter.Serialize(requestContext));
                            httpRequest.Build(byteBlock);
                        }
                        break;
                }
                this.Send(byteBlock);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                byteBlock.Dispose();
            }

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        waitData.Wait(invokeOption.WaitTime * 1000);
                        JsonRpcWaitContext resultContext = (JsonRpcWaitContext)waitData.WaitResult;
                        this.waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                        if (resultContext.error != null)
                        {
                            throw new RRQMRPCException(resultContext.error.message);
                        }
                        return;
                    }
                default:
                    return;
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
            switch (this.protocolType)
            {
                case JsonRpcProtocolType.Tcp:
                    {
                        string jsonString = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                        JsonResponseContext responseContext = (JsonResponseContext)this.jsonFormatConverter.Deserialize(jsonString, typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitContext waitContext = new JsonRpcWaitContext();
                            waitContext.Status = 1;
                            waitContext.Sign = int.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.ReturnJsonString = responseContext.result == null ? null : responseContext.result.ToString();
                            this.waitHandle.SetRun(waitContext);
                        }
                        break;
                    }

                case JsonRpcProtocolType.Http:
                    {
                        HttpResponse httpResponse = (HttpResponse)obj;
                        JsonResponseContext responseContext = (JsonResponseContext)this.jsonFormatConverter.Deserialize(httpResponse.Body, typeof(JsonResponseContext));
                        if (responseContext != null)
                        {
                            JsonRpcWaitContext waitContext = new JsonRpcWaitContext();
                            waitContext.Status = 1;
                            waitContext.Sign = int.Parse(responseContext.id);
                            waitContext.error = responseContext.error;
                            waitContext.ReturnJsonString = responseContext.result == null ? null : responseContext.result.ToString();
                            this.waitHandle.SetRun(waitContext);
                        }
                        break;
                    }

            }

        }

        /// <summary>
        /// 载入配置
        /// </summary>
        /// <param name="clientConfig"></param>
        protected override void LoadConfig(TcpClientConfig clientConfig)
        {
            base.LoadConfig(clientConfig);
            this.protocolType = (JsonRpcProtocolType)clientConfig.GetValue(JsonRPCClientConfig.ProtocolTypeProperty);
            switch (this.protocolType)
            {
                case JsonRpcProtocolType.Tcp:
                    this.SetDataHandlingAdapter(new TerminatorDataHandlingAdapter(this.bufferLength, "\r\n"));
                    break;
                case JsonRpcProtocolType.Http:
                    this.SetDataHandlingAdapter(new HttpDataHandlingAdapter(this.bufferLength, HttpType.Client));
                    break;
            }
            this.jsonFormatConverter = (JsonFormatConverter)clientConfig.GetValue(JsonRPCClientConfig.JsonFormatConverterProperty);

        }
    }
}