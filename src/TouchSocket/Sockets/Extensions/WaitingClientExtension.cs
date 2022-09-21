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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Run;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 等待设置
    /// </summary>
    public enum WaitingOptions
    {
        /// <summary>
        /// 发送和接收都经过适配器
        /// </summary>
        AllAdapter,

        /// <summary>
        /// 发送经过适配器，接收不经过
        /// </summary>
        SendAdapter,

        /// <summary>
        /// 发送不经过适配器，接收经过
        /// </summary>
        WaitAdapter,

        /// <summary>
        /// 全都不经过适配器。
        /// </summary>
        NoneAll
    }

    /// <summary>
    /// 等待型客户端。
    /// </summary>
    public interface IWaitingClient<TClient> : IWaitSender where TClient : IClient, IDefaultSender, ISender
    {
        /// <summary>
        /// 等待设置。
        /// </summary>
        public WaitingOptions WaitingOptions { get; set; }

        /// <summary>
        /// 客户端终端
        /// </summary>
        TClient Client { get; }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        ResponsedData SendThenResponse(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        ResponsedData SendThenResponse(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        ResponsedData SendThenResponse(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<ResponsedData> SendThenResponseAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default);
    }

    /// <summary>
    /// 响应数据。
    /// </summary>
    public struct ResponsedData
    {
        private readonly byte[] m_data;
        private readonly IRequestInfo m_requestInfo;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="requestInfo"></param>
        public ResponsedData(byte[] data, IRequestInfo requestInfo)
        {
            this.m_data = data;
            this.m_requestInfo = requestInfo;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data => this.m_data;

        /// <summary>
        /// RequestInfo
        /// </summary>
        public IRequestInfo RequestInfo => this.m_requestInfo;
    }

    /// <summary>
    /// WaitingClientExtensions
    /// </summary>
    public static class WaitingClientExtension
    {
        /// <summary>
        /// WaitingClient
        /// </summary>
        public static readonly DependencyProperty WaitingClientProperty =
            DependencyProperty.Register("WaitingClient", typeof(object), typeof(WaitingClientExtension), null);

        /// <summary>
        /// 获取可等待的客户端。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="waitingOptions"></param>
        /// <returns></returns>
        public static IWaitingClient<TClient> GetWaitingClient<TClient>(this TClient client, WaitingOptions waitingOptions = WaitingOptions.AllAdapter) where TClient : IClient, IDefaultSender, ISender
        {
            if (client.GetValue<IWaitingClient<TClient>>(WaitingClientProperty) is IWaitingClient<TClient> c1)
            {
                c1.WaitingOptions = waitingOptions;
                return c1;
            }

            WaitingClient<TClient> waitingClient = new WaitingClient<TClient>(client, waitingOptions);
            client.SetValue(WaitingClientProperty, waitingClient);
            return waitingClient;
        }
    }

    internal class WaitingClient<TClient> : IWaitingClient<TClient> where TClient : IClient, IDefaultSender, ISender
    {
        private readonly TClient m_client;

        private readonly WaitData<ResponsedData> m_waitData;

        private WaitingOptions m_waitingOptions;

        public WaitingClient(TClient client, WaitingOptions waitingOptions)
        {
            this.m_client = client ?? throw new ArgumentNullException(nameof(client));
            this.m_waitData = new WaitData<ResponsedData>();
            this.m_waitingOptions = waitingOptions;
        }

        public bool CanSend
        {
            get
            {
                if (this.m_client is ITcpClientBase tcpClient)
                {
                    return tcpClient.Online;
                }
                else if (this.m_client is IUdpSession)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public TClient Client => this.m_client;

        public WaitingOptions WaitingOptions { get => this.m_waitingOptions; set => this.m_waitingOptions = value; }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public ResponsedData SendThenResponse(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            lock (this)
            {
                try
                {
                    this.m_waitData.Reset();

                    if (this.m_waitingOptions == WaitingOptions.AllAdapter || this.m_waitingOptions == WaitingOptions.WaitAdapter)
                    {
                        this.m_client.OnHandleReceivedData += this.OnHandleReceivedData;
                    }
                    else
                    {
                        this.m_client.OnHandleRawBuffer += this.OnHandleRawBuffer;
                    }

                    if (this.m_waitingOptions == WaitingOptions.AllAdapter || this.m_waitingOptions == WaitingOptions.SendAdapter)
                    {
                        this.m_client.Send(buffer, offset, length);
                    }
                    else
                    {
                        this.m_client.DefaultSend(buffer, offset, length);
                    }

                    this.m_waitData.SetCancellationToken(token);
                    switch (this.m_waitData.Wait(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.m_waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException();
                        case WaitDataStatus.Canceled:
                            {
                                return default;
                            }
                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketRes.UnknownError.GetDescription());
                    }
                }
                finally
                {
                    if (this.m_waitingOptions == WaitingOptions.AllAdapter || this.m_waitingOptions == WaitingOptions.WaitAdapter)
                    {
                        this.m_client.OnHandleReceivedData -= this.OnHandleReceivedData;
                    }
                    else
                    {
                        this.m_client.OnHandleRawBuffer -= this.OnHandleRawBuffer;
                    }
                }
            }
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public ResponsedData SendThenResponse(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(buffer, 0, buffer.Length, timeout, token);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public ResponsedData SendThenResponse(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenResponse(buffer, offset, length, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenResponse(buffer, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<ResponsedData> SendThenResponseAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenResponse(byteBlock, timeout, token);
            });
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(buffer, offset, length, timeout, token).Data;
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenReturn(buffer, 0, buffer.Length, timeout, token);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenReturn(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenReturn(buffer, offset, length, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenReturn(buffer, timeout, token);
            });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenReturn(byteBlock, timeout, token);
            });
        }

        private bool OnHandleRawBuffer(ByteBlock byteBlock)
        {
            ResponsedData responsedData = new ResponsedData(byteBlock.ToArray(), null);
            return !this.m_waitData.Set(responsedData);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        private bool OnHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            ResponsedData responsedData;
            if (byteBlock != null)
            {
                responsedData = new ResponsedData(byteBlock.ToArray(), requestInfo);
            }
            else
            {
                responsedData = new ResponsedData(null, requestInfo);
            }

            return !this.m_waitData.Set(responsedData);
        }
    }
}